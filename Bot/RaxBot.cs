using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.NetworkInformation;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading;
using System.Xml.Schema;
using Google.Protobuf.Collections;
using Microsoft.Win32;
using SC2APIProtocol;
using Action = System.Action;
using System.Numerics;


namespace Bot
{
    class RaxBot : Bot
    {        
        private static Random random = new Random(GenerateSeed());       
        private Controller controller = new Controller(0);

        private static int GenerateSeed() {
            var currentDayOfYear = DateTime.Now.DayOfYear;
            var currentMinute = DateTime.Now.Minute;
            var seed = currentDayOfYear * 1000 + (currentMinute % 3);
            
            return seed;
        }



        public void OnStart(ResponseGameInfo gameInfo, ResponseObservation obs, uint playerId) {
            Logger.Info("GAME STARTED");
        }
        
        public void OnEnd(ResponseGameInfo gameInfo, ResponseObservation obs, uint playerId, Result result) {
            Logger.Info("GAME ENDED");
            Logger.Info("Result: {0}", result);
            
            
        }
        

        public IEnumerable<SC2APIProtocol.Action> OnFrame(ResponseGameInfo gameInfo, ResponseObservation obs, uint playerId)
        {
            controller.OpenFrame(gameInfo, obs);

            if (controller.frame == 0) {
                Logger.Info("RaxBot");
                Logger.Info("--------------------------------------");
                Logger.Info("Map: {0}", gameInfo.MapName);
                Logger.Info("--------------------------------------");
            }

            if (controller.frame == controller.SecsToFrames(1)) {
                controller.Chat("gl hf");
            }

            if ((controller.units.buildings.Count == 1) && (controller.units.buildings[0].Health <= controller.units.buildings[0].HealthMax * 0.35)) {
                if (!controller.chatLog.Contains("gg"))
                    controller.Chat("gg");                
            }

            //keep on buildings depots if supply is tight
            if (controller.maxSupply - controller.currentSupply <= 5) {
                if (controller.CanConstruct(Units.SUPPLY_DEPOT)) {
                    controller.Construct(Units.SUPPLY_DEPOT);
                }
            }
            
            //build barracks
            if (controller.CanConstruct(Units.BARRACKS)) {
                controller.Construct(Units.BARRACKS);
            }
            

            //train worker
            foreach (var cc in controller.units.resourceCenters) {
                controller.TrainWorker(cc);                
            }
            
                        
            //train marine
            foreach (var barracks in controller.units.barracks) {
                controller.TrainMarine(barracks);                
            }

            
            //attack when we have enough units
            if (controller.units.army.Count > 20) {
                //var armyUnits = controller.GetUnits(Units.ArmyUnits); //this works just as well
                var armyUnits = controller.units.army;
                
                if (controller.enemyLocations.Count > 0)
                    controller.Attack(armyUnits, controller.enemyLocations[0]);

            }
            

            return controller.CloseFrame();
        }
    }
}













