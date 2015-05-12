namespace SharedTypes
{
	public interface IClientService
	{
		void Init(string entryUrl);

		void SubmitAsync(string filePath, int nSplits, string outputDir, string mapClassName, string assemblyFilePath);
	}
}