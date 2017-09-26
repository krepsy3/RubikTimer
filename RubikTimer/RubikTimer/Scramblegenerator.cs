using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubikTimer
{
    class ScrambleGenerator
    {
        public static string[] Type = { "Other", "2x2x2 Cube", "3x3x3 Cube", "4x4x4 cube", "5x5x5 cube", "6x6x6 cube", "7x7x7 cube", "Square-1", "Pyraminx", "Megaminx" };
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
                    return CubeI(length);

                case 3:
                case 4:
                    return CubeII(length);

                case 7:
                    return SquareOne(length);

                case 8:
                    return Pyraminx(length);
            }
        }

        private string CubeI(byte length)
        {
            string[] moves = {
                "R", "R'", "R2",
                "L", "L'", "L2",
                "U", "U'", "U2",
                "D", "D'", "D2",
                "F", "F'", "F2",
                "B", "B'", "B2" };
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

        private string CubeII(byte length)
        {
            string[] moves = {
                "R", "R'", "R2", "r", "r'","r2", "Rr", "Rr'", "Rr2",
                "L", "L'", "L2", "l", "l'","l2", "Ll", "Ll'", "Ll2",
                "U", "U'", "U2", "u", "u'","u2", "Uu", "Uu'", "Uu2",
                "D", "D'", "D2", "d", "d'","d2", "Dd", "Dd'", "Dd2",
                "F", "F'", "F2", "f", "f'","f2", "Ff", "Ff'", "Ff2",
                "B", "B'", "B2", "b", "b'","r2", "Bb", "Bb'", "Bb2" };
            string result = "";

            int lastgroup = 0;
            int group = 0;

            for (byte b = 0; b < length; b++)
            {
                while (group == lastgroup) group = random.Next(0, 6);
                result += moves[((group * 9) + random.Next(0, 9))];
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