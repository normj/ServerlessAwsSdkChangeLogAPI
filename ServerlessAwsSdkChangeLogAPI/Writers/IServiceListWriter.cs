using System.IO;
using System.Text;
using System.Text.Json;

namespace ServerlessAwsSdkChangeLogAPI.Writers
{
    public interface IServiceListWriter : IWriter
    {
        public void WriteService(string service);
    }

    public class TextServiceListWriter : BaseTextWriter, IServiceListWriter
    {
        public void WriteService(string service)
        {
            _buffer.AppendLine(service);
        }
    }
    
    public class JsonServiceListWriter : BaseJsonWriter, IServiceListWriter
    {
        public void WriteService(string service)
        {
            _jsonWriter.WriteStringValue(service);
        }
    }
}