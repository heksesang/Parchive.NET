using Parchive.Library.Exceptions;
using Parchive.Library.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.PAR2
{
    /// <summary>
    /// An abstract base class for PAR2 packets.
    /// </summary>
    public abstract class Packet
    {
        /// <summary>
        /// Factory class for creating <see cref="Packet"/> objects.
        /// </summary>
        public class Factory
        {
            #region Fields
            /// <summary>
            /// Supported packet types.
            /// </summary>
            private Dictionary<PacketType, Type> _SupportedPackets = new Dictionary<PacketType, Type>();
            #endregion

            #region Properties
            /// <summary>
            /// Supported packet types.
            /// </summary>
            protected IReadOnlyDictionary<PacketType, Type> SupportedPackets
            {
                get
                {
                    return _SupportedPackets;
                }
            }
            #endregion

            #region Constructors
            /// <summary>
            /// Constructs a <see cref="Factory"/> supporting any <see cref="Packet"/> classes with <see cref="PacketAttribute"/> in the given assemblies.
            /// </summary>
            /// <param name="assemblies">The assemblies to search.</param>
            public Factory(params Assembly[] assemblies) : this(assemblies as IEnumerable<Assembly>)
            {
            }

            /// <summary>
            /// Constructs a <see cref="Factory"/> supporting any <see cref="Packet"/> classes with <see cref="PacketAttribute"/> in the given assemblies.
            /// </summary>
            /// <param name="assemblies">The assemblies to search.</param>
            public Factory(IEnumerable<Assembly> assemblies)
            {
                foreach (var assembly in assemblies)
                {
                    foreach (var type in assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && typeof(Packet).IsAssignableFrom(x)))
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
            }
            #endregion

            #region Methods
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
            public Packet FromStream(Stream input)
            {
                Packet packet = null;

                if (input.Position < input.Length)
                {
                    using (var reader = new BinaryReader(input, Encoding.ASCII, true))
                    {
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

                        packet.Checksum = hash;
                        packet.RecoverySetID = recoverySetID;

                        packet.Initialize(new ConstrainedStream(reader.BaseStream, reader.BaseStream.Position, length, true));

                        if (!packet.Verify())
                        {
                            throw new InvalidPacketError("Verification failed.");
                        }
                    }
                }

                return packet;
            }

            /// <summary>
            /// Gets the packet type object for a packet type.
            /// </summary>
            /// <typeparam name="TPacket">The packet type to get the object for.</typeparam>
            /// <returns>The packet type object.</returns>
            public PacketType GetPacketType<TPacket>() where TPacket : Packet
            {
                return GetPacketType(typeof(TPacket));
            }

            /// <summary>
            /// Gets the packet type object for a packet type.
            /// </summary>
            /// <typeparam name="TPacket">The packet type to get the object for.</typeparam>
            /// <returns>The packet type object.</returns>
            public PacketType GetPacketType(Type type)
            {
                return _SupportedPackets.Where(x => x.Value.FullName == type.FullName).Select(x => x.Key).FirstOrDefault();
            }
            #endregion
        }

        #region Constants
        /// <summary>
        /// The PAR2 packet signature. Each packet starts with this signature.
        /// </summary>
        public const long Signature = 6074036484213719376; // { 'P', 'A', 'R', '2', '\0', 'P', 'K', 'T' };
        #endregion

        #region Static Members
        public static readonly Factory DefaultFactory = new Factory(typeof(Packet).Assembly);
        #endregion

        #region Properties
        /// <summary>
        /// The checksum of the packet.
        /// </summary>
        protected byte[] Checksum { get; set; }

        /// <summary>
        /// The packet type.
        /// </summary>
        protected PacketType PacketType { get; set; }

        /// <summary>
        /// The ID of the recovery set.
        /// </summary>
        protected byte[] RecoverySetID { get; set; }

        /// <summary>
        /// The packet body in the form of a <see cref="Stream"/> object.
        /// </summary>
        public abstract Stream Body
        {
            get;
        }

        /// <summary>
        /// The packet header in the form of a <see cref="Stream"/> object.
        /// </summary>
        public Stream Header
        {
            get
            {
                using (var writer = new BinaryWriter(new MemoryStream(new byte[64]), Encoding.ASCII, true))
                {
                    writer.Write(Signature);
                    writer.Write(64 + Body.Length);
                    writer.Write(Checksum);
                    writer.Write(RecoverySetID);
                    writer.Write(PacketType.Identifier);

                    writer.BaseStream.Seek(0, SeekOrigin.Begin);

                    return writer.BaseStream;
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initializes the packet from a stream through a <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">A <see cref="Stream"/> containing the packet.</param>
        protected abstract void Initialize(Stream stream);

        /// <summary>
        /// Calculates the checksum of the packet.
        /// </summary>
        /// <returns>A <see cref="byte[]"/> containing a MD5 hash of the packet.</returns>
        protected byte[] CalculateChecksum()
        {
            using (var crypt = MD5.Create())
            {
                using (var reader = new BinaryReader(Header, Encoding.ASCII))
                {
                    reader.BaseStream.Seek(32, SeekOrigin.Begin);
                    var header = reader.ReadBytes(32);
                    crypt.TransformBlock(header, 0, header.Length, null, 0);
                }

                using (var reader = new BinaryReader(Body, Encoding.ASCII))
                {
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        var body = reader.ReadBytes(1048576);
                        crypt.TransformBlock(body, 0, body.Length, null, 0);
                    }

                    crypt.TransformFinalBlock(null, 0, 0);
                }

                return crypt.Hash;
            }
        }

        /// <summary>
        /// Updates the stored checksum of the packet.
        /// </summary>
        protected void UpdateChecksum()
        {
            Checksum = CalculateChecksum();
        }

        /// <summary>
        /// Verifies the integrity of the packet by calculating the checksum of the packet and comparing it to the stored checksum.
        /// </summary>
        /// <returns>true if the integrity of the packet is intact; otherwise, false.</returns>
        protected bool Verify()
        {
            if (CalculateChecksum().SequenceEqual(Checksum))
            {
                return true;
            }

            return false;
        }
        #endregion
    }
}