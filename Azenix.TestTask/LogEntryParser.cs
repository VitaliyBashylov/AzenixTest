using System;
using System.Globalization;
using System.Net;

namespace Azenix.TestTask
{
    public class LogEntryParser
    {
        private readonly string _input;
        private readonly AccessLogEntry _result;
        private int _index;

        private LogEntryParser(string input)
        {
            _input = input;
            _result = new AccessLogEntry();
            _index = 0;
        }
        public static AccessLogEntry Parse(string input)
        {
            //TODO: handle dashes
            var parser = new LogEntryParser(input)
                .Next((v, r) => r.IpAddress = v, ParseIp)
                .Next((v, r) => { }, ParseWord)
                .Next((v, r) => r.UserId = v, ParseWord)
                .Next((v, r) => r.RequestDate = v, ParseDate)
                .Next((v, r) => r.Request = v, ParseRequestInfo)
                .Next((v, r) => r.ResponseCode = (HttpStatusCode) v, ParseInt)
                .Next((v, r) => r.ResponseLength = v, ParseInt)
                .Next((v, r) => r.Referer = v, ParseWord)
                .Next((v, r) => r.UserAgent = v, (_, index) => Between(input, index, '"', '"'));

            return parser._result;
        }

        private static (int value, int next) ParseInt(string input, int index)
        {
            var (value, next) = ParseWord(input, index);
            return (int.Parse(value), next);
        }

        private LogEntryParser Next<T>(Action<T, AccessLogEntry> setter, Func<string, int, (T value, int next)> parser)
        {
            var (value, next) = parser(_input, _index);
            setter(value, _result);
            _index = next + 2;
            return this;
        }

        public static (RequestInfo value, int next) ParseRequestInfo(string input, int index)
        {
            var (value, next) = Between(input, index, '"', '"');
            var values = value.Split(' ');

            return (new RequestInfo()
            {
                Verb = values[0],
                Resource = values[1],
                Protocol = values[2],
            }, next);
        }

        public static (DateTimeOffset value, int next) ParseDate(string input, int index)
        {
            var (value, next) = Between(input, index, '[', ']');

            return (DateTimeOffset.ParseExact(value, "dd/MMM/yyyy:HH:mm:ss zzz", CultureInfo.InvariantCulture),
                next);
        }

        private static (string value, int next) Between(string input, int index, char start, char end)
        {
            if (input[index] != start)
                throw new ApplicationException($"invalid character at {index} expected {start} got {input[index]}");
            
            var endI = input.IndexOf(end, index + 1);
            return (input.Substring(index + 1, endI - index - 1), endI);
        }

        private static (string value, int next) ParseWord(string input, int index)
        {
            var end = input.IndexOf(' ', index);
            return end < 0 
                ? (input.Substring(index, input.Length - index), input.Length - 1) 
                : (input.Substring(index, end - index), end - 1);
        }

        public static (IPAddress value, int next) ParseIp(string input, int index)
        {
            var (value, next) = ParseWord(input, index);

            return (IPAddress.Parse(value), next: next);
        }
    }
}