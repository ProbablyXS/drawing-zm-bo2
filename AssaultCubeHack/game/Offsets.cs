namespace AssaultCubeHack
{
    class Offsets
    {
        public static int testVAL = 0x3AA7FB80;

        public static int baseGame = 0x01113F9C;  //MOVEMENT DES JOUEURS
        public static int PlayersList = 0xA9D6C;
        public const int ptrPlayerArray = 0x380; //Jump to the next entity (player)

        public const int headPos = 0x0;
        public const int footPos = 0x0;

        //CHECK IF ZOMBIE IS ALIVE
        public const int PlayerISALIVE_1 = 0x134; // 2050 is alive != is dead
        public const int PlayerISALIVE_2 = 0x3C; // 1 is alive != is dead
        public const int PlayerISALIVE_3 = 0x130; // 16777216 is alive != is dead
        //END

        public const int ZombieNumberPosition = 0x184;

        public const int viewMatrix = 0x0103AD40;  //Up 1 down -1 MATRIX CAMERA POSITION

        public static int SelfLocalPlayerNumberID = 0xA506C; //SA PROPRE ID (baseGame - A506C)
        public const int SelfLocalPlayerPOSITION = 0xb8; //SES PROPRE INFORMATIONS

    }
}
