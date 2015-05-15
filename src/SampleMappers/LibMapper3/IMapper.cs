using System.Collections.Generic;

namespace LibMapper3
{
    public interface IMapper
    {
        IList<KeyValuePair<string, string>> Map(string fileLine);
    }
}