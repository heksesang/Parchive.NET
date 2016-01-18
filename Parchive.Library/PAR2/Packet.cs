using Parchive.Library.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.PAR2
{
    /// <summary>
    /// A PAR2 packet.
    /// </summary>
    public abstract class Packet
    {
        #region Constants
        /// <summary>
        /// Magic sequence. Used to quickly identify location of packets.
        /// </summary>
        public const UInt64 Signature = 6074036484213719376; // { 'P', 'A', 'R', '2', '\0', 'P', 'K', 'T' };
        #endregion

        #region Static Initialization
        static Packet()
        {
            var types = typeof(Packet).Assembly.GetTypes();

            foreach (var type in types.Where(x => !x.IsAbstract && typeof(Packet).IsAssignableFrom(x)))
            {
                var attributes = type.GetCustomAttributes(typeof(PacketAttribute), false) as PacketAttribute[];

                if (attributes != null && attributes.Length > 0)
                {
                    var packetType = attributes[0].Type;

                    if (_SupportedPackets.ContainsKey(packetType))
                    {
                        throw new Exception("Packet type already registered.");
                    }

                    _SupportedPackets[packetType] = type;
                }
            }
        }
        #endregion

        #region Static Members
        private static Dictionary<PacketType, Type> _SupportedPackets = new Dictionary<PacketType, Type>();

        /// <summary>
        /// Supported packet types.
        /// </summary>
        protected static IDictionary<PacketType, Type> SupportedPackets
        {
            get
            {
                return _SupportedPackets;
            }
        }

        /// <summary>
        /// Loads a packet from a stream.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <returns>The packet.</returns>
        public static Packet FromStream(Stream input)
        {
            Packet packet = null;

            if (input.Position < input.Length)
            {
                using (var br = new BinaryReader(input, Encoding.UTF8, true))
                {
                    if (br.ReadUInt64() != Signature)
                    {
                        throw new Exception("Invalid PAR2 header signature.");
                    }

                    var length = br.ReadInt64() - 64;
                    var hash = br.ReadBytes(16);
                    var recoverySetID = br.ReadBytes(16);
                    var packetType = new PacketType(br.ReadBytes(16));

                    if (length < 0 || (length % 4) != 0)
                    {
                        throw new InvalidPacketError("Invalid packet length.");
                    }

                    Type t;
                    if (!SupportedPackets.TryGetValue(packetType, out t))
                    {
                        input.Seek(length, SeekOrigin.Current);
                        throw new UnsupportedPacketError(packetType);
                    }

                    packet = Activator.CreateInstance(t) as Packet;

                    packet._Stream = input;
                    packet._Offset = input.Position;

                    packet.Hash = hash;
                    packet.RecoverySetID = recoverySetID;
                    try
                    {
                        packet.Initialize(input, length);
                    }
                    catch (NotImplementedException)
                    {
                        Debug.WriteLine(packet.GetType().Name + " isn't implemented.");
                    }
                    input.Seek(packet._Offset + length, SeekOrigin.Begin);
                }
            }

            return packet;
        }
        #endregion

        #region Fields
        protected Stream _Stream;
        protected Int64 _Offset;
        #endregion

        #region Properties
        /// <summary>
        /// The checksum of the packet.
        /// </summary>
        public byte[] Hash { get; private set; }

        /// <summary>
        /// The ID of the recovery set.
        /// </summary>
        public byte[] RecoverySetID { get; private set; }
        #endregion

        #region Methods
        protected abstract void Initialize(Stream input, long length);
        #endregion
    }
}
