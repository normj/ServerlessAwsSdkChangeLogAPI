using System.IO;
using System.Text.Json;

namespace ServerlessAwsSdkChangeLogAPI.Common.Writers
{
    public class BaseJsonWriter
    {
        protected MemoryStream _buffer;
        protected Utf8JsonWriter _jsonWriter;
        
        public void Start()
        {
            _buffer = new MemoryStream();
            _jsonWriter = new Utf8JsonWriter(_buffer, new JsonWriterOptions
            {
                Indented = true
            });
            
            _jsonWriter.WriteStartArray();
        }
        
        public string Finish()
        {
            _jsonWriter.WriteEndArray();
            _jsonWriter.Flush();
            _buffer.Position = 0;
            
            return new StreamReader(_buffer).ReadToEnd();
        }        
    }
}