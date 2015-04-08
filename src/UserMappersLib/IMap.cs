using System;
using System.Collections.Generic;

namespace UserMappersLib
{
    public interface IMap
    {
        IList<KeyValuePair<String, String>> Map(string fileLine);
    }
}