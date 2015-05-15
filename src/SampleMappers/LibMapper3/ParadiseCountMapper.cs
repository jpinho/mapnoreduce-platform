using System.Collections.Generic;

namespace LibMapper3
{
    public class ParadiseCountMapper : IMapper
    {
        private static uint lineNo = 0;
        public IList<KeyValuePair<string, string>> Map(string fileLine) {
            lineNo++;
            IList<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            if (fileLine.Contains("Paradise")) {
                result.Add(new KeyValuePair<string, string>(lineNo.ToString(), "1"));
            }
            return result;
        }
    }
}