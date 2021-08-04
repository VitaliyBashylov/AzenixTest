using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net;
using NUnit.Framework;

namespace Azenix.TestTask.Tests
{
    public class LogEntryParserTests
    {
        [Test]
        public void TestIpParser()
        {
            Assert.AreEqual(new IPAddress(new byte[]{ 1, 1, 1, 1}), LogEntryParser.ParseIp("1.1.1.1", 0).value);
            Assert.AreEqual(new IPAddress(new byte[]{ 255, 255, 255, 255}), LogEntryParser.ParseIp("255.255.255.255", 0).value);

            Assert.Throws<FormatException>(() => LogEntryParser.ParseIp("1.1.1.999", 0));
        }
        [Test]
        public void TestDateParser()
        {
            var date = LogEntryParser.ParseDate("[10/Jul/2018:22:21:28 +0200] 123", 0);
            Assert.AreEqual(new DateTimeOffset(2018, 7, 10, 22, 21, 28, TimeSpan.FromHours(2)), date.value);
        }
        [Test]
        public void TestRequestParser()
        {
            var request = LogEntryParser.ParseRequestInfo("\"POST /asd/1223 HTTP/1.1\"", 0);
            Assert.AreEqual("POST", request.value.Verb);
            Assert.AreEqual("/asd/1223", request.value.Resource);
            Assert.AreEqual("HTTP/1.1", request.value.Protocol);
        }

        [Test]
        public void TestCompleteParser()
        {
            var line =
                @"50.112.00.11 - admin [11/Jul/2018:17:31:56 +0200] ""GET /asset.js HTTP/1.1"" 200 3574 ""-"" ""Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/536.6 (KHTML, like Gecko) Chrome/20.0.1092.0 Safari/536.6""";

            var r = LogEntryParser.Parse(line);
            Assert.AreEqual(IPAddress.Parse("50.112.00.11"), r.IpAddress);
            Assert.AreEqual("admin", r.UserId);
            Assert.AreEqual(new DateTimeOffset(2018, 7, 11, 17, 31, 56, TimeSpan.FromHours(2)), r.RequestDate);
            Assert.AreEqual("GET", r.Request.Verb);
            Assert.AreEqual("/asset.js", r.Request.Resource);
            Assert.AreEqual("HTTP/1.1", r.Request.Protocol);
            Assert.AreEqual(HttpStatusCode.OK, r.ResponseCode);
            Assert.AreEqual(3574, r.ResponseLength);
            Assert.AreEqual("Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/536.6 (KHTML, like Gecko) Chrome/20.0.1092.0 Safari/536.6", r.UserAgent);
        }

    }
}