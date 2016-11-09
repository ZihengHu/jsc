using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace JscServer.Models
{
    public class Coverage
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string Data { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime InsertTime { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdateTime { get; set; }
    }

    public class CoverageData
    {
        public IEnumerable<int?> LineData { get; set; }
        public IEnumerable<int?> FunctionData { get; set; }
        public IDictionary<string, IEnumerable<BranchData>> BranchData { get; set; }
    }

    public class BranchData
    {
        public int Position { get; set; }
        public int NodeLength { get; set; }
        public string Src { get; set; }
        public int EvalFalse { get; set; }
        public int EvalTrue { get; set; }
    }
}
