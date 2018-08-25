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

            if (Controller.frame == Controller.SecsToFrames(1)) Controller.Chat("gl hf");

            if (Controller.units.structures.Count == 1 && Controller.units.structures[0].Health <=
                Controller.units.structures[0].HealthMax * 0.35)
                if (!Controller.chatLog.Contains("gg"))
                    Controller.Chat("gg");

            //keep on buildings depots if supply is tight
            if (Controller.maxSupply - Controller.currentSupply <= 5)
                if (Controller.CanConstruct(Units.SUPPLY_DEPOT))
                    Controller.Construct(Units.SUPPLY_DEPOT);

            //build barracks
            if (Controller.CanConstruct(Units.BARRACKS)) Controller.Construct(Units.BARRACKS);


            //train worker
            foreach (var cc in Controller.units.resourceCenters) Controller.TrainWorker(cc);


            //train marine
            foreach (var barracks in Controller.units.barracks) Controller.TrainMarine(barracks);


            //attack when we have enough units
            if (Controller.units.army.Count > 20) {
                //var armyUnits = Controller.GetUnits(Units.ArmyUnits); //this works just as well
                var armyUnits = Controller.units.army;

                if (Controller.enemyLocations.Count > 0)
                    Controller.Attack(armyUnits, Controller.enemyLocations[0]);
            }


            return Controller.CloseFrame();
        }
    }
}