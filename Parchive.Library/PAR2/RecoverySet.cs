using Parchive.Library.Exceptions;
using Parchive.Library.IO;
using Parchive.Library.PAR2.Packets;
using Parchive.Library.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Parchive.Library.PAR2
{
    public class VerificationCompletedEventArgs
    {
        public byte[] RecoverySetID { get; set; }
    }

    /// <summary>
    /// A PAR2 archive.
    /// </summary>
    public class RecoverySet : IDisposable
    {
        #region Enums
        public enum FileStatus
        {
            OK,
            Damaged,
            Missing,
            Unknown
        }
        public enum SliceStatus
        {
            OK,
            Damaged,
            Missing
        }
        #endregion

        #region Fields
        private MainPacket _Main;
        
        private ImmutableDictionary<FileID, FileDescriptionPacket> _FileDescriptions
            = ImmutableDictionary<FileID, FileDescriptionPacket>.Empty;
        
        private ImmutableDictionary<FileID, InputFileSliceChecksumPacket> _InputFileSliceChecksums
            = ImmutableDictionary<FileID, InputFileSliceChecksumPacket>.Empty;
        
        private ImmutableList<RecoveryFile> _ParFiles
            = ImmutableList<RecoveryFile>.Empty;
        
        private ImmutableDictionary<FileID, Stream> _FileStreams
            = ImmutableDictionary<FileID, Stream>.Empty;
        
        private ImmutableDictionary<FileID, FileStatus> _FileStatus
            = ImmutableDictionary<FileID, FileStatus>.Empty;
        
        private long _AvailableRecoverySlices = 0;
        private long _RequiredRecoverySlices = 0;
        #endregion

        #region Properties
        /// <summary>
        /// The base directory of the archive.
        /// </summary>
        protected string WorkingDirectory
        {
            get
            {
                return _ParFiles.Select(x =>
                    new FileInfo(x.Location.AbsolutePath).DirectoryName).FirstOrDefault();
            }
        }

        /// <summary>
        /// The status of the source files.
        /// </summary>
        public IReadOnlyDictionary<FileID, FileStatus> Status
        {
            get
            {
                return _FileStatus;
            }
        }

        /// <summary>
        /// Description/metadata of the source files.
        /// </summary>
        public IReadOnlyList<SourceFile> Files
        {
            get
            {
                return _FileDescriptions.Select(x => new SourceFile(x.Value)).ToList();
            }
        }

        /// <summary>
        /// The number of available recovery slices in this set.
        /// </summary>
        public long AvailableRecoverySlices
        {
            get
            {
                return _AvailableRecoverySlices;
            }
        }

        /// <summary>
        /// The number of required slices to repair the source files in this set.
        /// </summary>
        public long RequiredRecoverySlices
        {
            get
            {
                return _RequiredRecoverySlices;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Triggers when the verification of source files is complete.
        /// </summary>
        public event EventHandler VerificationCompleted;
        #endregion

        #region Constructors
        protected RecoverySet() { }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a source file to the archive.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="input"></param>
        public void AddFile(string filename, Stream input)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes a source file from the archive.
        /// </summary>
        /// <param name="filename">The filename of the file.</param>
        public void RemoveFile(string filename)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initialize a new archive.
        /// </summary>
        /// <param name="sliceSize">The slice size.</param>
        /// <param name="creator">The application which created the archive.</param>
        /// <exception cref="Parchive.Library.Exceptions.InvalidSliceSizeError">
        /// The slice size is less than zero or isn't a multiple of 4.
        /// </exception>
        public static RecoverySet Initialize(long sliceSize, string creator = "")
        {
            return new RecoverySet
            {
                _Main = new MainPacket
                {
                    SliceSize = sliceSize
                }
            };
        }

        /// <summary>
        /// Open an existing archive.
        /// </summary>
        /// <param name="parFiles">The recovery files.</param>
        /// <exception cref="Parchive.Library.Exceptions.TooLargeNumberError">
        /// One or more packets in the recovery files use too large numbers.
        /// </exception>
        public static RecoverySet Open(IEnumerable<RecoveryFile> parFiles)
        {
            var rs = new RecoverySet();

            if (parFiles == null || parFiles.Count() == 0)
            {
                throw new Exception("Recovery files are missing.");
            }

            if (parFiles.Select(x => x.Name).Distinct().Count() != 1)
            {
                throw new Exception("Recovery files are not from the same set.");
            }

            try
            {
                foreach (var parFile in parFiles)
                {
                    rs.LoadParFile(parFile);
                }

                foreach (var fd in rs._FileDescriptions)
                {
                    rs.LoadFile(fd.Value.FileID);
                }
            }
            catch (Exception)
            {
                rs.Close();
                throw;
            }

            return rs;
        }

        /// <summary>
        /// Close the archive.
        /// </summary>
        public void Close()
        {
            _Main = null;
            _FileDescriptions = _FileDescriptions.Clear();
            _InputFileSliceChecksums = _InputFileSliceChecksums.Clear();
            _ParFiles = _ParFiles.Clear();

            _AvailableRecoverySlices = 0;
            _RequiredRecoverySlices = 0;

            foreach (var f in _FileStreams)
            {
                f.Value.Close();
            }
            _FileStreams = _FileStreams.Clear();
        }

        /// <summary>
        /// Load a recovery file.
        /// </summary>
        /// <param name="parFile">The recovery file.</param>
        /// <exception cref="Parchive.Library.Exceptions.TooLargeNumberError">
        /// One or more packets in the file uses too large numbers.
        /// </exception>
        protected void LoadParFile(RecoveryFile parFile)
        {
            var f = File.Open(parFile.Location.AbsolutePath, FileMode.Open, FileAccess.Read);

            Packet packet = null;

            do
            {
                if (packet != null)
                {
                    if (packet is MainPacket)
                    {
                        if (_Main == null)
                            _Main = (MainPacket)packet;
                    }
                    else if (packet is FileDescriptionPacket)
                    {
                        var fd = (FileDescriptionPacket)packet;
                        if (!_FileDescriptions.ContainsKey(fd.FileID))
                            _FileDescriptions = _FileDescriptions.Add(fd.FileID, fd);
                    }
                    else if (packet is InputFileSliceChecksumPacket)
                    {
                        var ifsc = (InputFileSliceChecksumPacket)packet;
                        if (!_InputFileSliceChecksums.ContainsKey(ifsc.FileID))
                            _InputFileSliceChecksums = _InputFileSliceChecksums.Add(ifsc.FileID, ifsc);
                    }
                    else if (packet is RecoverySlicePacket)
                    {
                        _AvailableRecoverySlices++;
                    }
                }

                try
                {
                    packet = Packet.FromStream(f);
                }
                catch (UnsupportedPacketError)
                {

                }
                catch (InvalidPacketError)
                {

                }
            }
            while (f.Position < f.Length);

            _ParFiles = _ParFiles.Add(parFile);
        }

        /// <summary>
        /// Load a source file.
        /// </summary>
        /// <param name="file">The FileID of the source file.</param>
        protected void LoadFile(FileID file)
        {
            var fileFound = false;
            var fd = _FileDescriptions[file];
            var cwd = new DirectoryInfo(WorkingDirectory);

            foreach (var filename in Directory.EnumerateFiles(cwd.FullName))
            {
                var fi = new FileInfo(filename);
                if (fd.Filename.TrimEnd('\0') == fi.GetUnixPath(cwd) && !_FileStreams.ContainsKey(fd.FileID))
                {
                    try
                    {
                        _FileStreams = _FileStreams.Add(fd.FileID, File.Open(filename, FileMode.Open, FileAccess.Read));
                        fileFound = true;
                    }
                    catch (IOException)
                    {
                        Debug.WriteLine("Unable to read file '" + filename + "'");
                    }
                }
            }

            if (!fileFound)
            {
                var buffer = new byte[16384];

                foreach (var filename in Directory.EnumerateFiles(cwd.FullName))
                {
                    buffer.SetAllValues<byte>(0);

                    var fi = new FileInfo(filename);

                    try
                    {
                        var f = File.Open(filename, FileMode.Open, FileAccess.Read);

                        try
                        {
                            f.Read(buffer, 0, buffer.Length);

                            using (var hash16k = MD5.Create())
                            {
                                hash16k.TransformFinalBlock(buffer, 0, buffer.Length);

                                if (!_FileStreams.ContainsKey(fd.FileID))
                                {
                                    _FileStreams = _FileStreams.Add(fd.FileID, f);
                                    fileFound = true;
                                }
                                else
                                {
                                    f.Close();
                                    f = null;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            if (f != null)
                            {
                                f.Close();
                                f = null;
                            }
                        }
                    }
                    catch (IOException)
                    {
                        Debug.WriteLine("Unable to open file '" + filename + "'");
                    }
                }
            }

            if (fileFound)
            {
                _FileStatus = _FileStatus.Add(fd.FileID, FileStatus.Unknown);
            }
            else
            {
                _FileStatus = _FileStatus.Add(fd.FileID, FileStatus.Missing);
            }
        }

        /// <summary>
        /// Reconstruct files.
        /// </summary>
        /// <param name="inputs"></param>
        protected void Reconstruct()
        {
            if (_Main.SliceSize > int.MaxValue)
            {
                throw new TooLargeNumberError("Too large slice size.");
            }
            
            var valid = new List<InputFileSliceChecksum>();
            
            var buffer = new byte[_Main.SliceSize];

            foreach (var input in _FileStreams)
            {
                // Get the slice checksums for this file.
                var slices = _InputFileSliceChecksums.FirstOrDefault(x => x.Key == input.Key);

                foreach (var checksum in slices.Value.Checksums)
                {
                    using (var actualChecksum = MD5.Create())
                    {
                        // Reset the buffer.
                        buffer.SetAllValues<byte>(0);

                        // Read the bytes into the buffer.
                        input.Value.Read(buffer, 0, buffer.Length);

                        // Hash the slice.
                        actualChecksum.TransformFinalBlock(buffer, 0, buffer.Length);

                        // Check if the hash is correct.
                        if (checksum.MD5.SequenceEqual(actualChecksum.Hash))
                        {
                            // Add to collection if correct.
                            valid.Add(checksum);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Runs a verification of the source files.
        /// </summary>
        public void Verify()
        {
            var taskGroup = new TaskGroup();
            var progress = taskGroup.Progress;

            foreach (var fileId in _FileDescriptions.Keys)
            {
                var inputSlices = _InputFileSliceChecksums[fileId];
                var stream = _FileStreams[fileId];

                var task = new Task<List<SliceStatus>>(() =>
                {
                    var buffer = new byte[_Main.SliceSize];

                    stream.Seek(0, SeekOrigin.Begin);

                    var sliceStatus = new List<SliceStatus>();
                    foreach (var checksum in inputSlices.Checksums)
                    {
                        if (stream.Position < stream.Length)
                        {
                            buffer.SetAllValues<byte>(0);
                            stream.Read(buffer, 0, buffer.Length);

                            using (var md5 = MD5.Create())
                            {
                                md5.TransformFinalBlock(buffer, 0, buffer.Length);

                                var status = checksum.MD5.SequenceEqual(md5.Hash) ?
                                    SliceStatus.OK : SliceStatus.Damaged;
                                sliceStatus.Add(status);

                                if (status != SliceStatus.OK)
                                    Interlocked.Increment(ref _RequiredRecoverySlices);
                            }

                            progress.Report(new TaskProgressEventArgs
                            {
                                Progress = (float)stream.Position / stream.Length,
                                TaskId = Task.CurrentId.Value
                            });
                        }
                        else
                        {
                            sliceStatus.Add(SliceStatus.Missing);
                            Interlocked.Increment(ref _RequiredRecoverySlices);
                        }
                    }

                    return sliceStatus;
                });

                taskGroup = taskGroup.Add(task);

                taskGroup.TaskCompleted += (sender,  e) =>
                {
                    if (e.TaskId == task.Id)
                        _FileStatus = _FileStatus.SetItem(fileId, task.Result.All(x => x == SliceStatus.OK) ?
                            FileStatus.OK : FileStatus.Damaged);
                };
            }

            taskGroup.Finished += (sender, e) =>
            {
                VerificationCompleted(this, new EventArgs());
            };

            taskGroup.Start();
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Close();
                }

                disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
