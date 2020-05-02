using ServerlessAwsSdkChangeLogAPI.Web.Writers;

namespace ServerlessAwsSdkChangeLogAPI.Web.Writers
{
    public enum ResponseWriterType {Text, Json}
    
    public interface IResponseWriterFactory
    {
        IServiceListWriter GetServiceListWriter(ResponseWriterType writerType);

        IServiceFeatureListWriter GetServiceFeatureListWriter(ResponseWriterType writerType);
    }
    
    public class ResponseWriterFactory : IResponseWriterFactory
    {
        public IServiceListWriter GetServiceListWriter(ResponseWriterType writerType)
        {
            switch (writerType)
            {
                case ResponseWriterType.Text:
                    return new TextServiceListWriter();
                case ResponseWriterType.Json:
                    return new JsonServiceListWriter();
            }
            
            return new TextServiceListWriter();
        }
        
        public IServiceFeatureListWriter GetServiceFeatureListWriter(ResponseWriterType writerType)
        {
            switch (writerType)
            {
                case ResponseWriterType.Text:
                    return new TextServiceFeatureListWriter();
                case ResponseWriterType.Json:
                    return new JsonServiceFeatureListWriter();
            }
            
            return new TextServiceFeatureListWriter();
        }        
    }
}