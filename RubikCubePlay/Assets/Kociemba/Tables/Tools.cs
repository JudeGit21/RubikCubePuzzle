using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace Kociemba
{
    public class Tools
    {
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Check if the cube string s represents a solvable cube.
        // 0: Cube is solvable
        // -1: There is not exactly one facelet of each colour
        // -2: Not all 12 edges exist exactly once
        // -3: Flip error: One edge has to be flipped
        // -4: Not all corners exist exactly once
        // -5: Twist error: One corner has to be twisted
        // -6: Parity error: Two corners or two edges have to be exchanged
        //
        /// <summary>
        /// Check if the cube definition string s represents a solvable cube.
        /// </summary>
        /// <param name="s"> is the cube definition string , see <seealso cref="Facelet"/> </param>
        /// <returns> 0: Cube is solvable<br>
        ///         -1: There is not exactly one facelet of each colour<br>
        ///         -2: Not all 12 edges exist exactly once<br>
        ///         -3: Flip error: One edge has to be flipped<br>
        ///         -4: Not all 8 corners exist exactly once<br>
        ///         -5: Twist error: One corner has to be twisted<br>
        ///         -6: Parity error: Two corners or two edges have to be exchanged </returns>
        public static int verify(string s)
        {
            int[] count = new int[6];
            try
            {
                for (int i = 0; i < 54; i++)
                {
                    count[(int)CubeColor.Parse(typeof(CubeColor), i.ToString())]++;
                }
            }
            catch (Exception)
            {
                return -1;
            }
            for (int i = 0; i < 6; i++)
            {
                if (count[i] != 9)
                {
                    return -1;
                }
            }

            FaceCube fc = new FaceCube(s);
            CubieCube cc = fc.toCubieCube();

            return cc.verify();
        }

        /// <summary>
        /// Generates a random cube. </summary>
        /// <returns> A random cube in the string representation. Each cube of the cube space has the same probability. </returns>
        public static string randomCube()
        {
            CubieCube cc = new CubieCube();
            System.Random gen = new System.Random();
            cc.setFlip((short)gen.Next(CoordCube.N_FLIP));
            cc.setTwist((short)gen.Next(CoordCube.N_TWIST));
            do
            {
                cc.setURFtoDLB(gen.Next(CoordCube.N_URFtoDLB));
                cc.setURtoBR(gen.Next(CoordCube.N_URtoBR));
            } while ((cc.edgeParity() ^ cc.cornerParity()) != 0);
            FaceCube fc = cc.toFaceCube();
            return fc.to_fc_String();
        }


        // <https://stackoverflow.com/questions/7742519/c-sharp-export-write-multidimension-array-to-file-csv-or-whatever>
        // Kristian Fenn: <https://stackoverflow.com/users/989539/kristian-fenn>

        
        // =====================================================================================
        // Table cache (Unity friendly)
        // =====================================================================================

        static string GetTablesFolder()
        {
            // Default: store in a writable folder.
            // In Unity this becomes persistentDataPath/Kociemba/Tables.
            // Outside Unity it falls back to current directory ./Kociemba/Tables.
            string baseDir = Directory.GetCurrentDirectory();

#if UNITY_5_3_OR_NEWER
            try
            {
                baseDir = UnityEngine.Application.persistentDataPath;
            }
            catch { }
#endif

            return Path.Combine(baseDir, "Kociemba", "Tables");
        }

        static string GetTablePath(string filename)
        {
            return Path.Combine(GetTablesFolder(), filename);
        }

        public static bool TrySerializeTable(string filename, short[,] array)
        {
            try
            {
                SerializeTable(filename, array);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TrySerializeSbyteArray(string filename, sbyte[] array)
        {
            try
            {
                SerializeSbyteArray(filename, array);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryDeserializeTable(string filename, out short[,] array)
        {
            try
            {
                array = DeserializeTable(filename);
                return true;
            }
            catch
            {
                array = null;
                return false;
            }
        }

        public static bool TryDeserializeSbyteArray(string filename, out sbyte[] array)
        {
            try
            {
                array = DeserializeSbyteArray(filename);
                return true;
            }
            catch
            {
                array = null;
                return false;
            }
        }

        public static void SerializeTable(string filename, short[,] array)
        {
            string folder = GetTablesFolder();
            Directory.CreateDirectory(folder);

            BinaryFormatter bf = new BinaryFormatter();
            using (Stream s = File.Open(GetTablePath(filename), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                bf.Serialize(s, array);
            }
        }

        public static short[,] DeserializeTable(string filename)
        {
            string path = GetTablePath(filename);

            // If not in cache, try to read from StreamingAssets (Unity) as a read-only source.
#if UNITY_5_3_OR_NEWER
            if (!File.Exists(path))
            {
                string sa = Path.Combine(UnityEngine.Application.streamingAssetsPath, "Kociemba", "Tables", filename);
                if (File.Exists(sa))
                {
                    path = sa;
                }
            }
#endif
            BinaryFormatter bf = new BinaryFormatter();
            using (Stream s = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return (short[,])bf.Deserialize(s);
            }
        }

        public static void SerializeSbyteArray(string filename, sbyte[] array)
        {
            string folder = GetTablesFolder();
            Directory.CreateDirectory(folder);

            BinaryFormatter bf = new BinaryFormatter();
            using (Stream s = File.Open(GetTablePath(filename), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                bf.Serialize(s, array);
            }
        }

        public static sbyte[] DeserializeSbyteArray(string filename)
        {
            string path = GetTablePath(filename);

#if UNITY_5_3_OR_NEWER
            if (!File.Exists(path))
            {
                string sa = Path.Combine(UnityEngine.Application.streamingAssetsPath, "Kociemba", "Tables", filename);
                if (File.Exists(sa))
                {
                    path = sa;
                }
            }
#endif

            BinaryFormatter bf = new BinaryFormatter();
            using (Stream s = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return (sbyte[])bf.Deserialize(s);
            }
        }

    }
}
