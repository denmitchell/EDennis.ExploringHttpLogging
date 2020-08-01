using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleHostedBlazorApp.Server.Middleware {
    public class HttpLoggingOptions {
        public bool Enabled { get; set; }
        public int MaxRequestBodyLength { get; set; } = 10000;
        public int MaxResponseBodyLength { get; set; } = 10000;
        public bool IncludeHeaders { get; set; }
        public bool IncludeClaims { get; set; }
        public Dictionary<string, string[]> ForClaims { get; set; }
        public Dictionary<string, string[]> ForQueries { get; set; }
    }
}
