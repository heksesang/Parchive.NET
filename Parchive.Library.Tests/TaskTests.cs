using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parchive.Library.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Parchive.Library.Tests
{
    [TestClass]
    public class TaskTests
    {
        [TestMethod]
        public void ConcurrentTaskGroupStart()
        {
            TaskGroup group = new TaskGroup();

            group = group.Add(new Task(() => Debug.WriteLine(nameof(ConcurrentTaskGroupStart))));
            group = group.Add(new Task(() => Debug.WriteLine(nameof(ConcurrentTaskGroupStart))));

            var t1 = new Task(() => group.Start());
            var t2 = new Task(() => group.Start());

            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Assert.AreEqual(TaskStatus.RanToCompletion, t1.Status);
            Assert.AreEqual(TaskStatus.RanToCompletion, t2.Status);
        }

        [TestMethod]
        public void TaskGroupFinishedSignal()
        {
            TaskGroup group = new TaskGroup();

            group = group.Add(new Task(() => Debug.WriteLine(nameof(TaskGroupFinishedSignal))));
            group = group.Add(new Task(() => Debug.WriteLine(nameof(TaskGroupFinishedSignal))));

            ManualResetEvent resetEvent = new ManualResetEvent(false);

            group.Finished += (sender, e) => resetEvent.Set();
            group.Start();

            if (!resetEvent.WaitOne(1000))
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TaskCompletedSignal()
        {
            TaskGroup group = new TaskGroup();

            int i = 0;

            group = group.Add(new Task(() => Debug.WriteLine(nameof(TaskCompletedSignal))));
            group = group.Add(new Task(() => Debug.WriteLine(nameof(TaskCompletedSignal))));

            group.TaskCompleted += (sender, e) => Interlocked.Increment(ref i);

            group.Start();

            Thread.Sleep(1000);

            if (i != 2)
            {
                Assert.Fail();
            }
        }
    }
}
