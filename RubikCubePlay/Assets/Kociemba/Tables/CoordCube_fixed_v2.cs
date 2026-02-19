using System;

namespace Kociemba
{
    internal class CoordCube
    {
        internal const short N_TWIST = 2187; // 3^7 possible corner orientations
        internal const short N_FLIP = 2048; // 2^11 possible edge flips
        internal const short N_SLICE1 = 495; // 12 choose 4 possible positions of FR,FL,BL,BR edges
        internal const short N_SLICE2 = 24; // 4! permutations of FR,FL,BL,BR edges in phase2
        internal const short N_PARITY = 2; // 2 possible corner parities
        internal const short N_URFtoDLF = 20160; // 8!/(8-6)! permutation of URF,UFL,ULB,UBR,DFR,DLF corners
        internal const short N_FRtoBR = 11880; // 12!/(12-4)! permutation of FR,FL,BL,BR edges
        internal const short N_URtoUL = 1320; // 12!/(12-3)! permutation of UR,UF,UL edges
        internal const short N_UBtoDF = 1320; // 12!/(12-3)! permutation of UB,DR,DF edges
        internal const short N_URtoDF = 20160; // 8!/(8-6)! permutation of UR,UF,UL,UB,DR,DF edges in phase2

        internal const int N_URFtoDLB = 40320; // 8! permutations of the corners
        internal const int N_URtoBR = 479001600; // 8! permutations of the corners

        internal const short N_MOVE = 18;

        // All coordinates are 0 for a solved cube except for UBtoDF, which is 114
        internal short twist;
        internal short flip;
        internal short parity;
        internal short FRtoBR;
        internal short URFtoDLF;
        internal short URtoUL;
        internal short UBtoDF;
        internal int URtoDF;

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Generate a CoordCube from a CubieCube
        internal CoordCube(CubieCube c, DateTime startTime, string currentTime, out string info)
        {
            info = currentTime;
            twist = c.getTwist();

            flip = c.getFlip();
            parity = c.cornerParity();
            FRtoBR = c.getFRtoBR();

            URFtoDLF = c.getURFtoDLF();
            URtoUL = c.getURtoUL();
            UBtoDF = c.getUBtoDF();
            URtoDF = c.getURtoDF();// only needed in phase2
            info += "[ Finished Initialiation: " + String.Format(@"{0:mm\\:ss\\.ffff}", (DateTime.Now - startTime)) + " ] ";

        }

        // A move on the coordinate level
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        internal virtual void move(int m)
        {
            twist = twistMove[twist, m];
            flip = flipMove[flip, m];
            parity = parityMove[parity][m];
            FRtoBR = FRtoBR_Move[FRtoBR, m];
            URFtoDLF = URFtoDLF_Move[URFtoDLF, m];
            URtoUL = URtoUL_Move[URtoUL, m];
            UBtoDF = UBtoDF_Move[UBtoDF, m];
            if (URtoUL < 336 && UBtoDF < 336) // updated only if UR,UF,UL,UB,DR,DF
            {
                // are not in UD-slice
                URtoDF = MergeURtoULandUBtoDF[URtoUL, UBtoDF];
            }
        }


        // ******************************************Phase 1 move tables*****************************************************

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Move table for the twists of the corners
        // twist < 2187 in phase 2.
        // twist = 0 in phase 2.
        internal static short[,] twistMove = CoordCubeTables.twist;
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Move table for the flips of the edges
        // flip < 2048 in phase 1
        // flip = 0 in phase 2.
        internal static short[,] flipMove = CoordCubeTables.flip;
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Parity of the corner permutation. This is the same as the parity for the edge permutation of a valid cube.
        // parity has values 0 and 1
        internal static short[][] parityMove = new short[][]
        {
    new short[] {1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1},
    new short[] {0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0}
        };


