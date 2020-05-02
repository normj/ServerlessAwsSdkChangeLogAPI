using System;
using ServerlessAwsSdkChangeLogAPI.Web.Writers;
using ServerlessAwsSdkChangeLogAPI.Web.Controllers;

using Xunit;

namespace ServerlessAwsSdkChangeLogAPI.UnitTests
{
    public class DetermineResponseTypeTests
    {
        [Fact]
        public void NullAcceptHeader()
        {
            var responseInfo = AwsSdkChangeLogController.DetermineResponseType(null);
            Assert.Equal("text/plain", responseInfo.ResponseContentType);
            Assert.Equal(ResponseWriterType.Text, responseInfo.WriterType);
        }
        
        [Fact]
        public void EmptyStringAcceptHeader()
        {
            var responseInfo = AwsSdkChangeLogController.DetermineResponseType(string.Empty);
            Assert.Equal("text/plain", responseInfo.ResponseContentType);
            Assert.Equal(ResponseWriterType.Text, responseInfo.WriterType);
        }

        [Fact]
        public void TextPlainAcceptHeader()
        {
            var responseInfo = AwsSdkChangeLogController.DetermineResponseType("text/plain");
            Assert.Equal("text/plain", responseInfo.ResponseContentType);
            Assert.Equal(ResponseWriterType.Text, responseInfo.WriterType);
        }
        
        [Fact]
        public void ApplicationJsonAcceptHeader()
        {
            var responseInfo = AwsSdkChangeLogController.DetermineResponseType("application/json");
            Assert.Equal("application/json", responseInfo.ResponseContentType);
            Assert.Equal(ResponseWriterType.Json, responseInfo.WriterType);
        }

        [Fact]
        public void ApplicationJsonSecondAcceptHeader()
        {
            var responseInfo = AwsSdkChangeLogController.DetermineResponseType("text/html, application/json");
            Assert.Equal("application/json", responseInfo.ResponseContentType);
            Assert.Equal(ResponseWriterType.Json, responseInfo.WriterType);
        }        
    }
}