using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedTypes
{
    public interface ICommand
    {
        void Execute();
        string ToString();
    }
}