        // ***********************************Phase 1 and 2 movetable********************************************************

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Move table for the four UD-slice edges FR, FL, Bl and BR
        // FRtoBRMove < 11880 in phase 1
        // FRtoBRMove < 24 in phase 2
        // FRtoBRMove = 0 for solved cube
        internal static short[,] FRtoBR_Move = CoordCubeTables.FRtoBR;

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Move table for permutation of six corners. The positions of the DBL and DRB corners are determined by the parity.
        // URFtoDLF < 20160 in phase 1
        // URFtoDLF < 20160 in phase 2
        // URFtoDLF = 0 for solved cube.
        internal static short[,] URFtoDLF_Move = CoordCubeTables.URFtoDLF;

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Move table for the permutation of six U-face and D-face edges in phase2. The positions of the DL and DB edges are
        // determined by the parity.
        // URtoDF < 665280 in phase 1
        // URtoDF < 20160 in phase 2
        // URtoDF = 0 for solved cube.
        internal static short[,] URtoDF_Move = CoordCubeTables.URtoDF;

        // **************************helper move tables to compute URtoDF for the beginning of phase2************************

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Move table for the three edges UR,UF and UL in phase1.
        internal static short[,] URtoUL_Move = CoordCubeTables.URtoUL;

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Move table for the three edges UB,DR and DF in phase1.
        internal static short[,] UBtoDF_Move = CoordCubeTables.UBtoDF;

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Table to merge the coordinates of the UR,UF,UL and UB,DR,DF edges at the beginning of phase2
        internal static short[,] MergeURtoULandUBtoDF = CoordCubeTables.MergeURtoULandUBtoDF;


        // ****************************************Pruning tables for the search*********************************************

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Pruning table for the permutation of the corners and the UD-slice edges in phase2.
        // The pruning table entries give a lower estimation for the number of moves to reach the solved cube.
        internal static sbyte[] Slice_URFtoDLF_Parity_Prun = CoordCubeTables.Slice_URFtoDLF_Parity_Prun;

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Pruning table for the permutation of the edges in phase2.
        // The pruning table entries give a lower estimation for the number of moves to reach the solved cube.
        internal static sbyte[] Slice_URtoDF_Parity_Prun = CoordCubeTables.Slice_URtoDF_Parity_Prun;

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Pruning table for the twist of the corners and the position (not permutation) of the UD-slice edges in phase1
        // The pruning table entries give a lower estimation for the number of moves to reach the H-subgroup.
        internal static sbyte[] Slice_Twist_Prun = CoordCubeTables.Slice_Twist_Prun;

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Pruning table for the flip of the edges and the position (not permutation) of the UD-slice edges in phase1
        // The pruning table entries give a lower estimation for the number of moves to reach the H-subgroup.
        internal static sbyte[] Slice_Flip_Prun = CoordCubeTables.Slice_Flip_Prun;
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


        // Set pruning value in table. Two values are stored in one byte.
        internal static void setPruning(sbyte[] table, int index, sbyte value)
        {
            if ((index & 1) == 0)
            {
                table[index / 2] &= unchecked((sbyte)(0xf0 | value));
            }
            else
            {
                table[index / 2] &= (sbyte)(0x0f | (value << 4));
            }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Extract pruning value
        internal static sbyte getPruning(sbyte[] table, int index)
        {
            if ((index & 1) == 0)
            {
                return (sbyte)(table[index / 2] & 0x0f);
            }
            else
            {
                return (sbyte)((int)((uint)(table[index / 2] & 0xf0) >> 4));
            }
        }
    }

    
    public static class CoordCubeTables
    {
        // These are loaded from disk if available; otherwise generated at runtime and cached.
        public static short[,] twist;
        public static short[,] flip;
        public static short[,] FRtoBR;
        public static short[,] URFtoDLF;
        public static short[,] URtoDF;
        public static short[,] URtoUL;
        public static short[,] UBtoDF;
        public static short[,] MergeURtoULandUBtoDF;

        public static sbyte[] Slice_URFtoDLF_Parity_Prun;
        public static sbyte[] Slice_URtoDF_Parity_Prun;
        public static sbyte[] Slice_Twist_Prun;
        public static sbyte[] Slice_Flip_Prun;

