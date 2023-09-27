using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatFrontEnd {
    public partial class Display : Form {

        private object _client_semaphor = new object();

        private Task _ConnectToServer = null!;
        private TcpClient _Client = null!;

        public TcpClient Client {
            get {
                lock (_client_semaphor) return _Client;
            }
            set {
                lock (_client_semaphor) _Client = value;
            }
        }

        private NetworkStream Stream = null!;
        private BinaryReader Reader = null!;
        private BinaryWriter Writer = null!;

        public Display() {
            InitializeComponent();
        }

        private void on_ButtonMouseClick(object sender, MouseEventArgs e) {
            if (e.Button != MouseButtons.Left) return;

            string ip_txt = txb_IpInput.Text;
            string port_txt = txb_PortInput.Text;

            if (!IPAddress.TryParse(ip_txt, out IPAddress? addr)) {
                MessageBox.Show("could not parse the IP", "ERROR", MessageBoxButtons.OK);
                return;
            }

            if (!int.TryParse(port_txt, out int port)) {
                MessageBox.Show("Could not parse the port", "ERROR", MessageBoxButtons.OK);
                return;
            }

            TcpClient client = new TcpClient();

            _ConnectToServer = new Task(() => {
                client.Connect(addr, port);

                if (!client.Connected) {
                    MessageBox.Show("Could not connect to server", "ERROR", MessageBoxButtons.OK);
                    return;
                }

                Client = client;
                Stream = Client.GetStream();
                Reader = new BinaryReader(Stream);
                Writer = new BinaryWriter(Stream);

                MessageBox.Show("Connection to server established", "Success", MessageBoxButtons.OK);
            });
        }
    }
}
