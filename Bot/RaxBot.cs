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


        private static int GenerateSeed() {
            var currentDayOfYear = DateTime.Now.DayOfYear;
            var currentMinute = DateTime.Now.Minute;
            var seed = currentDayOfYear * 1000 + (currentMinute % 3);
            
            return seed;
        }



        public void OnStart(ResponseGameInfo gameInfo, ResponseObservation obs, uint playerId)
        {
            Logger.Info("GAME STARTED");
        }

        public void OnEnd(ResponseGameInfo gameInfo, ResponseObservation obs, uint playerId, Result result)
        {
            Logger.Info("GAME ENDED");
            Logger.Info("Result: {0}", result);
        }


        public IEnumerable<SC2APIProtocol.Action> OnFrame()
        {
            Controller.OpenFrame();

            if (Controller.frame == 0) {
                Logger.Info("RaxBot");
                Logger.Info("--------------------------------------");
                Logger.Info("Map: {0}", Controller.gameInfo.MapName);
                Logger.Info("--------------------------------------");
            }

            if (Controller.frame == Controller.SecsToFrames(1)) {
                Controller.Chat("gl hf");
            }

            if ((Controller.units.buildings.Count == 1) && (Controller.units.buildings[0].Health <= Controller.units.buildings[0].HealthMax * 0.35)) {
                if (!Controller.chatLog.Contains("gg"))
                    Controller.Chat("gg");                
            }

            //keep on buildings depots if supply is tight
            if (Controller.maxSupply - Controller.currentSupply <= 5) {
                if (Controller.CanConstruct(Units.SUPPLY_DEPOT)) {
                    Controller.Construct(Units.SUPPLY_DEPOT);
                }
            }
            
            //build barracks
            if (Controller.CanConstruct(Units.BARRACKS)) {
                Controller.Construct(Units.BARRACKS);
            }
            

            //train worker
            foreach (var cc in Controller.units.resourceCenters) {
                Controller.TrainWorker(cc);                
            }
            
                        
            //train marine
            foreach (var barracks in Controller.units.barracks) {
                Controller.TrainMarine(barracks);                
            }

            
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

