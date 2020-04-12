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
        [InlineData("* Application Auto Scaling (3.1.0.0)", "unknown", "0.0.0.0")]
        [InlineData("* [GARelease](aaa) (3.1.0.0)", "unknown", "0.0.0.0")]
        public void ExtractServiceInformationTest(string line, string expectedName, string expectVersion)
        {
            var service = new AwsSdkChangeLogService(null, null, new ResponseWriterFactory());
            var info = service.ExtractServiceInformation(line);
            Assert.Equal(expectedName, info.name);
            Assert.Equal(expectVersion, info.version);
        }

        [Theory]
        [InlineData("### 3.3.717.0 (2020-04-07 18:10 UTC)", 2020, 4, 7)]
        [InlineData("### 3.3.717.0 2020-04-07 18:10 UTC", -1, -1, -1)]
        public void ExtractDateTest(string line, int year, int month, int day)
        {
            var service = new AwsSdkChangeLogService(null, null, new ResponseWriterFactory());
            var actualDate = service.ExtractDateFromLine(line);

            if (year == -1)
            {
                Assert.Equal(DateTime.MinValue, actualDate);                
            }
            else
            {
                Assert.Equal(year, actualDate.Year);
                Assert.Equal(month, actualDate.Month);
                Assert.Equal(day, actualDate.Day);
            }
        }
    }
}