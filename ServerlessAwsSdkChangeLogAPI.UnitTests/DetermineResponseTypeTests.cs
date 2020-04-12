using System;
using Xunit;
using ServerlessAwsSdkChangeLogAPI.Features;
using ServerlessAwsSdkChangeLogAPI.Services;

namespace ServerlessAwsSdkChangeLogAPI.UnitTests
{
    public class DetermineResponseTypeTests
    {
        [Fact]
        public void NullAcceptHeader()
        {
            var responseInfo = AwsSdkChangeLogModule.DetermineResponseType(null);
            Assert.Equal("text/plain", responseInfo.ResponseContentType);
            Assert.Equal(ResponseWriterType.Text, responseInfo.WriterType);
        }
        
        [Fact]
        public void EmptyStringAcceptHeader()
        {
            var responseInfo = AwsSdkChangeLogModule.DetermineResponseType(string.Empty);
            Assert.Equal("text/plain", responseInfo.ResponseContentType);
            Assert.Equal(ResponseWriterType.Text, responseInfo.WriterType);
        }

        [Fact]
        public void TextPlainAcceptHeader()
        {
            var responseInfo = AwsSdkChangeLogModule.DetermineResponseType("text/plain");
            Assert.Equal("text/plain", responseInfo.ResponseContentType);
            Assert.Equal(ResponseWriterType.Text, responseInfo.WriterType);
        }
        
        [Fact]
        public void ApplicationJsonAcceptHeader()
        {
            var responseInfo = AwsSdkChangeLogModule.DetermineResponseType("application/json");
            Assert.Equal("application/json", responseInfo.ResponseContentType);
            Assert.Equal(ResponseWriterType.Json, responseInfo.WriterType);
        }

        [Fact]
        public void ApplicationJsonSecondAcceptHeader()
        {
            var responseInfo = AwsSdkChangeLogModule.DetermineResponseType("text/html, application/json");
            Assert.Equal("application/json", responseInfo.ResponseContentType);
            Assert.Equal(ResponseWriterType.Json, responseInfo.WriterType);
        }        
    }
}