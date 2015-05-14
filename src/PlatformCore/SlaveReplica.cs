using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading;
using SharedTypes;

namespace PlatformCore
{
    [Serializable]
    public class SlaveReplica : IDisposable, ISlaveReplica
    {
        public const int PING_DELAY = 5000;
        private const int RECOVERY_ATTEMPT_DELAY = PING_DELAY * 3;
        private const int MAX_FAILED_HEARTBEATS_BEF_RECOVER = 3;

        private readonly Timer heartbeat;
        private readonly Timer masterRecovery;
        private bool isInitialized;
        private int failedHeartbeatAttempts;
        private bool inRecovery;

        public readonly Worker Worker;
        public Tuple<JobTrackerStateInfo, DateTime> MasterJobTrackerState { get; private set; }
        public bool Enabled { get; set; }
        public int Priority { get; set; }
        public List<SlaveReplica> Siblings { get; set; }

        private Dictionary< /*worker/replica uri*/ string, SlaveReplica> replicasRecoveryStates;

        public SlaveReplica(Worker worker) {
            Worker = worker;
            heartbeat = new Timer(SendHeartbeat, null, Timeout.Infinite, PING_DELAY);
            masterRecovery = new Timer(MasterRecoveryProcedure, null, Timeout.Infinite, RECOVERY_ATTEMPT_DELAY);
        }

        private void MasterRecoveryProcedure(object state) {
            Siblings.Sort((r1, r2) => r1.Priority.CompareTo(r2.Priority));

            if (Siblings == null || Siblings.Count == 0 || Priority < Siblings.First().Priority) {
                Trace.WriteLine("MasterRecoveryProcedure on replica '" + Worker.WorkerId + "' flowing to RecoverJobTracker routine.");
                RecoverJobTracker();
                return;
            }

            var proxyRepMaster = RemotingHelper.GetRemoteObject<IWorker>(Siblings.First().Worker.ServiceUrl);
            proxyRepMaster.SendReplicaState(this);
        }

        private void RecoverJobTracker() {
            throw new NotImplementedException();
        }

        private void Init() {
            isInitialized = true;
            heartbeat.Change(0, PING_DELAY);
        }

        private void SendHeartbeat(object state) {
            var error = false;
            try {
                Trace.WriteLine("JobTrackerSlave: Sending Ping to JobTrackerMaster - WorkerID:" + Worker.WorkerId + ".");
                var masterTracker = RemotingHelper.GetRemoteObject<TaskRunner>(MasterJobTrackerState.Item1.ServiceUri);
                masterTracker.AliveReplica(Worker.WorkerId);

                if (inRecovery) {
                    Trace.WriteLine("Job Tracker Master recovery completed!");
                    inRecovery = false;
                    masterRecovery.Change(Timeout.Infinite, RECOVERY_ATTEMPT_DELAY);
                }
            } catch (RemotingException) {
                error = true;
                Trace.WriteLine("SlaveReplica heartbeat failed, master tracker seems to be offline!");
            } catch (System.Exception ex) {
                error = true;
                Trace.WriteLine("SlaveReplica heartbeat failed, unknown error: '" + ex.Message + "'.");
            } finally {
                if (error) {
                    if (++failedHeartbeatAttempts >= MAX_FAILED_HEARTBEATS_BEF_RECOVER && !inRecovery) {
                        Trace.WriteLine("SlaveReplica '" + Worker.WorkerId + "' starting recovery procedure.");
                        inRecovery = true;
                        masterRecovery.Change(0, RECOVERY_ATTEMPT_DELAY);
                    } else
                        Trace.WriteLine("SlaveReplica in recovery, and still heartbeat is failing... nothing new!");
                }
            }
        }

        public void UpdateReplicas(List<SlaveReplica> replicasGroup) {
            Siblings = replicasGroup.FindAll(r => r.Worker.ServiceUrl != Worker.ServiceUrl);
        }

        public void SaveState(JobTrackerStateInfo state) {
            if (!isInitialized)
                Init();
            MasterJobTrackerState = new Tuple<JobTrackerStateInfo, DateTime>(state, DateTime.Now);
            Trace.WriteLine("JobTrackerSlave: SaveSate at '" + DateTime.Now.ToString("dd-MM-yyyyTHH:mm:ss:fff") + "'.");
        }

        public void Dispose() {
            heartbeat.Dispose();
            MasterJobTrackerState = null;
        }
    }
}