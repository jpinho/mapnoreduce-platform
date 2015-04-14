namespace SharedTypes
{
	public enum WorkerStatus
	{
		Busy,
		ReceivingJob,
		Available,
        Frozen,
		Offline
	}

	public enum JobTrackerState
	{
		Busy,
		Available
	}

	public enum JobTrackerMode
	{
		Active,
		Passive
	}
}