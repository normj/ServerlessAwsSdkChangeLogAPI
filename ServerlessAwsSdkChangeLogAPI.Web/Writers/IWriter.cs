namespace ServerlessAwsSdkChangeLogAPI.Web.Writers
{
    public interface IWriter
    {
        void Start();
        
        string Finish();
    }
}