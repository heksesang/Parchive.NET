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
        private Dictionary<FileID, FileDescriptionPacket> _FileDescriptions
            = new Dictionary<FileID, FileDescriptionPacket>();

        /// <summary>
        /// The Input File Slice Checksum packets of this recovery set.
        /// </summary>
        private Dictionary<FileID, InputFileSliceChecksumPacket> _InputFileSliceChecksums
            = new Dictionary<FileID, InputFileSliceChecksumPacket>();

        /// <summary>
        /// The Recovery Slice packets of this recovery set.
        /// </summary>
        private List<RecoverySlicePacket> _RecoverySlices
            = new List<RecoverySlicePacket>();
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
                        if (packet is RecoverySlicePacket)
                        {
                            _RecoverySlices.Add((RecoverySlicePacket)packet);
                        }
                        else if (packet.Verify())
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
                                    _FileDescriptions[fd.FileID] = fd;
                            }
                            else if (packet is InputFileSliceChecksumPacket)
                            {
                                var ifsc = (InputFileSliceChecksumPacket)packet;
                                if (!_InputFileSliceChecksums.ContainsKey(ifsc.FileID))
                                    _InputFileSliceChecksums[ifsc.FileID] = ifsc;
                            }
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
                while (input.Position < input.Length);
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

        /// <summary>
        /// Repairs the input data with the available recovery slices.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IEnumerable<Stream> Reconstruct(IEnumerable<Stream> input)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
