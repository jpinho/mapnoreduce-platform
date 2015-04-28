namespace SharedTypes
{
	public interface IClientSplitProviderService
	{
		string GetFileSplit(string filename, int splitNumber);
	}
}