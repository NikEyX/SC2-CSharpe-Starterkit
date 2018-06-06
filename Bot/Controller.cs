using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Xml.Schema;
using Google.Protobuf.Reflection;
using SC2APIProtocol;

namespace Bot {
    
    public class Controller {
        //editable
        private int frameDelay = 0; 
        
        //don't edit
        private ResponseObservation obs;
        private List<SC2APIProtocol.Action> actions;
        private ResponseGameInfo gameInfo;
        private static Random random = new Random();
        private double FRAMES_PER_SECOND = 22.4;        
        
        public ulong frame = 0;        
        public uint currentSupply = 0;
        public uint maxSupply = 0;
        public uint minerals = 0;
        public uint vespene = 0;
        
        public List<Vector3> enemyLocations = new List<Vector3>(); 
        public List<string> chatLog = new List<string>(); 

        public class UnitsHolder {
            public List<Unit> workers = new List<Unit>();
            public List<Unit> army = new List<Unit>();
            public List<Unit> barracks = new List<Unit>();
            public List<Unit> depots = new List<Unit>();
            public List<Unit> buildings = new List<Unit>();
            public List<Unit> resourceCenters = new List<Unit>();
        }

        public UnitsHolder units = new UnitsHolder();

        public Controller(int wait) {
            Logger.Info("Instantiated Controller");
            this.frameDelay = wait;
        }

        
        public void Pause() {
            Console.WriteLine("Press any key to continue...");
            while (Console.ReadKey().Key != ConsoleKey.Enter) {
                //do nothing
            }
        }

        public List<SC2APIProtocol.Action> CloseFrame() {
            return actions;
        }
        
        public ulong SecsToFrames(int seconds) {
            return (ulong) (FRAMES_PER_SECOND * seconds);
        }

        private void PopulateInventory() {
            this.units = new UnitsHolder();
            foreach (Unit unit in obs.Observation.RawData.Units) {
                if (unit.Alliance != Alliance.Self) continue;
                if (Units.ArmyUnits.Contains(unit.UnitType)) 
                    this.units.army.Add(unit);
                
                if (Units.Workers.Contains(unit.UnitType)) 
                    this.units.workers.Add(unit);
                
                if (Units.Buildings.Contains(unit.UnitType)) 
                    this.units.buildings.Add(unit);
                
                if (Units.ResourceCenters.Contains(unit.UnitType)) 
                    this.units.resourceCenters.Add(unit);
               
                if ((unit.UnitType == Units.SUPPLY_DEPOT) || (unit.UnitType == Units.SUPPLY_DEPOT_LOWERED)) 
                    this.units.depots.Add(unit);

                if ((unit.UnitType == Units.BARRACKS) || (unit.UnitType == Units.BARRACKS_FLYING)) 
                    this.units.barracks.Add(unit);
            } 
        }
        
        public void OpenFrame(ResponseGameInfo gameInfo, ResponseObservation obs) {            
            this.obs = obs;            
            this.gameInfo = gameInfo;                    
            this.actions = new List<SC2APIProtocol.Action>();

            if (obs == null) {
                Logger.Info("ResponseObservation is null! The application will terminate.");
                Pause();
                Environment.Exit(0);
            }

            foreach (var chat in obs.Chat) {
                this.chatLog.Add(chat.Message);
            }

            this.frame = obs.Observation.GameLoop;
            this.currentSupply = obs.Observation.PlayerCommon.FoodUsed;
            this.maxSupply = obs.Observation.PlayerCommon.FoodCap;
            this.minerals = obs.Observation.PlayerCommon.Minerals;
            this.vespene = obs.Observation.PlayerCommon.Vespene;
                
            PopulateInventory();

            if (frame == 0) {
                var rcPosition = GetPosition(units.resourceCenters[0]);
                foreach (var startLocation in gameInfo.StartRaw.StartLocations) {
                    var enemyLocation = new Vector3(startLocation.X, startLocation.Y, 0);
                    var distance = GetDistance(enemyLocation, rcPosition);
                    if (distance > 30)
                        enemyLocations.Add(enemyLocation);
                }
            }
            
            if (frameDelay > 0)
                System.Threading.Thread.Sleep(frameDelay);

        }


