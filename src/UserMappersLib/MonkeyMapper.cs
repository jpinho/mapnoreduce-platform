using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMappersLib
{
    public class MonkeyMapper : SharedTypes.IMap
    {
        #region IMap Members

        public IList<KeyValuePair<string, string>> Map(string fileLine) {
            IList<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            string[] values = fileLine.Split(' ');
            foreach (string val in values)
                result.Add(new KeyValuePair<string, string>(
                    val.GetHashCode().ToString("X"),
                    "dº.ºb # " + val.ToUpperInvariant()));
            return result;
        }

        #endregion IMap Members
    }
}