        static CoordCubeTables()
        {
            // Try to load all tables. If any is missing, generate everything once.
            bool ok =
                Tools.TryDeserializeTable("twist", out twist) &&
                Tools.TryDeserializeTable("flip", out flip) &&
                Tools.TryDeserializeTable("FRtoBR", out FRtoBR) &&
                Tools.TryDeserializeTable("URFtoDLF", out URFtoDLF) &&
                Tools.TryDeserializeTable("URtoDF", out URtoDF) &&
                Tools.TryDeserializeTable("URtoUL", out URtoUL) &&
                Tools.TryDeserializeTable("UBtoDF", out UBtoDF) &&
                Tools.TryDeserializeTable("MergeURtoULandUBtoDF", out MergeURtoULandUBtoDF) &&
                Tools.TryDeserializeSbyteArray("Slice_URFtoDLF_Parity_Prun", out Slice_URFtoDLF_Parity_Prun) &&
                Tools.TryDeserializeSbyteArray("Slice_URtoDF_Parity_Prun", out Slice_URtoDF_Parity_Prun) &&
                Tools.TryDeserializeSbyteArray("Slice_Twist_Prun", out Slice_Twist_Prun) &&
                Tools.TryDeserializeSbyteArray("Slice_Flip_Prun", out Slice_Flip_Prun);

            if (!ok)
            {
                GenerateAll();
                // Best effort cache. If caching fails (eg. permissions), solver still works.
                Tools.TrySerializeTable("twist", twist);
                Tools.TrySerializeTable("flip", flip);
                Tools.TrySerializeTable("FRtoBR", FRtoBR);
                Tools.TrySerializeTable("URFtoDLF", URFtoDLF);
                Tools.TrySerializeTable("URtoDF", URtoDF);
                Tools.TrySerializeTable("URtoUL", URtoUL);
                Tools.TrySerializeTable("UBtoDF", UBtoDF);
                Tools.TrySerializeTable("MergeURtoULandUBtoDF", MergeURtoULandUBtoDF);

                Tools.TrySerializeSbyteArray("Slice_URFtoDLF_Parity_Prun", Slice_URFtoDLF_Parity_Prun);
                Tools.TrySerializeSbyteArray("Slice_URtoDF_Parity_Prun", Slice_URtoDF_Parity_Prun);
                Tools.TrySerializeSbyteArray("Slice_Twist_Prun", Slice_Twist_Prun);
                Tools.TrySerializeSbyteArray("Slice_Flip_Prun", Slice_Flip_Prun);
            }
        }

        static CubieCube Copy(CubieCube c)
        {
            return new CubieCube(
                (Corner[])c.cp.Clone(),
                (byte[])c.co.Clone(),
                (Edge[])c.ep.Clone(),
                (byte[])c.eo.Clone()
            );
        }

        static void GenerateAll()
        {
            GenerateMoveTables();
            GenerateMergeTable();
            GeneratePruningTables();
        }

