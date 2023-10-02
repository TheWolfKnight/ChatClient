using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatAbastraction.Models
{
    public struct Connection
    {
        private object _name_semaphor = new object();
        private string _name = null!;

        public string Name
        {
            get { lock (_name_semaphor) return _name; }
            set { lock (_name_semaphor) _name = value; }
        }

        public bool Connected = true;

        private Socket Conn;

        public NetworkStream Stream { get; private set; }
        private BinaryReader Reader;
        private BinaryWriter Writer;

        public Connection(string name, Socket conn)
        {
            _name = name;
            Conn = conn;
            Stream = new NetworkStream(conn);
            Reader = new BinaryReader(Stream);
            Writer = new BinaryWriter(Stream);
        }

        public bool WriteDate(string data)
        {
            try
            {
                Writer.Write(data);
                return true;
            }
            catch (IOException)
            {
                Connected = false;
                return false;
            }
        }

        public bool TryReadData(out string result)
        {
            if (!Stream.DataAvailable)
            {
                result = null!;
                return false;
            }

            try
            {
                string data = Reader.ReadString();
                result = data;
                return true;
            }
            catch (Exception)
            {
                result = null!;
                return false;
            }
        }
    }
}
