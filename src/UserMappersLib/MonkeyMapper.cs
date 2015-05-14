using System.Collections.Generic;
using System.Linq;

namespace UserMappersLib
{
    public class MonkeyMapper : IMap
    {
        #region IMap Members

        public IList<KeyValuePair<string, string>> Map(string fileLine) {
            var values = fileLine.Split(' ');
            return values.Select(
                val => new KeyValuePair<string, string>(
                    val.GetHashCode().ToString("X"),
                    "dº.ºb # " + val.ToUpperInvariant())).ToList();
        }

        #endregion IMap Members
    }
}