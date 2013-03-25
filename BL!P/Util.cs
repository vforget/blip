using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Blip
{
    class Util
    {
        public static bool SetupDirectories(string projectDir)
        {
            string xmlDir = projectDir + "\\xml";
            string gbDir = projectDir + "\\gb";
            string txtDir = projectDir + "\\txt";
            try
            {
                if (!Directory.Exists(xmlDir))
                {
                    Directory.CreateDirectory(xmlDir);
                }
                if (!Directory.Exists(gbDir))
                {
                    Directory.CreateDirectory(gbDir);
                }
                if (!Directory.Exists(txtDir))
                {
                    Directory.CreateDirectory(txtDir);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
