using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowSplineTool
{
    class Vertex
    {
        public float X;
        public float Y;
        public float Z;

        public Vertex()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }
        public Vertex(float a, float b, float c)
        {
            X = a;
            Y = b;
            Z = c;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            string[] Arguments = Environment.GetCommandLineArgs();

            Console.WriteLine("============================================");
            Console.WriteLine("| Shadow Spline Tool by igorseabra4");
            Console.WriteLine("| Usage: drag Shadow spline files into the executable to convert them to .OBJ");
            Console.WriteLine("| Just opening the program will convert every file found in the folder.");
            Console.WriteLine("============================================");

            if (Arguments.Length <= 1)
                Arguments = Directory.GetFiles(Directory.GetCurrentDirectory());

            foreach (string i in Arguments)
                if (Path.GetExtension(i).ToLower() == ".ptp")
                    ConvertPTPToOBJ(i);
            Console.ReadKey();
        }

        private static int Switch(int a)
        {
            byte[] b = BitConverter.GetBytes(a);
            b = new byte[] { b[3], b[2], b[1], b[0] };
            return BitConverter.ToInt32(b, 0);
        }

        private static float Switch(float a)
        {
            byte[] b = BitConverter.GetBytes(a);
            b = new byte[] { b[3], b[2], b[1], b[0] };
            return BitConverter.ToSingle(b, 0);
        }

        private static void ConvertPTPToOBJ(string FileName)
        {
            Console.WriteLine("Reading " + FileName);

            List<List<Vertex>> SplineList = new List<List<Vertex>>();

            BinaryReader SPLReader = new BinaryReader(new FileStream(FileName, FileMode.Open));

            SPLReader.BaseStream.Position = 0x8;
            int AmountOfSplines = Switch(SPLReader.ReadInt32()) / 4;

            SPLReader.BaseStream.Position = 0x20;
            int[] PointerList = new int[AmountOfSplines];
            for (int i = 0; i < AmountOfSplines; i++)
            {
                PointerList[i] = Switch(SPLReader.ReadInt32()) + 0x20;
            }

            foreach (int i in PointerList)
            {
                SPLReader.BaseStream.Position = i;
                int AmountOfPoints = Switch(SPLReader.ReadInt32());
                SPLReader.BaseStream.Position += 0x2C;
                List<Vertex> VertexList = new List<Vertex>();
                for (int j = 0; j < AmountOfPoints; j++)
                {
                    VertexList.Add(new Vertex(Switch(SPLReader.ReadSingle()), Switch(SPLReader.ReadSingle()), Switch(SPLReader.ReadSingle())));
                    SPLReader.BaseStream.Position += 0x14;
                }
                SplineList.Add(VertexList);
            }

            Console.WriteLine("Creating " + Path.ChangeExtension(FileName, "obj"));
            StreamWriter SplineOBJWriter = new StreamWriter(new FileStream(Path.ChangeExtension(FileName, "obj"), FileMode.Create));
            SplineOBJWriter.WriteLine("#Splines exported by Shadow Spline Tool");
            SplineOBJWriter.WriteLine();

            int TotalCount = 0;

            foreach (List<Vertex> i in SplineList)
            {
                foreach (Vertex j in i)
                    SplineOBJWriter.WriteLine(string.Format("v {0} {1} {2}", j.X, j.Y, j.Z));
                SplineOBJWriter.WriteLine();

                string Everynumber = "";

                for (int j = TotalCount + 1; j <= TotalCount + i.Count; j++)
                    Everynumber += " " + j.ToString();

                SplineOBJWriter.WriteLine("g spline_" + Path.GetFileNameWithoutExtension(FileName).ToLower() + TotalCount.ToString());
                SplineOBJWriter.WriteLine("l" + Everynumber);
                SplineOBJWriter.WriteLine();
                TotalCount += i.Count;
            }

            SplineOBJWriter.Close();
            Console.WriteLine("Success.");
        }
    }
}
