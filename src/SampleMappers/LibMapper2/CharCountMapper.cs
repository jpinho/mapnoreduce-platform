using System.Collections.Generic;

namespace LibMapper2
{
    public class CharCountMapper : IMapper
    {
        private static uint lineNo = 0;
        public IList<KeyValuePair<string, string>> Map(string fileLine) {
            CharCountMapper.lineNo++;
            IList<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            result.Add(new KeyValuePair<string, string>(CharCountMapper.lineNo.ToString(), fileLine.Length.ToString()));
            return result;
        }
    }
}