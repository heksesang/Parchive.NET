using Parchive.Library.PAR2;
using Parchive.Library.PAR2.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parchive.Library.IO
{
    /// <summary>
    /// Contains metadata of a PAR2 file.
    /// </summary>
    public struct RecoveryFile
    {
        #region Properties
        /// <summary>
        /// The path of the file.
        /// </summary>
        public Uri Location { get; set; }
        
        /// <summary>
        /// The set name. This is the base filename of the file.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The exponents that are present in this file.
        /// </summary>
        public Range<long> Exponents { get; set; }
        #endregion

        #region Constants
        private const string _Regex = @"(.+?)(?:\.vol(\d+)\+(\d+))*(\.PAR2|\.par2)";
        #endregion

        #region Methods
        /// <summary>
        /// Gets a list of source files used to create this PAR2 file.
        /// </summary>
        /// <returns>A collection of SourceFile objects.</returns>
        public IEnumerable<SourceFile> GetSourceList()
        {
            var yieldedFiles = new List<FileID>();
            
            using (var reader = new ParReader(StreamBuilder.GetContent(Location)))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var fd = reader.GetNextPacket(Packet.GetPacketType<FileDescriptionPacket>()) as FileDescriptionPacket;

                    if (fd == null)
                        break;

                    if (yieldedFiles.Contains(fd.FileID))
                        continue;

                    yield return new SourceFile(fd);
                }
            }
        }

        public async Task<Stream> GetContentAsync()
        {
            return await StreamBuilder.GetContentAsync(Location);
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Parses the URI of a PAR2 file and extracts any metadata from the filename.
        /// </summary>
        /// <param name="uri">The filename of the PAR2 file.</param>
        /// <returns>A RecoveryFile object containing the metadata.</returns>
        public static RecoveryFile FromUri(Uri uri)
        {
            FileInfo fi;

            if (uri.IsAbsoluteUri)
                fi = new FileInfo(uri.AbsolutePath);
            else
                fi = new FileInfo(uri.ToString());

            var m = Regex.Match(fi.Name, _Regex);

            long start;
            long count;

            Range<long> range = null;

            if (long.TryParse(m.Groups[2].Value, out start) && long.TryParse(m.Groups[3].Value, out count))
            {
                range = new Range<long>
                {
                    Minimum = start,
                    Maximum = start + count - 1
                };
            }

            return new RecoveryFile
            {
                Location = fi.Exists ? new Uri(fi.FullName) : uri,
                Name = m.Groups[1].Value,
                Exponents = range
            };
        }
        #endregion
    }
}
