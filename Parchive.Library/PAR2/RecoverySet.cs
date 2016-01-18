using Parchive.Library.Exceptions;
using Parchive.Library.PAR2.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.PAR2
{
    /// <summary>
    /// A PAR2 recovery set.
    /// </summary>
    public class RecoverySet
    {
        #region Fields
        /// <summary>
        /// The Main packet of this recovery set.
        /// </summary>
        private MainPacket _Main;

        /// <summary>
        /// The File Description packets of this recovery set.
        /// </summary>
        private List<FileDescriptionPacket> _FileDescriptions = new List<FileDescriptionPacket>();

        /// <summary>
        /// The Input File Slice Checksum packets of this recovery set.
        /// </summary>
        private List<InputFileSliceChecksumPacket> _InputFileSliceChecksums = new List<InputFileSliceChecksumPacket>();
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sliceSize">The slice size of the recovery set. Must be a multiple of 4.</param>
        /// <param name="creator">The client who created this recovery set.</param>
        public RecoverySet(UInt64 sliceSize, string creator)
        {
            _Main = new MainPacket();
            _Main.SliceSize = sliceSize;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="inputs">Input data to read packets from.</param>
        public RecoverySet(IEnumerable<Stream> inputs)
        {
            foreach (var input in inputs)
            {
                Packet packet = null;

                do
                {
                    if (packet != null)
                    {
                        if (packet is MainPacket)
                        {
                            _Main = (MainPacket)packet;
                        }
                        else if (packet is FileDescriptionPacket)
                        {
                            _FileDescriptions.Add((FileDescriptionPacket)packet);
                        }
                        else if (packet is InputFileSliceChecksumPacket)
                        {
                            _InputFileSliceChecksums.Add((InputFileSliceChecksumPacket)packet);
                        }
                    }

                    try
                    {
                        packet = Packet.FromStream(input);
                    }
                    catch (UnsupportedPacketError)
                    {
                        
                    }
                }
                while (packet != null);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a file to the recovery set.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="input"></param>
        public void AddFile(string filename, Stream input)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes a file from the recovery set.
        /// </summary>
        /// <param name="filename">The filename of the file.</param>
        public void RemoveFile(string filename)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
