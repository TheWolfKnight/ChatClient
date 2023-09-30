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
        // Object for synchronizing access to the network stream
        private readonly object _stream_semaphor = new object();

        // Object for synchronizing access to the message queue
        private readonly object _msg_queue_semaphor = new object();

        // Queue for storing messages to be displayed
        private Queue<string> MessageDisplay = null!;

        // Thread responsible for reading messages from the network
        private Thread ReadThread = null!;

        // TcpClient instance used for network communication
        public TcpClient Client = null!;

        // User's display name for identification
        private string DisplayName = null!;

        // Flag indicating whether the entered IP address is valid
        private bool ValidIp = false;

        // Flag indicating whether the entered port number is valid
        private bool ValidPort = false;

        // Network stream for communication with the server
        private NetworkStream Stream = null!;

        // BinaryWriter for writing data to the network stream
        private BinaryWriter Writer = null!;

        // Constructor for the Display class
        public Display() {
            // Initialize the component (assuming this is a Windows Forms class)
            InitializeComponent();

            // Create a message queue for storing messages to be displayed
            lock (_msg_queue_semaphor) {
                MessageDisplay = new Queue<string>();
            }
        }

        private void on_ButtonMouseClick(object sender, MouseEventArgs e) {
            // Check if the left mouse button was clicked; if not, do nothing
            if (e.Button != MouseButtons.Left) return;

            // Check if the entered IP address and port are valid
            if (!ValidIp || !ValidPort) {
                MessageBox.Show("Please check your Ip and Port");
                return;
            }

            // Retrieve the text from the IP and port input fields
            string ip_txt = txb_IpInput.Text;
            string port_txt = txb_PortInput.Text;

            // Attempt to parse the entered IP address into an IPAddress object
            if (!IPAddress.TryParse(ip_txt, out IPAddress? addr)) {
                // Display an error message if parsing fails
                MessageBox.Show("Could not parse the IP", "ERROR", MessageBoxButtons.OK);
                return;
            }

            // Attempt to parse the entered port number into an integer
            if (!int.TryParse(port_txt, out int port)) {
                // Display an error message if parsing fails
                MessageBox.Show("Could not parse the port", "ERROR", MessageBoxButtons.OK);
                return;
            }

            // Set the DisplayName to the entered nickname
            DisplayName = txb_NicknameInput.Text;

            // Create a new TcpClient instance to establish a connection
            TcpClient client = new TcpClient();

            // Start a background task to establish a connection to the server
            Task.Run(() => {
                // Attempt to connect to the server using the provided IP address and port
                client.Connect(addr, port);

                // Check if the client successfully connected to the server
                if (!client.Connected) {
                    // Display an error message if the connection fails
                    MessageBox.Show("Could not connect to server", "ERROR", MessageBoxButtons.OK);
                    return;
                }

                // Set the Client property to the connected TcpClient instance
                Client = client;

                // Lock access to the Stream property and set it to the client's network stream
                lock (_stream_semaphor)
                    Stream = Client.GetStream();

                // Create a new thread to handle reading and displaying messages
                ReadThread = new Thread(() => {
                    // Initialize a BinaryReader and lock access to the Stream
                    BinaryReader reader = null!;
                    lock (_stream_semaphor)
                        reader = new BinaryReader(Stream);

                    while (true) {
                        // Check if there is data available in the stream
                        if (Stream.DataAvailable) {
                            // Read a message from the stream and enqueue it for display
                            string msg = reader.ReadString();
                            MessageDisplay.Enqueue(msg);
                        }

                        // Initialize a variable to store the displayed messages
                        string msg_disp = "";

                        // Lock access to the MessageDisplay queue
                        lock (_msg_queue_semaphor) {

                            // Ensure that the number of messages in the queue does not exceed 10
                            while (MessageDisplay.Count() > 10) {
                                MessageDisplay.Dequeue();
                            }

                            // Concatenate the messages in the queue for display
                            foreach (string message in MessageDisplay) {
                                msg_disp += message + Environment.NewLine;
                            }

                        }
                        rtb_MessageDisplay.Invoke(() => {
                            rtb_MessageDisplay.Text = msg_disp;
                        });
                    }
                });
                // Set the ReadThread to run as a background thread, allowing it to exit when the application exits
                ReadThread.IsBackground = true;

                // Start the ReadThread, which handles reading messages from the network
                ReadThread.Start();

                // Lock access to the network stream semaphore to ensure exclusive access
                lock (_stream_semaphor) {
                    // Create a BinaryWriter to write data to the network stream
                    Writer = new BinaryWriter(Stream);

                    // Write the user's display name to the network stream
                    Writer.Write(DisplayName);
                }

                // disable the children in Africa
                btn_Connect.Enabled = false;

                // Display a success message indicating that the connection to the server has been established
                MessageBox.Show("Connection to server established", "Success", MessageBoxButtons.OK);
            });
        }

        // Event handler for validating an IP address input field
        private void on_IPValidate(object sender, EventArgs e) {

            // Extract the text from the sender, which should be a TextBox control
            string potential_ip = ((TextBox)sender).Text;

            // Define a regular expression pattern to match valid IP addresses (IPv4)
            Regex regex = new Regex(@"[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}");

            // Attempt to match the input text against the regular expression pattern
            Match match = regex.Match(potential_ip);

            // Set the ValidIp flag based on whether the input text matches the pattern
            ValidIp = match.Success;

            // Change the text color of the input field based on whether the IP is valid
            if (ValidIp)
                ((TextBox)sender).ForeColor = Color.Black; // Valid IP, set text color to black
            else
                ((TextBox)sender).ForeColor = Color.Red;   // Invalid IP, set text color to red
        }

        // Event handler for validating a port number input field
        private void on_PortValidate(object sender, EventArgs e) {

            // Extract the text from the sender, which should be a TextBox control
            string potential_port = ((TextBox)sender).Text;

            // Attempt to parse the input text as an integer to check for a valid port number
            if (int.TryParse(potential_port, out int port)) {
                // Check if the parsed port is greater than zero and less than or equal to the maximum port number (65535)
                bool greater_than_zero = port > -0;
                bool less_than_max = port <= 65535;

                // Set the ValidPort flag based on whether the port number is valid
                ValidPort = greater_than_zero && less_than_max;

                // Change the text color of the input field based on whether the port number is valid
                if (ValidPort)
                    ((TextBox)sender).ForeColor = Color.Black; // Valid port number, set text color to black
                else
                    ((TextBox)sender).ForeColor = Color.Red;   // Invalid port number, set text color to red

                return;
            }

            // If parsing fails, set the ValidPort flag to false and set the text color to red
            ValidPort = false;
            ((TextBox)sender).ForeColor = Color.Red;
        }

        // Event handler for handling a mouse click event on the "Send" button
        private void on_SendClick(object sender, MouseEventArgs e) {
            // Check if the left mouse button was clicked; if not, do nothing
            if (e.Button != MouseButtons.Left) return;

            // Get the message text from the message input field
            string msg = txb_MessageInput.Text;

            // Check if the message is empty; if it is, do nothing
            if (msg.Length < 1) return;

            // Write the message to the network stream using the BinaryWriter
            Writer.Write(msg);

            // Clear the message input field after sending the message
            txb_MessageInput.Text = "";
        }

        // Event handler for handling the form's closing event
        private void on_FormClosed(object sender, FormClosingEventArgs e) {
            // Close the TcpClient if it's not null
            Client?.Close();

            // Close the network stream if it's not null
            Stream?.Close();

            // Interrupt (stop) the ReadThread if it's not null
            ReadThread?.Interrupt();
        }
    }
}
