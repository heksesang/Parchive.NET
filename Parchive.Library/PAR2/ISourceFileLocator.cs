using Parchive.Library.PAR2.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.PAR2
{
    public interface ISourceFileLocator
    {
        /// <summary>
        /// Locates source files belonging to the recovery files.
        /// </summary>
        /// <param name="parFiles">The recovery files to find source files for.</param>
        /// <param name="additionalParams">Might be used by derived classes where additional parameters are required to locate the files.</param>
        /// <returns>A collection of SourceFile objects.</returns>
        IEnumerable<SourceFile> Locate(IEnumerable<RecoveryFile> parFiles, params object[] additionalParams);
    }

    public class LocalSourceFileLocator : ISourceFileLocator
    {
        /// <summary>
        /// Creates SourceFile objects for all the source files from the recovery files.
        /// </summary>
        /// <param name="parFiles">The recovery files to create SourceFile objects for.</param>
        /// <param name="additionalParams">Unused.</param>
        /// <returns>A collection of SourceFile objects.</returns>
        public IEnumerable<SourceFile> Locate(IEnumerable<RecoveryFile> parFiles, params object[] additionalParams)
        {
            var yieldedFiles = new List<FileID>();

            foreach (var parFile in parFiles)
            {
                using (var reader = new RecoveryFileReader(File.Open(parFile.Filename, FileMode.Open, FileAccess.Read)))
                {
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        var fd = reader.GetNextPacket(Packet.GetPacketType<FileDescriptionPacket>()) as FileDescriptionPacket;

                        if (fd == null)
                            break;

                        if (yieldedFiles.Contains(fd.FileID))
                            continue;

                        yieldedFiles.Add(fd.FileID);

                        var src = new SourceFile(fd);
                        var directory = Path.GetDirectoryName(parFiles.First().Filename);
                        var path = new FileInfo(Path.Combine(directory, src.Location)).FullName;
                        
                        src.Location = File.Exists(path) ? path : FindFilename(fd.Hash16k, directory);

                        yield return src;
                    }
                }
            }
        }
        
        /// <summary>
        /// Find the filename based on the hash of the first 16k bytes of the file.
        /// </summary>
        /// <param name="targetHash">The 16k hash to compare the files with.</param>
        /// <param name="directory">The directory to start the search in.</param>
        /// <returns>The filename if the file can be found; otherwise string.Empty.</returns>
        private string FindFilename(byte[] targetHash, string directory)
        {
            var buffer = new byte[16384];
            
            foreach (var filename in Directory.EnumerateFiles(directory))
            {
                buffer.SetAllValues<byte>(0);

                var fi = new FileInfo(filename);

                try
                {
                    using (var f = File.Open(filename, FileMode.Open, FileAccess.Read))
                    {
                        f.Read(buffer, 0, buffer.Length);

                        using (var hash16k = MD5.Create())
                        {
                            hash16k.TransformFinalBlock(buffer, 0, buffer.Length);

                            if (hash16k.Hash.SequenceEqual(targetHash))
                            {
                                return filename;
                            }
                        }
                    }
                }
                catch (IOException)
                {
                    Debug.WriteLine("Unable to open file '" + filename + "'");
                }
            }

            foreach (var sub in Directory.EnumerateDirectories(directory))
            {
                var filename = FindFilename(targetHash, sub);

                if (filename != string.Empty)
                    return filename;
            }

            return string.Empty;
        }
    }
}
