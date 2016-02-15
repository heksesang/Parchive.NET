using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Parchive.Library.Tasks
{
    public class TaskProgressEventArgs
    {
        public int TaskId { get; set; }
        public float Progress { get; set; }
    }
}