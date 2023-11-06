namespace AssaultCubeHack
{
    class Player
    {
        public int pointerPlayer;
        //public int playerslist;

        public Vector3 PositionHead
        {
            //get { return Memory.ReadHEAD(pointerPlayer + Offsets.headPos, 0); }
            //get { return Memory.ReadHEAD(Offsets.testVAL); }
            get { return Memory.ReadHEAD(pointerPlayer); }
        }

        public Vector3 PositionFoot
        {
            //get { return Memory.ReadFOOT(pointerPlayer + Offsets.footPos); }
            //get { return Memory.ReadFOOT(Offsets.testVAL); }
            get { return Memory.ReadFOOT(pointerPlayer); }
        }

        public Vector3 SelfPosHead
        {
            get { return Memory.ReadHEAD(Offsets.baseGame + Offsets.SelfLocalPlayerPOSITION); }
        }

        public Vector3 SelfPosFoot
        {
            get { return Memory.ReadFOOT(Offsets.baseGame + Offsets.SelfLocalPlayerPOSITION); }
        }

        public int ZombieNumberPosition
        {
            get { return Memory.Read<int>(pointerPlayer + Offsets.ZombieNumberPosition) + 1; }
        }

        public (int alive1, int alive2, int alive3) PlayerISALIVE()
        {
            int alive1 = Memory.Read<int>(pointerPlayer + Offsets.PlayerISALIVE_1);
            int alive2 = Memory.Read<int>(pointerPlayer + Offsets.PlayerISALIVE_2);
            int alive3 = Memory.Read<int>(pointerPlayer + Offsets.PlayerISALIVE_3);
            return (alive1, alive2, alive3);
        }

        public Player(int pointerPlayer)
        {
            this.pointerPlayer = pointerPlayer;
            //this.playerslist = playerslist;
        }

    }
}
