using Parchive.Library.Exceptions;
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
    /// <summary>
    /// A PAR2 packet.
    /// </summary>
    public abstract class Packet
    {
        #region Constants
        /// <summary>
        /// The PAR2 packet signature. Each packet starts with this signature.
        /// </summary>
        public const long Signature = 6074036484213719376; // { 'P', 'A', 'R', '2', '\0', 'P', 'K', 'T' };
        #endregion

        #region Static Initialization
        /// <summary>
        /// Static constructor which registers any classes in the assembly which are tagged with PacketAttribute.
        /// </summary>
        /// <exception cref="Parchive.Library.Exceptions.InitializationError">
        /// There exists multiple PacketAttributes with the same PacketType value.
        /// </exception>
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
                        throw new InitializationError("Packet type already registered.");
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
        /// Reads a PAR2 packet from an input stream.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <returns>A PAR2 packet.</returns>
        /// <exception cref="Parchive.Library.Exceptions.InvalidPacketError">
        /// The data in the stream is not a valid PAR2 packet.
        /// </exception>
        /// <exception cref="Parchive.Library.Exceptions.UnsupportedPacketError">
        /// The packet type is not supported by this library.
        /// </exception>
        /// <exception cref="Parchive.Library.Exceptions.TooLargeNumberError">
        /// The length of the packet is too large.
        /// </exception>
        public static Packet FromStream(Stream input)
        {
            Packet packet = null;

            if (input.Position < input.Length)
            {
                var reader = new BinaryReader(input, Encoding.UTF8, true);

                if (reader.ReadInt64() != Signature)
                {
                    throw new InvalidPacketError("Invalid PAR2 header signature.");
                }

                var length = reader.ReadInt64() - 64;
                
                if (length < 0)
                {
                    throw new TooLargeNumberError();
                }

                var hash = reader.ReadBytes(16);
                var recoverySetID = reader.ReadBytes(16);
                var packetType = new PacketType(reader.ReadBytes(16));

                if ((length % 4) != 0)
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
                
                packet._Offset = reader.BaseStream.Position;
                packet._Length = length;

                packet.Checksum = hash;
                packet.RecoverySetID = recoverySetID;
                
                packet.Initialize(reader);

                if (!packet.Verify(reader))
                {
                    throw new InvalidPacketError("Verification failed.");
                }

                input.Seek(packet._Offset + packet._Length, SeekOrigin.Begin);
            }

            return packet;
        }

        /// <summary>
        /// Gets the packet type object for a packet type.
        /// </summary>
        /// <typeparam name="TPacket">The packet type to get the object for.</typeparam>
        /// <returns>The packet type object.</returns>
        public static PacketType GetPacketType<TPacket>()
        {
            return _SupportedPackets.Where(x => x.Value == typeof(TPacket)).Select(x => x.Key).FirstOrDefault();
        }
        #endregion

        #region Fields
        protected long _Offset;
        protected long _Length;
        #endregion

        #region Properties
        /// <summary>
        /// The checksum of the packet.
        /// Calculated from RecoverySetID + PacketType + packet body.
        /// </summary>
        public byte[] Checksum { get; private set; }

        /// <summary>
        /// The ID of the recovery set.
        /// </summary>
        public byte[] RecoverySetID { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Initializes the packet from a stream.
        /// </summary>
        /// <param name="reader">The reader that provides access to the stream.</param>
        protected abstract void Initialize(BinaryReader reader);

        /// <summary>
        /// Verifies the integrity of the packet by calculating the checksum of
        /// the RecoverySetID, PacketType, and the remaining data from a stream.
        /// </summary>
        /// <param name="reader">The reader that provides access to the stream.</param>
        /// <returns>true if the integrity of the packet is intact; otherwise, false.</returns>
        public bool Verify(BinaryReader reader)
        {
            using (var crypt = MD5.Create())
            {
                crypt.TransformBlock(RecoverySetID, 0, RecoverySetID.Length, null, 0);

                var attributes = (PacketAttribute[])this.GetType().GetCustomAttributes(typeof(PacketAttribute), false);
                if (attributes.Length == 0)
                {
                    throw new VerificationError("PacketAttribute is required to verify a packet.");
                }
                var packetType = attributes[0].Type.Identifier;

                crypt.TransformBlock(packetType, 0, packetType.Length, null, 0);

                reader.BaseStream.Seek(_Offset, SeekOrigin.Begin);
                var body = reader.ReadBytes((int)_Length);
                crypt.TransformFinalBlock(body, 0, body.Length);

                if (crypt.Hash.SequenceEqual(Checksum))
                {
                    return true;
                }

                return false;
            }
        }
        #endregion
    }
}