using System.Collections.Generic;
using SC2APIProtocol;

namespace Bot {
    internal class RaxBot : Bot {
        
        //the following will be called every frame
        //you can increase the amount of frames that get processed for each step at once in Wrapper/GameConnection.cs: stepSize  
        public IEnumerable<Action> OnFrame() {
            Controller.OpenFrame();

            if (Controller.frame == 0) {
                Logger.Info("RaxBot");
                Logger.Info("--------------------------------------");
                Logger.Info("Map: {0}", Controller.gameInfo.MapName);
                Logger.Info("--------------------------------------");
            }

            if (Controller.frame == Controller.SecsToFrames(1)) 
                Controller.Chat("gl hf");

            var structures = Controller.GetUnits(Units.Structures);
            if (structures.Count == 1) {
                //last building                
                if (structures[0].integrity < 0.4) //being attacked or burning down                 
                    if (!Controller.chatLog.Contains("gg"))
                        Controller.Chat("gg");                
            }

            var resourceCenters = Controller.GetUnits(Units.ResourceCenters);
            foreach (var rc in resourceCenters) {
                if (Controller.CanConstruct(Units.SCV))
                    rc.Train(Units.SCV);
            }
            
            
            //keep on buildings depots if supply is tight
            if (Controller.maxSupply - Controller.currentSupply <= 5)
                if (Controller.CanConstruct(Units.SUPPLY_DEPOT))
                    if (Controller.GetPendingCount(Units.SUPPLY_DEPOT) == 0)                    
                        Controller.Construct(Units.SUPPLY_DEPOT);

            
            //distribute workers optimally every 10 frames
            if (Controller.frame % 10 == 0)
                Controller.DistributeWorkers();
            
            

            //build up to 4 barracks at once
            if (Controller.CanConstruct(Units.BARRACKS)) 
                if (Controller.GetTotalCount(Units.BARRACKS) < 4)                
                    Controller.Construct(Units.BARRACKS);          
            
            //train marine
            foreach (var barracks in Controller.GetUnits(Units.BARRACKS, onlyCompleted:true)) {
                if (Controller.CanConstruct(Units.MARINE))
                    barracks.Train(Units.MARINE);
            }

            //attack when we have enough units
            var army = Controller.GetUnits(Units.ArmyUnits);
            if (army.Count > 20) {
                if (Controller.enemyLocations.Count > 0)
                    Controller.Attack(army, Controller.enemyLocations[0]);
            }            

            return Controller.CloseFrame();
        }
    }
}