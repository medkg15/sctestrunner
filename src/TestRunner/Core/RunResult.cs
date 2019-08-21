using TestRunner.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRunner.Core
{
    public class RunResult
    {
        public int Total { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public int Errors { get; set; }
        public int Skipped { get; set; }
        public int Inconclusive { get; set; }
        public int Invalid { get; set; }
        public int Ignored { get; set; }
        public decimal ExecutionTime { get; set; }

        public IEnumerable<FixtureResult> Fixtures { get; set; }

        public IEnumerable<FixtureResult> ErrorList { get; set; }
        
        public IEnumerable<FixtureResult> IgnoredList { get; set; }
        
        public string TextOutput { get; set; }
    }
}
