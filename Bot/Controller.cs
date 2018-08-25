using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using SC2APIProtocol;
using Action = SC2APIProtocol.Action;

namespace Bot {
    public static class Controller {
        //editable
        private static readonly int frameDelay = 0; //too fast? increase this to e.g. 20

        //don't edit
        private static readonly List<Action> actions = new List<Action>();
        private static readonly Random random = new Random();
        private const double FRAMES_PER_SECOND = 22.4;

        public static ResponseGameInfo gameInfo;
        public static ResponseData gameData;
        public static ResponseObservation obs;
        public static ulong frame;
        public static uint currentSupply;
        public static uint maxSupply;
        public static uint minerals;
        public static uint vespene;

        public static readonly List<Vector3> enemyLocations = new List<Vector3>();
        public static readonly List<string> chatLog = new List<string>();

        public static UnitsHolder units = new UnitsHolder();


        public static void Pause() {
            Console.WriteLine("Press any key to continue...");
            while (Console.ReadKey().Key != ConsoleKey.Enter) {
                //do nothing
            }
        }

        public static ulong SecsToFrames(int seconds) {
            return (ulong) (FRAMES_PER_SECOND * seconds);
        }

        private static void PopulateInventory() {
            units = new UnitsHolder();
            foreach (var unit in obs.Observation.RawData.Units) {
                if (unit.Alliance != Alliance.Self) continue;
                if (Units.ArmyUnits.Contains(unit.UnitType))
                    units.army.Add(unit);

                if (Units.Workers.Contains(unit.UnitType))
                    units.workers.Add(unit);

                if (Units.Structures.Contains(unit.UnitType))
                    units.structures.Add(unit);

                if (Units.ResourceCenters.Contains(unit.UnitType))
                    units.resourceCenters.Add(unit);

                if (unit.UnitType == Units.SUPPLY_DEPOT || unit.UnitType == Units.SUPPLY_DEPOT_LOWERED)
                    units.depots.Add(unit);

                if (unit.UnitType == Units.BARRACKS || unit.UnitType == Units.BARRACKS_FLYING)
                    units.barracks.Add(unit);
            }
        }

        public static List<Action> CloseFrame() {
            return actions;
        }


        public static void OpenFrame() {
            if (gameInfo == null || gameData == null || obs == null) {
                if (gameInfo == null)
                    Logger.Info("GameInfo is null! The application will terminate.");
                else if (gameData == null)
                    Logger.Info("GameData is null! The application will terminate.");
                else
                    Logger.Info("ResponseObservation is null! The application will terminate.");
                Pause();
                Environment.Exit(0);
            }

            actions.Clear();

            foreach (var chat in obs.Chat) chatLog.Add(chat.Message);

            frame = obs.Observation.GameLoop;
            currentSupply = obs.Observation.PlayerCommon.FoodUsed;
            maxSupply = obs.Observation.PlayerCommon.FoodCap;
            minerals = obs.Observation.PlayerCommon.Minerals;
            vespene = obs.Observation.PlayerCommon.Vespene;

            PopulateInventory();

            //initialization
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
                Thread.Sleep(frameDelay);
        }


        public static void AddAction(Action action) {
            actions.Add(action);
        }


        public static void Chat(string message, bool team = false) {
            var actionChat = new ActionChat();
            if (team)
                actionChat.Channel = ActionChat.Types.Channel.Team;
            else
                actionChat.Channel = ActionChat.Types.Channel.Broadcast;
            actionChat.Message = message;

            var action = new Action();
            action.ActionChat = actionChat;
            AddAction(action);
        }


        public static Vector3 GetPosition(Unit unit) {
            return new Vector3(unit.Pos.X, unit.Pos.Y, unit.Pos.Z);
        }

        public static double GetDistance(Unit unit1, Unit unit2) {
            return Vector3.Distance(GetPosition(unit1), GetPosition(unit2));
        }

        public static double GetDistance(Unit unit, Vector3 location) {
            return Vector3.Distance(GetPosition(unit), location);
        }

        public static double GetDistance(Vector3 pos1, Vector3 pos2) {
            return Vector3.Distance(pos1, pos2);
        }


        public static Unit GetMineralField() {
            var mineralFields = GetUnits(Units.MineralFields, Alliance.Neutral);
            foreach (var mf in mineralFields)
            foreach (var rc in units.resourceCenters)
                if (GetDistance(mf, rc) < 10)
                    return mf;
            return null;
        }

        public static void Attack(List<Unit> units, Vector3 target) {
            var action = CreateRawUnitCommand(Abilities.ATTACK);
            action.ActionRaw.UnitCommand.TargetWorldSpacePos = new Point2D();
            action.ActionRaw.UnitCommand.TargetWorldSpacePos.X = target.X;
            action.ActionRaw.UnitCommand.TargetWorldSpacePos.Y = target.Y;
            foreach (var unit in units)
                action.ActionRaw.UnitCommand.UnitTags.Add(unit.Tag);
            AddAction(action);
        }

