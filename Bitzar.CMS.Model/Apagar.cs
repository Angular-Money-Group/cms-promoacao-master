using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MethodCache.Attributes
{
    public enum Members { All };

    public class Cache : Attribute
    {
        public Cache()
        {

        }

        public Cache(Members member)
        {

        }
    }

    public class NoCache : Attribute
    {
    }
}