using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Parchive.Library.Tasks
{
    public class TaskGroup : IGrouping<byte[], Task>, IImmutableList<Task>
    {
        #region Private Classes
        private class TaskProgressReporter : IProgress<TaskProgressEventArgs>
        {
            #region Methods
            public void Report(TaskProgressEventArgs value)
            {
                if (ProgressUpdated != null)
                    ProgressUpdated(this, value);
            }
            #endregion

            #region Events
            public event EventHandler<TaskProgressEventArgs> ProgressUpdated;
            #endregion
        }
        #endregion

        #region Fields
        private byte[] _Key;
        private ImmutableList<Task> _Tasks = ImmutableList<Task>.Empty;
        private CancellationTokenSource _TokenSource = new CancellationTokenSource();
        private TaskProgressReporter _Progress = new TaskProgressReporter();
        #endregion

        #region Properties
        /// <summary>
        /// Object used to report progress for a task.
        /// </summary>
        public IProgress<TaskProgressEventArgs> Progress
        {
            get
            {
                return _Progress;
            }
        }

        /// <summary>
        /// The token for the Cancel() method.
        /// </summary>
        public CancellationToken Token
        {
            get
            {
                return _TokenSource.Token;
            }
        }

        /// <summary>
        /// The ID of the task group.
        /// </summary>
        public byte[] Key
        {
            get
            {
                return _Key;
            }
        }
        
        int IReadOnlyCollection<Task>.Count
        {
            get
            {
                return _Tasks.Count;
            }
        }

        Task IReadOnlyList<Task>.this[int index]
        {
            get
            {
                return _Tasks[index];
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor.
        /// </summary>
        public TaskGroup()
        {
            _Key = Guid.NewGuid().ToByteArray();
            _Progress.ProgressUpdated += (sender, e) => ProgressUpdated(this, e);
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="o">The source object for the copy.</param>
        protected TaskGroup(TaskGroup o)
        {
            _Key = o.Key;
            _Progress = o._Progress;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Starts all the tasks in the group.
        /// </summary>
        public void Start()
        {
            foreach (var task in this)
            {
                if (task.Status == TaskStatus.Created)
                    task.Start();
            }
        }

        /// <summary>
        /// Cancels all tasks which uses the cancellation token from the Token property.
        /// </summary>
        public void Cancel()
        {
            _TokenSource.Cancel();
        }

        public IEnumerator<Task> GetEnumerator()
        {
            return _Tasks.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IImmutableList<Task> IImmutableList<Task>.Clear()
        {
            return new TaskGroup(this)
            {
                _Tasks = _Tasks.Clear()
            };
        }

        TaskGroup Clear()
        {
            return (this as IImmutableList<Task>).Clear() as TaskGroup;
        }

        int IImmutableList<Task>.IndexOf(Task item, int index, int count, IEqualityComparer<Task> equalityComparer)
        {
            return _Tasks.IndexOf(item, index, count, equalityComparer);
        }

        int IImmutableList<Task>.LastIndexOf(Task item, int index, int count, IEqualityComparer<Task> equalityComparer)
        {
            return _Tasks.LastIndexOf(item, index, count, equalityComparer);
        }

        IImmutableList<Task> IImmutableList<Task>.Add(Task value)
        {
            return AttachContinuationTasks(new TaskGroup(this)
            {
                _Tasks = _Tasks.Add(value)
            });
        }

        public TaskGroup Add(Task value)
        {
            return (this as IImmutableList<Task>).Add(value) as TaskGroup;
        }

        IImmutableList<Task> IImmutableList<Task>.AddRange(IEnumerable<Task> items)
        {
            return AttachContinuationTasks(new TaskGroup(this)
            {
                _Tasks = _Tasks.AddRange(items)
            });
        }

        public TaskGroup AddRange(IEnumerable<Task> items)
        {
            return (this as IImmutableList<Task>).AddRange(items) as TaskGroup;
        }

        IImmutableList<Task> IImmutableList<Task>.Insert(int index, Task element)
        {
            return AttachContinuationTasks(new TaskGroup(this)
            {
                _Tasks = _Tasks.Insert(index, element)
            });
        }

        public TaskGroup Insert(int index, Task element)
        {
            return (this as IImmutableList<Task>).Insert(index, element) as TaskGroup;
        }

        IImmutableList<Task> IImmutableList<Task>.InsertRange(int index, IEnumerable<Task> items)
        {
            return AttachContinuationTasks(new TaskGroup(this)
            {
                _Tasks = _Tasks.InsertRange(index, items)
            });
        }

        public TaskGroup InsertRange(int index, IEnumerable<Task> items)
        {
            return (this as IImmutableList<Task>).InsertRange(index, items) as TaskGroup;
        }

        IImmutableList<Task> IImmutableList<Task>.Remove(Task value, IEqualityComparer<Task> equalityComparer)
        {
            return AttachContinuationTasks(new TaskGroup(this)
            {
                _Tasks = _Tasks.Remove(value, equalityComparer)
            });
        }

        public TaskGroup Remove<TCompareKey>(Task value, Func<Task, TCompareKey> compareKeySelector)
        {
            return (this as IImmutableList<Task>).Remove(value, compareKeySelector) as TaskGroup;
        }

        IImmutableList<Task> IImmutableList<Task>.RemoveAll(Predicate<Task> match)
        {
            return AttachContinuationTasks(new TaskGroup(this)
            {
                _Tasks = _Tasks.RemoveAll(match)
            });
        }

        public TaskGroup RemoveAll(Predicate<Task> match)
        {
            return (this as IImmutableList<Task>).RemoveAll(match) as TaskGroup;
        }

        IImmutableList<Task> IImmutableList<Task>.RemoveRange(IEnumerable<Task> items, IEqualityComparer<Task> equalityComparer)
        {
            return AttachContinuationTasks(new TaskGroup(this)
            {
                _Tasks = _Tasks.RemoveRange(items, equalityComparer)
            });
        }

        public TaskGroup RemoveRange<TCompareKey>(IEnumerable<Task> items, Func<Task, TCompareKey> compareKeySelector)
        {
            return (this as IImmutableList<Task>).RemoveRange(items, compareKeySelector) as TaskGroup;
        }

        IImmutableList<Task> IImmutableList<Task>.RemoveRange(int index, int count)
        {
            return AttachContinuationTasks(new TaskGroup(this)
            {
                _Tasks = _Tasks.RemoveRange(index, count)
            });
        }

        public TaskGroup RemoveRange(int index, int count)
        {
            return (this as IImmutableList<Task>).RemoveRange(index, count) as TaskGroup;
        }

        IImmutableList<Task> IImmutableList<Task>.RemoveAt(int index)
        {
            return AttachContinuationTasks(new TaskGroup(this)
            {
                _Tasks = _Tasks.RemoveAt(index)
            });
        }

        public TaskGroup RemoveAt(int index)
        {
            return (this as IImmutableList<Task>).RemoveAt(index) as TaskGroup;
        }

        IImmutableList<Task> IImmutableList<Task>.SetItem(int index, Task value)
        {
            return AttachContinuationTasks(new TaskGroup(this)
            {
                _Tasks = _Tasks.SetItem(index, value)
            });
        }

        public TaskGroup SetItem(int index, Task value)
        {
            return (this as IImmutableList<Task>).SetItem(index, value) as TaskGroup;
        }

        IImmutableList<Task> IImmutableList<Task>.Replace(Task oldValue, Task newValue, IEqualityComparer<Task> equalityComparer)
        {
            return AttachContinuationTasks(new TaskGroup(this)
            {
                _Tasks = _Tasks.Replace(oldValue, newValue, equalityComparer)
            });
        }

        public TaskGroup Replace<TCompareKey>(Task oldValue, Task newValue, Func<Task, TCompareKey> compareKeySelector)
        {
            return (this as IImmutableList<Task>).Replace(oldValue, newValue, compareKeySelector) as TaskGroup;
        }

        IEnumerator<Task> IEnumerable<Task>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Attaches continuation tasks for callbacks.
        /// </summary>
        /// <param name="o">The task group to which the tasks will be attached.</param>
        /// <returns>The task group.</returns>
        private TaskGroup AttachContinuationTasks(TaskGroup o)
        {
            o = AttachGroupFinishedContinuation(o);
            o = AttachTaskCompletedContinuation(o);

            return o;
        }

        /// <summary>
        /// Attaches the group finished callback.
        /// </summary>
        /// <param name="o">The task group to which the tasks will be attached.</param>
        /// <returns>The task group.</returns>
        private TaskGroup AttachGroupFinishedContinuation(TaskGroup o)
        {
            Task.WhenAll(o._Tasks).ContinueWith((t) =>
            {
                if (o.Finished != null)
                    o.Finished(o, new TaskGroupFinishedEventArgs { TaskGroupId = this._Key });
            });

            return o;
        }

        /// <summary>
        /// Attaches the task completed callback.
        /// </summary>
        /// <param name="o">The task group to which the tasks will be attached.</param>
        /// <returns>The task group.</returns>
        private TaskGroup AttachTaskCompletedContinuation(TaskGroup o)
        {
            foreach (var task in o._Tasks)
            {
                task.ContinueWith((t) =>
                {
                    if (o.TaskCompleted != null)
                        o.TaskCompleted(o, new TaskCompletedEventArgs { TaskId = t.Id });
                });
            }

            return o;
        }
        #endregion

        #region Events
        /// <summary>
        /// Triggers when a task reports progress.
        /// </summary>
        public event EventHandler<TaskProgressEventArgs> ProgressUpdated;

        /// <summary>
        /// Triggers when a task completes.
        /// </summary>
        public event EventHandler<TaskCompletedEventArgs> TaskCompleted;

        /// <summary>
        /// Triggers when the group has finished execution.
        /// </summary>
        public event EventHandler<TaskGroupFinishedEventArgs> Finished;
        #endregion
    }
}
