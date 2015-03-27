using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedTypes
{
    public interface IMap
    {
        IList<KeyValuePair<String, String>> Map(string fileLine);
    }
}