        public void AddAction(SC2APIProtocol.Action action) {
            actions.Add(action);
        }


        public void Chat(string message, bool team=false) {            
            ActionChat actionChat = new ActionChat();
            if (team)
                actionChat.Channel = ActionChat.Types.Channel.Team;
            else
                actionChat.Channel = ActionChat.Types.Channel.Broadcast;
            actionChat.Message = message;
            
            SC2APIProtocol.Action action = new SC2APIProtocol.Action();
            action.ActionChat = actionChat;
            AddAction(action);
        }
        
        
        
        public Vector3 GetPosition(Unit unit) {
            return new Vector3(unit.Pos.X, unit.Pos.Y, unit.Pos.Z);
        }

        public double GetDistance(Unit unit1, Unit unit2) {
            return Vector3.Distance(GetPosition(unit1), GetPosition(unit2));
        }

        public double GetDistance(Unit unit, Vector3 location) {
            return Vector3.Distance(GetPosition(unit), location);
        }          
        
        public double GetDistance(Vector3 pos1, Vector3 pos2) {
            return Vector3.Distance(pos1, pos2);
        }                
        

        public Unit GetMineralField() {
            var mineralFields = GetUnits(Units.MineralFields, alliance:Alliance.Neutral);            
            foreach (var mf in mineralFields) {
                foreach (var rc in units.resourceCenters) {
                    if (GetDistance(mf, rc) < 10) return mf;
                }
            }
            return null;
        }

        public void Attack(List<Unit> units, Vector3 target) {
            var action = CreateRawUnitCommand(Abilities.ATTACK);            
            action.ActionRaw.UnitCommand.TargetWorldSpacePos = new Point2D();
            action.ActionRaw.UnitCommand.TargetWorldSpacePos.X = target.X;
            action.ActionRaw.UnitCommand.TargetWorldSpacePos.Y = target.Y;
            foreach (var unit in units)
                action.ActionRaw.UnitCommand.UnitTags.Add(unit.Tag);            
            AddAction(action);
        }
        
        public List<Unit> GetUnits(HashSet<uint> hashset, Alliance alliance=Alliance.Self)
        {
            List<Unit> units = new List<Unit>();                           
            foreach (Unit unit in obs.Observation.RawData.Units) {
                if ((hashset.Contains(unit.UnitType)) && (unit.Alliance == alliance)) 
                    units.Add(unit);
            }
            return units;
        }
        
        private List<Unit> GetUnits(uint unitType, Alliance alliance=Alliance.Self)
        {
            List<Unit> units = new List<Unit>();
            foreach (Unit unit in obs.Observation.RawData.Units) {
                if ((unit.UnitType == unitType) && (unit.Alliance == alliance))
                    units.Add(unit);
            }
            return units;
        }
        
        
        public bool CanConstruct(uint buildingType)
        {            
            if (units.workers.Count == 0) return false;                        

            //we need rc for every unit
            if (units.resourceCenters.Count == 0) return false;
            foreach (var building in units.resourceCenters) {
                if (building.BuildProgress < 1.0) return false;
            }

            if (buildingType == Units.SUPPLY_DEPOT)
                return (minerals >= 100);            
            
            
            foreach (var building in units.depots)
                if (building.BuildProgress < 1.0) return false;
            
            if (buildingType == Units.BARRACKS)
                return (minerals >= 150);

            return false;
        }

        private SC2APIProtocol.Action CreateRawUnitCommand(int ability) {            
            SC2APIProtocol.Action action = new SC2APIProtocol.Action();
            action.ActionRaw = new ActionRaw();
            action.ActionRaw.UnitCommand = new ActionRawUnitCommand();
            action.ActionRaw.UnitCommand.AbilityId = ability;
            return action;
        }       

