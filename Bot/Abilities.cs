using System.Collections.Generic;

namespace Bot {
    internal static class Abilities {
        //you can get all these values from the stableid.json file (just search for it on your PC)

        public static int BUILD_SUPPLY_DEPOT = 319;
        public static int BUILD_BARRACKS = 321;
        public static int BUILD_BUNKER = 324;
        public static int BUILD_REFINERY = 320;
        public static int BUILD_FACTORY = 328;
        public static int BUILD_STARPORT = 329;
        public static int BUILD_TECHLAB = 3682;
        public static int BUILD_REACTOR = 3683;
        public static int BUILD_ORBITAL_COMMAND = 1516;
        public static int BUILD_PLANETARY_FORTRESS = 1450;
        public static int BUILD_COMMAND_CENTER = 318;
        public static int BUILD_ENGINEERING_BAY = 322;
        public static int BUILD_MISSILE_TURRET = 323;
        public static int BUILD_ARMORY = 331;
        
        public static int TRAIN_SCV = 524;
        public static int TRAIN_MARINE = 560;
        public static int TRAIN_REAPER = 561;
        public static int TRAIN_HELLION = 595;
        public static int TRAIN_VIKING = 624;
        public static int TRAIN_RAVEN = 622;
        public static int TRAIN_SIEGE_TANK = 591;
        public static int TRAIN_BANSHEE = 621;
        public static int TRAIN_THOR = 594;
        public static int TRAIN_CYCLONE = 597;
        
        public static int RESEARCH_BANSHEE_CLOAK = 790;
        public static int RESEARCH_INFERNAL_PREIGNITER = 761;
        public static int RESEARCH_UPGRADE_MECH_AIR = 3699;     
        public static int RESEARCH_UPGRADE_MECH_ARMOR = 3700;   
        public static int RESEARCH_UPGRADE_MECH_GROUND = 3701;  
        
        public static int CANCEL_CONSTRUCTION = 314;       
        public static int CANCEL = 3659;
        public static int CANCEL_LAST = 3671;
        public static int LIFT = 3679;
        public static int LAND = 3678;
        
        public static int SMART = 1;
        public static int STOP = 4;        
        public static int ATTACK = 23;
        public static int MOVE = 16;        
        public static int PATROL = 17;
        public static int RALLY = 3673;
        public static int REPAIR = 316;
        
        public static int THOR_SWITCH_AP = 2362;
        public static int THOR_SWITCH_NORMAL = 2364;
        public static int SCANNER_SWEEP = 399;
        public static int YAMATO = 401;
        public static int CALL_DOWN_MULE = 171;
        public static int CLOAK = 3676;
        public static int REAPER_GRENADE = 2588;
        public static int DEPOT_RAISE = 558;
        public static int DEPOT_LOWER = 556;
        public static int SIEGE_TANK = 388;
        public static int UNSIEGE_TANK = 390;
        public static int TRANSFORM_TO_HELLBAT = 1998;
        public static int TRANSFORM_TO_HELLION = 1978;
        public static int UNLOAD_BUNKER = 408;
        public static int SALVAGE_BUNKER = 32;
        
        //gathering/returning minerals
        public static int GATHER_RESOURCES = 295;
        public static int RETURN_RESOURCES = 296;


        //gathering/returning minerals
        public static int GATHER_MINERALS = 295;
        public static int RETURN_MINERALS = 296;


        public static readonly Dictionary<uint, int> FromBuilding = new Dictionary<uint, int> {
            {Units.SUPPLY_DEPOT, BUILD_SUPPLY_DEPOT},
            {Units.BARRACKS, BUILD_BARRACKS},
            {Units.BUNKER, BUILD_BUNKER}
            //you need to add whatever you need here
        };
    }
}