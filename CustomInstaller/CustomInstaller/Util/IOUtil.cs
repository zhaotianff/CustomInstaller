using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CustomInstaller.Util
{
    public class IOUtil
    {
        public static void CreateDir(string dir)
        {
            try
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
            catch
            {

            }
        }

        public static string CheckSlash(string dir)
        {
            if (dir.EndsWith("/") || dir.EndsWith("\\"))
                return dir.Substring(0,dir.Length -1);    

            return dir;
        }

        public static void CreateSubDir(string rootDir,string fullPath)
        {
            var pathArray = fullPath.Split('/');

            var pathArrayLength = pathArray.Length;

            if (pathArrayLength == 1)
                return;

            if(pathArrayLength == 3)
            {

            }

            for (int i = 0; i < pathArrayLength-1; i++)
            {
                pathArray[i] = pathArray[i] + "\\";
            }

            for (int i = 0; i < pathArrayLength -1; i++)
            {
                var path = "";
                for (int j = 0; j <= i; j++)
                {
                    path += pathArray[j];
                }
                CreateDir(rootDir + "\\" + path);              
            }
        }
    }
}
