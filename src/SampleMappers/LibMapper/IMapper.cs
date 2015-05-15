using System.Collections.Generic;

namespace LibMapper
{
    public interface IMapper
    {
        IList<KeyValuePair<string, string>> Map(string fileLine);
    }
}