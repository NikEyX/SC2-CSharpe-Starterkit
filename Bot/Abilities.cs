using System.Collections.Generic;

namespace Bot
{
    class Abilities
    {
        //you can get all these values from the stableid.json file (just search for it on your PC)
        
        public static int BUILD_SUPPLY_DEPOT = 319;
        public static int BUILD_BARRACKS = 321;
        public static int BUILD_BUNKER = 324;
        
        public static int TRAIN_SCV = 524;
        public static int TRAIN_MARINE = 560;
        
        public static int CANCEL_CONSTRUCTION = 314;       
        public static int CANCEL = 3659;
        public static int CANCEL_LAST = 3671;
        public static int LIFT = 3679;
        public static int LAND = 3678;
        
        public static int SMART = 1;
        public static int STOP = 4;        
        public static int ATTACK = 3674;
        public static int MOVE = 16;        
        public static int PATROL = 17;
        public static int RALLY = 3673;

        public static int REPAIR = 3685;

        public static int DEPOT_RAISE = 558;
        public static int DEPOT_LOWER = 556;
        
        //gathering/returning minerals
        public static int GATHER_MINERALS = 295;
        public static int RETURN_MINERALS = 296;
        
        
        public static readonly Dictionary<uint, int> FromBuilding = new Dictionary<uint, int>()
        {
            { Units.SUPPLY_DEPOT, BUILD_SUPPLY_DEPOT },
            { Units.BARRACKS, BUILD_BARRACKS },
            { Units.BUNKER, BUILD_BUNKER },
        };
        
    }
}