        static void GenerateMoveTables()
        {
            // twist move table
            twist = new short[CoordCube.N_TWIST, CoordCube.N_MOVE];
            CubieCube a = new CubieCube();
            for (short i = 0; i < CoordCube.N_TWIST; i++)
            {
                a.setTwist(i);
                for (int face = 0; face < 6; face++)
                {
                    CubieCube b = Copy(a);
                    b.cornerMultiply(CubieCube.moveCube[face]);
                    twist[i, 3 * face] = b.getTwist();
                    b.cornerMultiply(CubieCube.moveCube[face]);
                    twist[i, 3 * face + 1] = b.getTwist();
                    b.cornerMultiply(CubieCube.moveCube[face]);
                    twist[i, 3 * face + 2] = b.getTwist();
                }
            }

            // flip move table
            flip = new short[CoordCube.N_FLIP, CoordCube.N_MOVE];
            for (short i = 0; i < CoordCube.N_FLIP; i++)
            {
                a.setFlip(i);
                for (int face = 0; face < 6; face++)
                {
                    CubieCube b = Copy(a);
                    b.edgeMultiply(CubieCube.moveCube[face]);
                    flip[i, 3 * face] = b.getFlip();
                    b.edgeMultiply(CubieCube.moveCube[face]);
                    flip[i, 3 * face + 1] = b.getFlip();
                    b.edgeMultiply(CubieCube.moveCube[face]);
                    flip[i, 3 * face + 2] = b.getFlip();
                }
            }

            // FRtoBR move table
            FRtoBR = new short[CoordCube.N_FRtoBR, CoordCube.N_MOVE];
            for (short i = 0; i < CoordCube.N_FRtoBR; i++)
            {
                a.setFRtoBR(i);
                for (int face = 0; face < 6; face++)
                {
                    CubieCube b = Copy(a);
                    b.edgeMultiply(CubieCube.moveCube[face]);
                    FRtoBR[i, 3 * face] = b.getFRtoBR();
                    b.edgeMultiply(CubieCube.moveCube[face]);
                    FRtoBR[i, 3 * face + 1] = b.getFRtoBR();
                    b.edgeMultiply(CubieCube.moveCube[face]);
                    FRtoBR[i, 3 * face + 2] = b.getFRtoBR();
                }
            }

            // URFtoDLF move table
            URFtoDLF = new short[CoordCube.N_URFtoDLF, CoordCube.N_MOVE];
            for (short i = 0; i < CoordCube.N_URFtoDLF; i++)
            {
                a.setURFtoDLF(i);
                for (int face = 0; face < 6; face++)
                {
                    CubieCube b = Copy(a);
                    b.cornerMultiply(CubieCube.moveCube[face]);
                    URFtoDLF[i, 3 * face] = b.getURFtoDLF();
                    b.cornerMultiply(CubieCube.moveCube[face]);
                    URFtoDLF[i, 3 * face + 1] = b.getURFtoDLF();
                    b.cornerMultiply(CubieCube.moveCube[face]);
                    URFtoDLF[i, 3 * face + 2] = b.getURFtoDLF();
                }
            }

            // URtoUL move table
            URtoUL = new short[CoordCube.N_URtoUL, CoordCube.N_MOVE];
            for (short i = 0; i < CoordCube.N_URtoUL; i++)
            {
                a.setURtoUL(i);
                for (int face = 0; face < 6; face++)
                {
                    CubieCube b = Copy(a);
                    b.edgeMultiply(CubieCube.moveCube[face]);
                    URtoUL[i, 3 * face] = b.getURtoUL();
                    b.edgeMultiply(CubieCube.moveCube[face]);
                    URtoUL[i, 3 * face + 1] = b.getURtoUL();
                    b.edgeMultiply(CubieCube.moveCube[face]);
                    URtoUL[i, 3 * face + 2] = b.getURtoUL();
                }
            }

            // UBtoDF move table
            UBtoDF = new short[CoordCube.N_UBtoDF, CoordCube.N_MOVE];
            for (short i = 0; i < CoordCube.N_UBtoDF; i++)
            {
                a.setUBtoDF(i);
                for (int face = 0; face < 6; face++)
                {
                    CubieCube b = Copy(a);
                    b.edgeMultiply(CubieCube.moveCube[face]);
                    UBtoDF[i, 3 * face] = b.getUBtoDF();
                    b.edgeMultiply(CubieCube.moveCube[face]);
                    UBtoDF[i, 3 * face + 1] = b.getUBtoDF();
                    b.edgeMultiply(CubieCube.moveCube[face]);
                    UBtoDF[i, 3 * face + 2] = b.getUBtoDF();
                }
            }

            // URtoDF move table
            URtoDF = new short[CoordCube.N_URtoDF, CoordCube.N_MOVE];
            for (short i = 0; i < CoordCube.N_URtoDF; i++)
            {
                a.setURtoDF(i);
                for (int face = 0; face < 6; face++)
                {
                    CubieCube b = Copy(a);
                    b.edgeMultiply(CubieCube.moveCube[face]);
                    URtoDF[i, 3 * face] = (short)b.getURtoDF();
                    b.edgeMultiply(CubieCube.moveCube[face]);
                    URtoDF[i, 3 * face + 1] = (short)b.getURtoDF();
                    b.edgeMultiply(CubieCube.moveCube[face]);
                    URtoDF[i, 3 * face + 2] = (short)b.getURtoDF();
                }
            }
        }

