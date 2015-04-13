namespace SharedTypes
{
	public interface IJobTracker
	{
		void ScheduleJob(IJobTask job);
		void Alive(int wid);
		void FreezeCommunication();
		void UnfreezeCommunication();
	}
}