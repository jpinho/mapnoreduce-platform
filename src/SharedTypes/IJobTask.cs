namespace SharedTypes
{
    public interface IJobTask
    {
        string FileName { get; set; }
        string MapClassName { get; set; }
        byte[] MapFunctionAssembly { get; set; }
        string OutputReceiverURL { get; set; }
        int SplitNumber { get; set; }
        string SplitProviderURL { get; set; }
    }
}