        public static List<Unit> GetUnits(HashSet<uint> hashset, Alliance alliance = Alliance.Self) {
            var units = new List<Unit>();
            foreach (var unit in obs.Observation.RawData.Units)
                if (hashset.Contains(unit.UnitType) && unit.Alliance == alliance)
                    units.Add(unit);
            return units;
        }

        private static List<Unit> GetUnits(uint unitType, Alliance alliance = Alliance.Self) {
            var units = new List<Unit>();
            foreach (var unit in obs.Observation.RawData.Units)
                if (unit.UnitType == unitType && unit.Alliance == alliance)
                    units.Add(unit);
            return units;
        }


        public static bool CanAfford(uint buildingType) {
            return minerals >= gameData.Units[(int) buildingType].MineralCost &&
                   vespene >= gameData.Units[(int) buildingType].VespeneCost;
        }

        public static bool CanConstruct(uint buildingType) {
            if (units.workers.Count == 0) return false;

            //we need rc for every unit
            if (units.resourceCenters.Count == 0) return false;
            foreach (var building in units.resourceCenters)
                if (building.BuildProgress < 1.0)
                    return false;

            if (buildingType == Units.SUPPLY_DEPOT)
                return CanAfford(buildingType);

            foreach (var building in units.depots)
                if (building.BuildProgress < 1.0)
                    return false;

            if (buildingType == Units.BARRACKS)
                return CanAfford(buildingType);

            //catch all
            return CanAfford(buildingType);
        }

        private static Action CreateRawUnitCommand(int ability) {
            var action = new Action();
            action.ActionRaw = new ActionRaw();
            action.ActionRaw.UnitCommand = new ActionRawUnitCommand();
            action.ActionRaw.UnitCommand.AbilityId = ability;
            return action;
        }

        private static uint GetUnitOrder(Unit unit) {
            if (unit.Orders.Count == 0) return 0;
            return unit.Orders[0].AbilityId;
        }

        public static void TrainWorker(Unit resourceCenter, bool queue = false) {
            if (resourceCenter == null) return;

            if (!queue && GetUnitOrder(resourceCenter) == Abilities.TRAIN_SCV)
                return;

            var action = CreateRawUnitCommand(Abilities.TRAIN_SCV);
            action.ActionRaw.UnitCommand.UnitTags.Add(resourceCenter.Tag);
            AddAction(action);
        }


        public static void TrainMarine(Unit barracks, bool queue = false) {
            if (barracks == null) return;
            if (!queue && GetUnitOrder(barracks) == Abilities.TRAIN_MARINE)
                return;

            var action = CreateRawUnitCommand(Abilities.TRAIN_MARINE);
            action.ActionRaw.UnitCommand.UnitTags.Add(barracks.Tag);
            AddAction(action);
        }


        public static void Construct(uint unitType) {
            var worker = GetAvailableWorker();
            if (worker == null) return;

            Vector3 startingSpot;
            if (units.resourceCenters.Count > 0) {
                var cc = units.resourceCenters[0];
                startingSpot = GetPosition(cc);
            }
            else {
                startingSpot = GetPosition(worker);
            }

            var radius = 12;

            //trying to find a valid construction spot
            Vector3 constructionSpot;
            while (true) {
                constructionSpot = new Vector3(startingSpot.X + random.Next(-radius, radius + 1),
                    startingSpot.Y + random.Next(-radius, radius + 1), worker.Pos.Z);
                var valid = true;

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

            Logger.Info("Attempting to construct: {0} @ {1} / {2}", unitType.ToString(), constructionSpot.X,
                constructionSpot.Y);
        }

        public static Unit GetAvailableWorker() {
            foreach (var worker in units.workers) {
                var order = GetUnitOrder(worker);
                if (order == 0) return worker;

                if (order != Abilities.GATHER_MINERALS) continue;
                return worker;
            }

            return null;
        }


        private static void FocusCamera(Unit unit) {
            if (unit == null) return;
            var action = new Action();
            action.ActionRaw = new ActionRaw();
            action.ActionRaw.CameraMove = new ActionRawCameraMove();
            action.ActionRaw.CameraMove.CenterWorldSpace = new Point();
            action.ActionRaw.CameraMove.CenterWorldSpace.X = unit.Pos.X;
            action.ActionRaw.CameraMove.CenterWorldSpace.Y = unit.Pos.Y;
            action.ActionRaw.CameraMove.CenterWorldSpace.Z = unit.Pos.Z;
            actions.Add(action);
        }

        public class UnitsHolder {
            public readonly List<Unit> army = new List<Unit>();
            public readonly List<Unit> barracks = new List<Unit>();
            public readonly List<Unit> depots = new List<Unit>();
            public readonly List<Unit> resourceCenters = new List<Unit>();
            public readonly List<Unit> structures = new List<Unit>();
            public readonly List<Unit> workers = new List<Unit>();
        }
    }
}