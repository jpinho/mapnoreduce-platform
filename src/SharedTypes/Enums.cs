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
		Available,
        Frozen
	}

	public enum JobTrackerMode
	{
		Active,
		Passive
	}
}