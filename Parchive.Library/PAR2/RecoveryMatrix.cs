using Parchive.Library.IO;
using Parchive.Library.PAR2.Packets;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.PAR2
{
    /// <summary>
    /// 
    /// </summary>
    public class RecoveryMatrix
    {
        /// <summary>
        /// Constructs a <see cref="RecoveryMatrix"/> object for a collection of source files, and generates a number of recovery slices.
        /// Each recovery slice can recover one damaged slice in a source file.
        /// </summary>
        /// <param name="destination">The destination of the recovery files.</param>
        /// <param name="filename">The base filename of the recovery files.</param>
        /// <param name="sources">The collection of <see cref="SourceFile"/> objects.</param>
        /// <param name="sliceSize">The size of each slice in the source files. This is also the size of each recovery slice.</param>
        /// <param name="numRecoverySlices">The number of recovery slices to generate.</param>
        public RecoveryMatrix(Uri destination, string filename, IEnumerable<SourceFile> sources, long sliceSize, ushort numRecoverySlices)
        {
            this.sliceSize = sliceSize;

            // Initialize buffer.
            var bufferSize = CalculateBufferSize(sliceSize);
            var buffer = new byte[bufferSize].SetAllValues<byte>(0);

            // Initialize recovery slices.
            location = destination;
            this.filename = filename;
            InitializeRecoverySlices(numRecoverySlices);

            // Set initial values for read count and constant.
            long readCount = 0;
            var n = 1;
            
            foreach (var source in sources)
            {
                using (var reader = new BinaryReader(source.GetContent()))
                {
                    // Process until end of file.
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        // Reset the buffer on every read.
                        buffer.SetAllValues<byte>(0);

                        // If a full slice has been read.
                        if (readCount == sliceSize)
                        {
                            // Reset read count.
                            readCount = 0;

                            // Increment constant index.
                            n++;
                        }

                        // Note current offset into slice. Will be needed later.
                        long pos = readCount;

                        // Read data into the buffer.
                        readCount += reader.Read(buffer, 0, bufferSize);

                        // Add the new data to the accumulated data.
                        for (int i = 0, e = 1; i < numRecoverySlices; i++, e++)
                        {
                            var values = ReadRecoverySlice(i, pos, bufferSize / 2).Zip(buffer, (x, y) => Tuple.Create(x, (int)y));
                            WriteRecoverySlice(i, pos / 2, RecoveryMath.Recovery.acc(values, n, e));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the size to use for the buffers.
        /// </summary>
        /// <param name="sliceSize">The slice size.</param>
        /// <returns>The size of buffers.</returns>
        private int CalculateBufferSize(long sliceSize)
        {
            return (int)sliceSize;
        }

        private long sliceSize;
        private string filename;
        private Uri location;
        private IImmutableList<Tuple<int, RecoveryFile>> slices;

        private const string FilenameFormat = "{0}.vol{1:000}+{2:00}.par2";

        /// <summary>
        /// Initializes the recovery files.
        /// </summary>
        /// <param name="count">The number of slices to initialize.</param>
        private void InitializeRecoverySlices(int count)
        {
            var files = ImmutableList.Create<RecoveryFile>();

            if (count > 0)
            {
                int v = 0;
                int n = 1;

                files = files.Add(new RecoveryFile()
                {
                    Location = new Uri(location, string.Format(string.Format(FilenameFormat, filename, v, n))),
                    Exponents = new Range<long> { Minimum = 1, Maximum = 1 }
                });

                while ((v + n) < count)
                {
                    v += n;
                    n *= 2;

                    if ((v + n) < count)
                    {
                        files = files.Add(new RecoveryFile()
                        {
                            Location = new Uri(location, string.Format(string.Format(FilenameFormat, filename, v, n))),
                            Exponents = new Range<long> { Minimum = v, Maximum = v + n }
                        });
                    }
                }

                if (v != count)
                {
                    files = files.Add(new RecoveryFile()
                    {
                        Location = new Uri(location, string.Format(string.Format(FilenameFormat, filename, v, count - v))),
                        Exponents = new Range<long> { Minimum = v, Maximum = count }
                    });
                }
            }

            int sliceCount = 0;
            foreach (var file in files)
            {
                using (var writer = new ParWriter(file.GetContentStreamAsync().Result))
                {
                    for (var e = file.Exponents.Minimum; e <= file.Exponents.Maximum; e++)
                    {
                        var packet = new RecoverySlicePacket
                        {
                            Exponent = (uint)e
                        };

                        writer.Write(packet);
                    }
                }
            }
        }

        /// <summary>
        /// Reads the accumulated data of a recovery slice.
        /// </summary>
        /// <param name="index">The recovery slice index.</param>
        /// <param name="offset">The offset to start reading at.</param>
        /// <param name="count">The number of values to read.</param>
        /// <returns>A collection of <see cref="int"/> values.</returns>
        private IEnumerable<int> ReadRecoverySlice(int index, long offset, int count)
        {
            var slice = slices[index];

            using (var s = new BinaryReader(slice.Item2.GetContentStreamAsync().Result))
            {
                s.BaseStream.Seek(slice.Item1 + offset * 2, SeekOrigin.Begin);
                for (var i = 0; i < count; i++)
                {
                    yield return s.ReadUInt16();
                }
            }
        }

        /// <summary>
        /// Writes new accumulated data of a recovery file.
        /// </summary>
        /// <param name="index">The recovery slice index.</param>
        /// <param name="offset">The offset where the values should be written.</param>
        /// <param name="values">A collection of <see cref="int"/> values. These will be written as 16-bit integers.</param>
        private void WriteRecoverySlice(int index, long offset, IEnumerable<int> values)
        {
            var slice = slices[index];

            //using (var s = new BinaryReader(slice.Item2.GetContentStreamAsync().Result))
            //{
            //    s.BaseStream.Seek(slice.Item1 + offset * 2, SeekOrigin.Begin);
            //    for (var i = 0; i < values.Count(); i++)
            //    {
            //        yield return s.ReadUInt16();
            //    }
            //}
        }
    }
}
