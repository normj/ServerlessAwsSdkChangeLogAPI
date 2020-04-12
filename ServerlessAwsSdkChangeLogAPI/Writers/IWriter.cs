namespace ServerlessAwsSdkChangeLogAPI.Writers
{
    public interface IWriter
    {
        public void Start();
        
        public string Finish();
    }
}