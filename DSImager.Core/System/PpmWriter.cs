using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSImager.Core.System
{
    public static class PpmWriter
    {

        public static void WritePPM(string filename, byte[] pbuf, int width, int height, int bpp)
        {
            FileStream fs = new FileStream(filename, FileMode.Create);

            char[] magic = new char[] { 'P', '6' };
            if (bpp == 1)
                magic[1] = '1';
            else if (bpp == 8)
                magic[1] = '5';
            else if (bpp >= 24)
                magic[1] = '6';

            int bytesPP = bpp / 8;

            BinaryWriter sw = new BinaryWriter(fs);
            sw.Write(magic);
            sw.Write(' ');
            sw.Write(Encoding.ASCII.GetBytes(width.ToString()));
            sw.Write(' ');
            sw.Write(Encoding.ASCII.GetBytes(height.ToString()));
            if (bpp >= 8)
            {
                sw.Write(' ');
                sw.Write(Encoding.ASCII.GetBytes("255"));
            }

            sw.Write('\n');
            if (bpp >= 8)
            {
                for (int i = 0; i < pbuf.Length; i += bytesPP)
                {
                    for (int p = 0; p < bytesPP; p++)
                        sw.Write(pbuf[i + p]);
                }
            }
            else if (bpp == 1)
            {
                List<string> pixels = new List<string>();
                for (int i = 0; i < pbuf.Length; i++)
                {
                    byte octet = pbuf[i];

                    for (int p = 0; p < 8; p++)
                    {
                        bool isOne = (octet & 1 << p) > 0;
                        string px = isOne ? "1" : "0";
                        pixels.Add(px);
                    }

                }
                sw.Write(Encoding.ASCII.GetBytes(string.Join(" ", pixels.ToArray())));
            }


            sw.Close();
        }
    }
}
