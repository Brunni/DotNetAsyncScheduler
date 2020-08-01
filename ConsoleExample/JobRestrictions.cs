using System;
using System.Collections.Generic;
using AsyncScheduler;
using AsyncScheduler.Restrictions;

namespace ConsoleExample
{
    public class JobRestriction : IJobStartRestriction
    {
        public bool RestrictStart(string jobToStart, IEnumerable<string> runningJobs)
        {
            return jobToStart == nameof(ExampleTask2);
        }
    }
}