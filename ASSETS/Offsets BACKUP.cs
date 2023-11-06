namespace AssaultCubeHack
{
    class Offsets
    {


        //// NON HOST //OLD AVEC LE HAUT ET LE BAS
        //public const int baseGame = 0x00468AD0;  //POUR LE ESP
        //public const int viewMatrix = 0x1065F40;  //POUR LE ESP


        ////ptrPlayerEnitity -> variableOffset
        //public const int ptrPlayerArray = 0x8C; //size of ptrPlayerArray //POUR LE ESP
        ////public const int numPlayers = 0x01149ECC; //size of ptrPlayerArray //POUR LE ESP


        ////ex: 0050F4E8+0C pointer to F8 = 100(health)
        ////pointer to players
        ////player variables
        //public const int health = 0x50; //SAVOIR SI IL EST MORT OU PAS MORT
        //public const int team = 0x50; //POUR LE ESP LA COULEUR DES CARRERS DE TA TEAM //optinal pour diff les couleurs
        //public const int headPos = 0x44; //POUR LE ESP
        //public const int footPos = 0x20; //POUR LE ESP
        //// NON HOST //OLD AVEC LE HAUT ET LE BAS


        // NON HOST ENEMIE
        //public static int baseGame = 0x01140878;  //MOVEMENT DES JOUEURS
        public static int baseGame;  //MOVEMENT DES JOUEURS
        public static int test;  //MOVEMENT DES JOUEURS
        public const int JUMPTOPlayerbaseGame = 0x2C; // CALC add for aimbot
        public const int ptrPlayerArray = 0x380; //size of ptrPlayerArray
        public const int headPos = 0x0;
        public const int footPos = 0x0;
        public const int PlayerISALIVE2 = 0x44; // 544 is alive || 0 is dead

        public static int VSAT = 0x5CB18;

        public static int PlayersLIST = 0x00;
        public const int ptrPlayerLISTArray = 0x808;
        public const int JUMPTOPlayersLIST = 0x3B600; // CALC add for aimbot
        public const int PlayerTEAM = 0x20;
        public const int PlayerTEAMForFFA = 0x28;
        public const int PlayerISALIVE = 0xC4; // 0 is alive || 1 is dead
        public const int PlayerNAME = 0x00;
        public const int PlayerNumberID = 0x84;
        public const int PlayerPING = 0x7C;
        public const int PlayerPositionLeaderBoard = 0x84;
        public const int PlayerTagTeamName = 0x74;
        public const int PlayerWeapon = 0xD4;
        public const int PlayerCROUCH = 0x578;

        public const int viewMatrix = 0x1065F40;  //Up 1 down -1

        public static int SelfLocalPlayerNumberID = 0xA506C; //SA PROPRE ID (baseGame - A506C)

        public const int SelfLocalPlayer = 0x02E3D838; //SES PROPRE INFORMATIONS
        public const int SelfLocalPlayerPOSITION = 0x1C8; //SES PROPRE INFORMATIONS
        public const int SelfLocalPlayerTEAM = 0x200; //SA PROPRE TEAM

        public const int SelfJumpPOS = 0x5CDCC; // CALC add for aimbot
        public const int yaw = 0x4; // de bas en 360 degrée  (camera X)
        public const int pitch = 0x0; // de bas en HAUT (camera Y)
        // NON HOST

    }
}
