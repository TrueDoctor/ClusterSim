using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ClusterLib
{
    public static class Misc
    {
        public static int CountFiles(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            return di.GetFiles().Length;
        }
    }
}
