using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parchive.Library.IO
{
    public abstract class SerializationFactory
    {
        private static SerializationFactory _current;

        public static SerializationFactory Current
        {
            get
            {
                if (_current == null) throw new InvalidOperationException();
                return _current;
            }
        }

        protected SerializationFactory()
        {
            if (_current != null) throw new InvalidOperationException("You can only create one instance of the Portability Factory.");            

            var types = typeof(SerializationFactory).Assembly.GetTypes();

            foreach (var type in types)
            {
            }

            Debug.WriteLine("Setting SerializationFactory.Current");
            _current = this;
        }

        public T Deserialize<T>(byte[] input) where T : ISerializable
        {
            var o = default(T);
            o.Deserialize(input);

            return o;
        }

        public byte[] Serialize<T>(T o) where T : ISerializable
        {
            return o.Serialize();
        }
    }
}
