using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedTypes
{
    public interface IJobTracker
    {
        void Alive(int wid);
        void Complete(int wid);
        void FreezeCommunication();
        void UnfreezeCommunication();
    }
}
