namespace SharedTypes
{
	public interface IJobTracker
	{
		void Run();
		void ScheduleJob(IJobTask job);
		void Alive(int wid);
		void FreezeCommunication();
		void UnfreezeCommunication();
	}
}