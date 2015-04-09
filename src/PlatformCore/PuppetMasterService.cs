﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using SharedTypes;

namespace PlatformCore
{
    public class PuppetMasterService : MarshalByRefObject, IPuppetMasterService
    {
        private readonly Dictionary<int, IWorker> workers = new Dictionary<int, IWorker>();
        public static readonly Uri ServiceUrl = new Uri("tcp://localhost:9008/MNRP-PuppetMasterService");

        public PuppetMasterService() {
            // Required for .NET Remoting Proxy Classes.
        }

        public void CreateWorker(int workerId, string serviceUrl, string entryUrl) {
            var worker = Worker.Run(workerId, new Uri(serviceUrl), workers);
            workers.Add(workerId, worker);
            worker.UpdateAvailableWorkers(workers);

            Trace.WriteLine(string.Format("New worker created: id '{0}', url '{1}'."
                , workerId, worker.ServiceUrl));

            if (!string.IsNullOrWhiteSpace(entryUrl))
                NotifyWorkerCreation(worker);
        }

        public void GetStatus() {
            foreach (IWorker worker in workers.Values) {
                worker.GetStatus();
            }

        }

        public Dictionary<int, IWorker> GetWorkers() {
            return workers;
        }

        public void Wait(int seconds) {
            Thread.Sleep(seconds * 1000);
        }

        public void SlowWorker(int WorkerId, int seconds) {
            IWorker worker;

            try {
                worker = workers[WorkerId];
            } catch (Exception e) {
                throw new InvalidWorkerIdException(WorkerId, e);
            }

            worker.Slow(seconds);
        }

        public void FreezeWorker(int WorkerId)
        {
            IWorker worker;

            try
            {
                worker = workers[WorkerId];
            }
            catch (Exception e)
            {
                throw new InvalidWorkerIdException(WorkerId, e);
            }

            worker.Freeze();
        }

        public void UnfreezeWorker(int WorkerId)
        {
            IWorker worker;

            try
            {
                worker = workers[WorkerId];
            }
            catch (Exception e)
            {
                throw new InvalidWorkerIdException(WorkerId, e);
            }

            worker.UnFreeze();
        }

        public void FreezeCommunication(int WorkerId) {
            IWorker worker;

            try
            {
                worker = workers[WorkerId];
            }
            catch (Exception e)
            {
                throw new InvalidWorkerIdException(WorkerId, e);
            }

            worker.FreezeCommunication();
        }

        public void UnfreezeCommunication(int WorkerId) {
            IWorker worker;

            try
            {
                worker = workers[WorkerId];
            }
            catch (Exception e)
            {
                throw new InvalidWorkerIdException(WorkerId, e);
            }

            worker.UnfreezeCommunication();
        }

        private void NotifyWorkerCreation(Worker worker) {
            Trace.WriteLine("Sends notification to worker at ENTRY_URL informing worker creation.");
            //TODO: Contact worker at ENTRY_URL and announce new worker available.
        }

        /// <summary>
        /// Serves a Marshalled Puppet Master object at a specific IChannel under ChannelServices.
        /// </summary>
        public static void Run() {
            PuppetMasterService service = new PuppetMasterService();
            RemotingHelper.CreateService(service, ServiceUrl);
            Trace.WriteLine("Puppet Master Service listening at '" + ServiceUrl + "'");
        }
    }
}
