namespace SharedTypes
{
	public enum WorkerStatus
	{
		Busy,
		ReceivingJob,
		Available
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