using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubikTimer
{
    class ScrambleGenerator
    {
        public static string[] Type = { "Other", "2x2x2 Cube", "3x3x3 Cube", "Square-1", "Pyraminx" };
        private Random random;

        public ScrambleGenerator()
        {
            random = new Random();
        }

        public string Generate(byte type, byte length)
        {
            switch (type)
            {
                case 0:
                default:
                    return "";

                case 1:
                case 2:
                    return Cube(length);

                case 3:
                    return SquareOne(length);

                case 4:
                    return Pyraminx(length);
            }
        }

        private string Cube(byte length)
        {
            string[] moves = { "R", "R'", "R2", "L", "L'", "L2", "U", "U'", "U2", "D", "D'", "D2", "F", "F'", "F2", "B", "B'", "B2" };
            string result = "";

            int lastgroup = 0;
            int group = 0;

            for (byte b = 0; b < length; b++)
            {
                while (group == lastgroup) group = random.Next(0, 6);
                result += moves[((group * 3) + random.Next(0, 3))];
                result += " ";
                lastgroup = group;
            }

            return result;
        }

        private string Pyraminx(byte length)
        {
        string[] moves = { "R", "R'", "L", "L'", "F", "F'", "B", "B'" };
        string result = "";

            int lastgroup = 0;
            int group = 0;

            for (byte b = 0; b < length; b++)
            {
                while (group == lastgroup) group = random.Next(0, 4);
                result += moves[((group * 2) + random.Next(0, 1))];
                result += " ";
                lastgroup = group;
            }

            result.Remove(result.Length - 1);

            foreach(char c in new char[] { 'r', 'l', 'f', 'b'})
            {
                byte b = (byte)random.Next(0, 4);
                result += b > 1 ? (" " + c.ToString()) : "";
                result += b > 2 ? "'" : "";
            }

            return result;
        }

        private string SquareOne(byte lenght)
        {
            string result = "";

            for (byte b = 0; b < lenght; b++)
            {
                short up = (short)random.Next(-6, 7);
                short down = (short)random.Next(-6, 7);
                while (up == 0 && down == 0) down = (short)random.Next(-6, 7);

                result += ("(" + up + "," + down + ") / ");
            }
            result = result.Remove(result.Length - 3);
            
            return result;
        }
    }
}
