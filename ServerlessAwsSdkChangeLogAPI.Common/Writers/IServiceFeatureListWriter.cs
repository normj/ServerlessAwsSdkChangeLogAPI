using System;

namespace ServerlessAwsSdkChangeLogAPI.Common.Writers
{
    public interface IServiceFeatureListWriter : IWriter
    {
        void StartRelease(string version, DateTime releaseDate);
        void EndRelease();

        void StartFeatures();
        void AddFeature(string feature);
        void EndFeatures();
    }

    public class TextServiceFeatureListWriter : BaseTextWriter, IServiceFeatureListWriter
    {
        public void StartRelease(string version, DateTime releaseDate)
        {
            _buffer.AppendLine($"Version {version} released {releaseDate.ToShortDateString()}");
        }

        public void EndRelease()
        {
            _buffer.AppendLine();
        }

        public void StartFeatures()
        {
        }

        public void AddFeature(string feature)
        {
            _buffer.AppendLine("* " + feature);
        }

        public void EndFeatures()
        {
            _buffer.AppendLine();
        }
    }
    
    public class JsonServiceFeatureListWriter : BaseJsonWriter, IServiceFeatureListWriter
    {
        public void StartRelease(string version, DateTime releaseDate)
        {
            _jsonWriter.WriteStartObject();
            _jsonWriter.WriteString("version", version);
            _jsonWriter.WriteString("release-date", releaseDate.ToShortDateString());
        }

        public void EndRelease()
        {
            _jsonWriter.WriteEndObject();
        }

        public void StartFeatures()
        {
            _jsonWriter.WriteStartArray("features");
        }

        public void AddFeature(string feature)
        {
            _jsonWriter.WriteStringValue(feature);
        }

        public void EndFeatures()
        {
            _jsonWriter.WriteEndArray();
        }
    }    
}