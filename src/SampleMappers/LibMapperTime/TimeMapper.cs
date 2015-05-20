using System.Collections.Generic;
using System.Threading;

namespace LibMapperTime
{
    public class TimeMapper : IMapper
    {
        private static uint lineNo = 0;
        public IList<KeyValuePair<string, string>> Map(string fileLine) {
            lineNo++;
            Thread.Sleep(1000);
            IList<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            result.Add(new KeyValuePair<string, string>(lineNo.ToString(), fileLine));
            return result;
        }
    }
}