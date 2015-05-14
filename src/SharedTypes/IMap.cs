using System.Collections.Generic;

namespace SharedTypes
{
    public interface IMap
    {
        IList<KeyValuePair<string, string>> Map(string fileLine);
    }
}