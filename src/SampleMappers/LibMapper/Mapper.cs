using System.Collections.Generic;

namespace LibMapper
{
    public class Mapper : IMapper
    {
        public IList<KeyValuePair<string, string>> Map(string fileLine) {
            IList<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            result.Add(new KeyValuePair<string, string>("testKey1", fileLine));
            result.Add(new KeyValuePair<string, string>("testKey2", "anotherValue"));
            return result;
        }
    }
}