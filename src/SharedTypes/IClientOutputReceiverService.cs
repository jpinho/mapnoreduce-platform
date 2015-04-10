namespace SharedTypes
{
    public interface IClientOutputReceiverService
    {
        void ReceiveMapOutputFragment(string filename, string[] result, int splitNumber);
    }
}