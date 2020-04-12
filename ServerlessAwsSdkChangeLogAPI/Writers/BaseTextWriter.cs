using System.Text;

namespace ServerlessAwsSdkChangeLogAPI.Writers
{
    public abstract class BaseTextWriter
    {
        protected StringBuilder _buffer;

        public void Start()
        {
            _buffer = new StringBuilder();
        }
        
        public string Finish()
        {
            return _buffer.ToString();
        }        
    }
}