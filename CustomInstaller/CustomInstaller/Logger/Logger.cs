using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CustomInstaller.Logger
{
    public static class Logger
    {
        private static void Write(string log)
        {
            using(System.IO.FileStream fs = new System.IO.FileStream("install.log",System.IO.FileMode.Append))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(log);
                }
            }
        }

        public static void Info(string msg)
        {
            try
            {
                Write(msg);
            }
            catch
            {

            }
        }

        public static void Error(string msg)
        {
            try
            {
                Write("[Error]" + msg);
            }
            catch
            {

            }
        }
    }
}
