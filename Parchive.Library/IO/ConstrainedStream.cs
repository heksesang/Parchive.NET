using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Parchive.Library.IO
{
    public class ConstrainedStream : Stream
    {
        private Stream wrappedStream;
        private bool leaveOpen;
        private Range<long> constraints;
        
        public ConstrainedStream(Stream wrappedStream, long offset, long length) : this(wrappedStream, offset, length, false)
        {
        }
        
        public ConstrainedStream(Stream wrappedStream, long offset, long length, bool leaveOpen)
        {
            this.wrappedStream = wrappedStream;
            this.leaveOpen = leaveOpen;
            constraints = new Range<long>
            {
                Minimum = offset,
                Maximum = offset + length
            };
            this.wrappedStream.Seek(offset, SeekOrigin.Begin);
        }

        public override bool CanRead
        {
            get
            {
                return wrappedStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return wrappedStream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return wrappedStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return constraints.Maximum - constraints.Minimum;
            }
        }

        public override long Position
        {
            get
            {
                return constrainPosition(wrappedStream.Position);
            }

            set
            {
                wrappedStream.Position = constrainPosition(value);
            }
        }

        public override bool CanTimeout
        {
            get
            {
                return wrappedStream.CanTimeout;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                return wrappedStream.ReadTimeout;
            }

            set
            {
                wrappedStream.ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                return wrappedStream.WriteTimeout;
            }

            set
            {
                wrappedStream.WriteTimeout = value;
            }
        }

        public override void Close()
        {
            if (!leaveOpen)
                wrappedStream.Close();
        }

        public override void Flush()
        {
            wrappedStream.Flush();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (!CanRead)
                throw new NotSupportedException();

            count = constrainCount(count);

            return BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            if (!CanRead)
                throw new NotSupportedException();

            return EndRead(asyncResult);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (!CanRead)
                throw new NotSupportedException();

            count = constrainCount(count);

            return ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!CanRead)
                throw new NotSupportedException();

            count = constrainCount(count);

            return wrappedStream.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            if (Position == constraints.Maximum)
                return -1;

            return base.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!CanSeek)
                throw new NotSupportedException();

            return constrainPosition(wrappedStream.Seek(constraintOffset(offset, origin), origin));
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (!CanWrite)
                throw new NotSupportedException();

            return BeginWrite(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            if (!CanWrite)
                throw new NotSupportedException();

            EndWrite(asyncResult);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (!CanWrite)
                throw new NotSupportedException();

            count = constrainCount(count);

            return wrappedStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!CanWrite)
                throw new NotSupportedException();

            count = constrainCount(count);

            wrappedStream.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            if (Position < constraints.Maximum)
            {
                base.WriteByte(value);
            }
        }

        private long constraintOffset(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    return constrainOffsetWithBeginOrigin(offset);

                case SeekOrigin.End:
                    return constrainOffsetWithEndOrigin(offset);

                case SeekOrigin.Current:
                    return constrainOffsetWithCurrentOrigin(offset);
            }

            return offset;
        }

        private long constrainOffsetWithBeginOrigin(long offset)
        {
            if (!constraints.ContainsValue(constraints.Minimum + offset))
            {
                if (constraints.Minimum + offset < constraints.Minimum)
                {
                    return constraints.Minimum;
                }
                else
                {
                    return constraints.Maximum;
                }
            }
            return constraints.Minimum + offset;
        }

        private long constrainOffsetWithEndOrigin(long offset)
        {
            if (!constraints.ContainsValue(constraints.Maximum - offset))
            {
                if (constraints.Maximum - offset < constraints.Minimum)
                {
                    return constraints.Minimum;
                }
                else
                {
                    return constraints.Maximum;
                }
            }
            return constraints.Maximum - offset;
        }

        private long constrainOffsetWithCurrentOrigin(long offset)
        {
            if (!constraints.ContainsValue(Position + offset))
            {
                if (Position + offset < constraints.Minimum)
                {
                    return constraints.Minimum - Position;
                }
                else
                {
                    return constraints.Maximum - Position;
                }
            }
            return Position + offset;
        }

        private long constrainPosition(long position)
        {
            if (!constraints.ContainsValue(position))
            {
                return position;
            }

            return position - constraints.Minimum;
        }

        private int constrainCount(int count)
        {
            if (count >= 0 && !constraints.ContainsValue(wrappedStream.Position + count))
            {
                count = (int)(constraints.Maximum - wrappedStream.Position);
            }

            return count;
        }
    }
}
