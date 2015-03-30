using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedTypes
{
    public interface IJobTracker
    {
        void Alive(String wid);
        void Complete(String wid);
        void Beat();
    }
}
