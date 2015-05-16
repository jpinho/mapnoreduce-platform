using System.Collections.Generic;

namespace LibMapper2
{
    public interface IMapper
    {
        IList<KeyValuePair<string, string>> Map(string fileLine);
    }
}