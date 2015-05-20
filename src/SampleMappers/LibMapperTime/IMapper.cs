using System.Collections.Generic;

namespace LibMapperTime
{
    public interface IMapper
    {
        IList<KeyValuePair<string, string>> Map(string fileLine);
    }
}