        static void GenerateMergeTable()
        {
            MergeURtoULandUBtoDF = new short[336, 336];
            for (short uRtoUL = 0; uRtoUL < 336; uRtoUL++)
            {
                for (short uBtoDF = 0; uBtoDF < 336; uBtoDF++)
                {
                    CubieCube a = new CubieCube();
                    a.setURtoUL(uRtoUL);
                    a.setUBtoDF(uBtoDF);
                    MergeURtoULandUBtoDF[uRtoUL, uBtoDF] = (short)a.getURtoDF();
                }
            }
        }

        static void GeneratePruningTables()
        {
            // Phase1 pruning tables
            Slice_Twist_Prun = new sbyte[CoordCube.N_SLICE1 * CoordCube.N_TWIST / 2];
            Slice_Flip_Prun = new sbyte[CoordCube.N_SLICE1 * CoordCube.N_FLIP / 2];

            for (int i = 0; i < Slice_Twist_Prun.Length; i++) Slice_Twist_Prun[i] = -1;
            for (int i = 0; i < Slice_Flip_Prun.Length; i++) Slice_Flip_Prun[i] = -1;

            // solved state
            CoordCube.setPruning(Slice_Twist_Prun, 0, 0);
            CoordCube.setPruning(Slice_Flip_Prun, 0, 0);

            int done = 1;
            for (int depth = 0; done < CoordCube.N_SLICE1 * CoordCube.N_TWIST; depth++)
            {
                for (int i = 0; i < CoordCube.N_SLICE1 * CoordCube.N_TWIST; i++)
                {
                    if (CoordCube.getPruning(Slice_Twist_Prun, i) == depth)
                    {
                        int slice = i / CoordCube.N_TWIST;
                        int twistCoord = i % CoordCube.N_TWIST;
                        for (int move = 0; move < CoordCube.N_MOVE; move++)
                        {
                            int newSlice = FRtoBR[slice * 24, move] / 24;
                            int newTwist = twist[twistCoord, move];
                            int idx = newSlice * CoordCube.N_TWIST + newTwist;
                            if (CoordCube.getPruning(Slice_Twist_Prun, idx) == -1)
                            {
                                CoordCube.setPruning(Slice_Twist_Prun, idx, (sbyte)(depth + 1));
                                done++;
                            }
                        }
                    }
                }
            }

            done = 1;
            for (int depth = 0; done < CoordCube.N_SLICE1 * CoordCube.N_FLIP; depth++)
            {
                for (int i = 0; i < CoordCube.N_SLICE1 * CoordCube.N_FLIP; i++)
                {
                    if (CoordCube.getPruning(Slice_Flip_Prun, i) == depth)
                    {
                        int slice = i / CoordCube.N_FLIP;
                        int flipCoord = i % CoordCube.N_FLIP;
                        for (int move = 0; move < CoordCube.N_MOVE; move++)
                        {
                            int newSlice = FRtoBR[slice * 24, move] / 24;
                            int newFlip = flip[flipCoord, move];
                            int idx = newSlice * CoordCube.N_FLIP + newFlip;
                            if (CoordCube.getPruning(Slice_Flip_Prun, idx) == -1)
                            {
                                CoordCube.setPruning(Slice_Flip_Prun, idx, (sbyte)(depth + 1));
                                done++;
                            }
                        }
                    }
                }
            }

            // Phase2 pruning tables
            int size1 = CoordCube.N_SLICE2 * CoordCube.N_URFtoDLF * CoordCube.N_PARITY;
            int size2 = CoordCube.N_SLICE2 * CoordCube.N_URtoDF * CoordCube.N_PARITY;

            Slice_URFtoDLF_Parity_Prun = new sbyte[size1 / 2];
            Slice_URtoDF_Parity_Prun = new sbyte[size2 / 2];

            for (int i = 0; i < Slice_URFtoDLF_Parity_Prun.Length; i++) Slice_URFtoDLF_Parity_Prun[i] = -1;
            for (int i = 0; i < Slice_URtoDF_Parity_Prun.Length; i++) Slice_URtoDF_Parity_Prun[i] = -1;

            CoordCube.setPruning(Slice_URFtoDLF_Parity_Prun, 0, 0);
            CoordCube.setPruning(Slice_URtoDF_Parity_Prun, 0, 0);

            done = 1;
            for (int depth = 0; done < size1; depth++)
            {
                for (int i = 0; i < size1; i++)
                {
                    if (CoordCube.getPruning(Slice_URFtoDLF_Parity_Prun, i) == depth)
                    {
                        int slice = i / (CoordCube.N_URFtoDLF * CoordCube.N_PARITY);
                        int rest = i % (CoordCube.N_URFtoDLF * CoordCube.N_PARITY);
                        int urfToDlf = rest / CoordCube.N_PARITY;
                        int parity = rest % CoordCube.N_PARITY;

                        for (int move = 0; move < CoordCube.N_MOVE; move++)
                        {
                            // Only phase2 moves: U,D,R2,F2,L2,B2 => move index 0..17
                            // In standard mapping, phase2 allowed moves are: U (0-2), D (9-11), and quarter turns for R,F,L,B are not allowed.
                            // Here we skip moves with odd power for faces R,F,L,B.
                            int face = move / 3;
                            int power = move % 3;
                            if ((face == 1 || face == 2 || face == 4 || face == 5) && power != 1) // allow only 180 (power==1) for R,F,L,B
                            {
                                continue;
                            }

                            int newSlice = FRtoBR[slice, move] % 24;
                            int newUrfToDlf = URFtoDLF[urfToDlf, move];
                            int newParity = CoordCube.parityMove[parity][move];
                            int idx = (newSlice * CoordCube.N_URFtoDLF + newUrfToDlf) * CoordCube.N_PARITY + newParity;
                            if (CoordCube.getPruning(Slice_URFtoDLF_Parity_Prun, idx) == -1)
                            {
                                CoordCube.setPruning(Slice_URFtoDLF_Parity_Prun, idx, (sbyte)(depth + 1));
                                done++;
                            }
                        }
                    }
                }
            }

            done = 1;
            for (int depth = 0; done < size2; depth++)
            {
                for (int i = 0; i < size2; i++)
                {
                    if (CoordCube.getPruning(Slice_URtoDF_Parity_Prun, i) == depth)
                    {
                        int slice = i / (CoordCube.N_URtoDF * CoordCube.N_PARITY);
                        int rest = i % (CoordCube.N_URtoDF * CoordCube.N_PARITY);
                        int urToDf = rest / CoordCube.N_PARITY;
                        int parity = rest % CoordCube.N_PARITY;

                        for (int move = 0; move < CoordCube.N_MOVE; move++)
                        {
                            int face = move / 3;
                            int power = move % 3;
                            if ((face == 1 || face == 2 || face == 4 || face == 5) && power != 1)
                            {
                                continue;
                            }

                            int newSlice = FRtoBR[slice, move] % 24;
                            int newUrToDf = URtoDF[urToDf, move];
                            int newParity = CoordCube.parityMove[parity][move];
                            int idx = (newSlice * CoordCube.N_URtoDF + newUrToDf) * CoordCube.N_PARITY + newParity;
                            if (CoordCube.getPruning(Slice_URtoDF_Parity_Prun, idx) == -1)
                            {
                                CoordCube.setPruning(Slice_URtoDF_Parity_Prun, idx, (sbyte)(depth + 1));
                                done++;
                            }
                        }
                    }
                }
            }
        }
    }
}