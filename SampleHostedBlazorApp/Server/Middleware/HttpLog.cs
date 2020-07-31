using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleHostedBlazorApp.Server.Middleware {
    public class HttpLog {
        public string Path { get; set; }
        public string QueryString { get; set; }
        public string DisplayUrl { get; set; }
        public string Method { get; set; }
        public string Payload { get; set; }
        public string Response { get; set; }
        public string ResponseCode { get; set; }
        public DateTime RequestedOn { get; set; }
        public DateTime RespondedOn { get; set; }
        public bool IsSuccessStatusCode { get; set; }
    }
}
