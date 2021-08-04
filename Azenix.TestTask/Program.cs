using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using MoreLinq.Extensions;

namespace Azenix.TestTask
{
    public class RequestInfo
    {
        public string Verb { get; set; }
        public string Resource { get; set; }
        public string Protocol { get; set; }
    }
    public class AccessLogEntry
    {
        public IPAddress IpAddress { get; set; }
        public string UserId { get; set; }
        public DateTimeOffset RequestDate { get; set; }
        public RequestInfo Request { get; set; }
        public HttpStatusCode ResponseCode { get; set; }
        public long ResponseLength { get; set; }
        public string Referer { get; set; }
        public string UserAgent { get; set; }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            //TODO: validate
            var file = args[0];

            var data = (await File.ReadAllLinesAsync(file)).Select(LogEntryParser.Parse).ToList();

            //TODO: unit test that
            var numberOfUniqueIps = data.GroupBy(i => i.IpAddress).Count();
            var mostVisitUrls = data.Select(i => i.Request.Resource)
                .GroupBy(i => i)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .ToList();
            var mostActiveIps = data.Select(i => i.IpAddress)
                .GroupBy(i => i)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .ToList();
            
            Console.WriteLine($"number of unique ids: {numberOfUniqueIps}");
            Console.WriteLine($"Most visited uris:");
            mostVisitUrls.Select(g => g.Key).ForEach(k => Console.WriteLine(k));
            Console.WriteLine($"Most active ips:");
            mostActiveIps.Select(g => g.Key).ForEach(Console.WriteLine);
        }
    }
}