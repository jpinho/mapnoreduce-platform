using System.Collections.Generic;

namespace SharedTypes
{
    public interface IMapper
    {
        IList<KeyValuePair<string, string>> Map(string fileLine);
    }
}