        private uint GetUnitOrder(Unit unit) {
            if (unit.Orders.Count == 0) return 0;
            return unit.Orders[0].AbilityId;
        }

        public void TrainWorker(Unit resourceCenter, bool queue=false) {
            if (resourceCenter == null) return;            
            
            if ((!queue) && (GetUnitOrder(resourceCenter) == Abilities.TRAIN_SCV)) 
                return;
            
            var action = CreateRawUnitCommand(Abilities.TRAIN_SCV);
            action.ActionRaw.UnitCommand.UnitTags.Add(resourceCenter.Tag);
            AddAction(action);
        }


        public void TrainMarine(Unit barracks, bool queue=false) {
            if (barracks == null) return;                        
            if ((!queue) && (GetUnitOrder(barracks) == Abilities.TRAIN_MARINE)) 
                return;
            
            var action = CreateRawUnitCommand(Abilities.TRAIN_MARINE);
            action.ActionRaw.UnitCommand.UnitTags.Add(barracks.Tag);
            AddAction(action);
        }

        
        public void Construct(uint unitType) {
            var worker = GetAvailableWorker();
            if (worker == null) return;

            Vector3 startingSpot;
            if (units.resourceCenters.Count > 0) {
                var cc = units.resourceCenters[0];
                startingSpot = GetPosition(cc);
            }
            else
                startingSpot = GetPosition(worker);

            var radius = 12;

            //trying to find a valid construction spot
            Vector3 constructionSpot;
            while (true) {
                constructionSpot = new Vector3(startingSpot.X + random.Next(-radius, radius + 1), startingSpot.Y + random.Next(-radius, radius + 1), worker.Pos.Z);
                bool valid = true;

                //avoid building in the mineral line
                foreach (var w in units.workers) {
                    if (w.Tag == worker.Tag) continue;
                    if (GetDistance(w, constructionSpot) <= 3) {
                        valid = false;
                        break;
                    }
                }

                if (valid) break;
            }


            var constructAction = CreateRawUnitCommand(Abilities.FromBuilding[unitType]);
            constructAction.ActionRaw.UnitCommand.UnitTags.Add(worker.Tag);
            constructAction.ActionRaw.UnitCommand.TargetWorldSpacePos = new Point2D();
            constructAction.ActionRaw.UnitCommand.TargetWorldSpacePos.X = constructionSpot.X;
            constructAction.ActionRaw.UnitCommand.TargetWorldSpacePos.Y = constructionSpot.Y;
            AddAction(constructAction);

            var mf = GetMineralField();            
            if (mf != null) {
                var returnAction = CreateRawUnitCommand(Abilities.SMART);
                returnAction.ActionRaw.UnitCommand.UnitTags.Add(worker.Tag);
                returnAction.ActionRaw.UnitCommand.TargetUnitTag = mf.Tag;
                returnAction.ActionRaw.UnitCommand.QueueCommand = true;
                AddAction(returnAction);
            }

            Logger.Info("Attempting to construct: {0} @ {1} / {2}", unitType.ToString(), constructionSpot.X, constructionSpot.Y);            
        }

        public Unit GetAvailableWorker() {
            foreach (var worker in units.workers) {
                var order = GetUnitOrder(worker);
                if (order == 0) return worker;
                
                if (order != Abilities.GATHER_MINERALS) continue;
                return worker;
            }

            return null;
        }

        
        private void FocusCamera(Unit unit) {
            if (unit == null) return;
            SC2APIProtocol.Action action = new SC2APIProtocol.Action();
            action.ActionRaw = new ActionRaw();
            action.ActionRaw.CameraMove = new ActionRawCameraMove();
            action.ActionRaw.CameraMove.CenterWorldSpace = new Point();
            action.ActionRaw.CameraMove.CenterWorldSpace.X = unit.Pos.X;
            action.ActionRaw.CameraMove.CenterWorldSpace.Y = unit.Pos.Y;
            action.ActionRaw.CameraMove.CenterWorldSpace.Z = unit.Pos.Z;
            actions.Add(action);
        }

    }
}