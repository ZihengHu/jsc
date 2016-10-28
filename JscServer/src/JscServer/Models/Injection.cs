using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JscServer.Models
{
    public class Injection
    {
        public string Id { get; set; }
        public string OriginalUrl { get; set; }
        public string InjectedPath { get; set; }
    }
}
