using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTTGC.Prat.Common
{
    public static class Util
    {
        public static bool Get(this Dictionary<string, bool> dict, string key)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }

            return false;
        }
    }
}
