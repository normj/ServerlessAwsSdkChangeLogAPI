using System;
using Xunit;
using ServerlessAwsSdkChangeLogAPI.Services;
namespace ServerlessAwsSdkChangeLogAPI.UnitTests
{
    public class LineParserTests
    {
        [Theory]
        [InlineData("* Core 3.3.106.4", "Core", "3.3.106.4")]
        [InlineData("* Core (3.3.106.4)", "Core", "3.3.106.4")]
        [InlineData("* IoT (3.3.7.0)", "IoT", "3.3.7.0")]
        [InlineData("* FailCase", "unknown", "0.0.0.0")]
        [InlineData("* FailCase (", "unknown", "0.0.0.0")]
        [InlineData("* FailCase )(", "unknown", "0.0.0.0")]
        [InlineData("* FailCase )", "unknown", "0.0.0.0")]
        [InlineData("* Application Auto Scaling (3.1.0.0)", "Application Auto Scaling", "3.1.0.0")]
        public void ExtractServiceInformationTest(string line, string expectedName, string expectVersion)
        {
            var service = new AwsSdkChangeLogService(null, null);
            var info = service.ExtractServiceInformation(line);
            Assert.Equal(expectedName, info.name);
            Assert.Equal(expectVersion, info.version);
        }
    }
}