using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace PuppetMasterLib
{
    public interface IMap
    {
        HashSet<KeyValuePair<String, String>> Map(String key, String value);
    }
}
