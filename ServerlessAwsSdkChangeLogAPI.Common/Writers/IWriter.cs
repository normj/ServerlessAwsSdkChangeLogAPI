namespace ServerlessAwsSdkChangeLogAPI.Common.Writers
{
    public interface IWriter
    {
        void Start();
        
        string Finish();
    }
}