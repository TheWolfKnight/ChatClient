using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

using ChatAbastraction;

namespace ChatFrontEnd {
    public partial class Display : Form {

        private object _client_semaphor = new object();
        private object _stream_semaphor = new object();
        private object _msg_queue_semaphor = new object();

        private TcpClient _Client = null!;

        private Queue<string> MessageDisplay {
            get {
                lock (_msg_queue_semaphor) return MessageDisplay;
            }
            set {
                lock (_msg_queue_semaphor) MessageDisplay = value;
            }
        }

        private Thread ReadThread = null!;

        public TcpClient Client {
            get {
                lock (_client_semaphor) return _Client;
            }
            set {
                lock (_client_semaphor) _Client = value;
            }
        }

        private bool ValidIp = false;
        private bool ValidPort = false;

        private NetworkStream Stream {
            get {
                lock (_stream_semaphor) return Stream;
            }
            set {
                lock (_stream_semaphor) Stream = value;
            }
        }

        private BinaryWriter Writer = null!;

        public Display() {
            InitializeComponent();
            MessageDisplay = new Queue<string>();
        }

        private void on_ButtonMouseClick(object sender, MouseEventArgs e) {
            if (e.Button != MouseButtons.Left) return;

            if (!ValidIp || !ValidPort) {
                MessageBox.Show("Please check your Ip and Port");
                return;
            }

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

            Task.Run(() => {
                client.Connect(addr, port);

                if (!client.Connected) {
                    MessageBox.Show("Could not connect to server", "ERROR", MessageBoxButtons.OK);
                    return;
                }

                Client = client;
                Stream = Client.GetStream();

                ReadThread = new Thread(() => {
                    BinaryReader reader = new BinaryReader(Stream);

                    while (true) {
                        if (Stream.DataAvailable) {
                            string msg = reader.ReadString();
                            MessageDisplay.Enqueue(msg);
                        }
                    }
                });
                ReadThread.Start();

                Writer = new BinaryWriter(Stream);

                MessageBox.Show("Connection to server established", "Success", MessageBoxButtons.OK);
            });
        }

        private void on_IPValidate(object sender, EventArgs e) {

            string potential_ip = ((TextBox)sender).Text;

            Regex regex = new Regex(@"[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}");
            Match match = regex.Match(potential_ip);

            ValidIp = match.Success;

            if (ValidIp)
                ((TextBox)sender).ForeColor = Color.Black;
            else
                ((TextBox)sender).ForeColor = Color.Red;
        }

        private void on_PortValidate(object sender, EventArgs e) {

            string potential_port = ((TextBox)sender).Text;

            if (int.TryParse(potential_port, out int port)) {
                bool greater_than_zero = port > -0;
                bool less_than_max = port <= 65535;

                ValidPort = greater_than_zero && less_than_max;

                if (ValidPort)
                    ((TextBox)sender).ForeColor = Color.Black;
                else
                    ((TextBox)sender).ForeColor = Color.Red;

                return;
            }

            ValidPort = false;
            ((TextBox)sender).ForeColor = Color.Red;
        }

        private void on_SendClick(object sender, MouseEventArgs e) {
            if (e.Button != MouseButtons.Left) return;

            string msg = txb_MessageInput.Text;

            if (msg.Length < 1) return;

            Writer.Write(msg);
        }

        ~Display() {
            Client?.Close();
            Stream?.Close();
            ReadThread?.Interrupt();
        }
    }
}
