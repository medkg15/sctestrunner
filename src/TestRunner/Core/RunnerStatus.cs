using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRunner.Core
{
    public class RunnerStatus
    {
        public bool IsActive { get; set; }
        public int TotalTests { get; set; }
        public int CompletedTests { get; set; }
    }
}
