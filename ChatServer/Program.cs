using System.Net;
using System.Net.Quic;
using System.Net.Sockets;
using System.Reflection.Metadata;

using ChatAbastraction;

namespace ChatClient.Server;

public class Program {
    static void Main(string[] args) {
        IPAddress addr = IPAddress.Any;

        TcpListener chat_listener = new TcpListener(addr, 7001);
        TcpListener comm_listener = new TcpListener(addr, 7000);
        comm_listener.Start();
        chat_listener.Start();

        Thread comm_thread = StartCommandServer(comm_listener);
        comm_thread.IsBackground = false;
        Thread chat_thread = StartChatServer(chat_listener);
        chat_thread.IsBackground = false;

        bool quit = false;

        while (!quit) {
            char t = (char)Console.Read();

            if (t == 'q')
                quit = true;
        }

        comm_thread.Interrupt();
        chat_thread.Interrupt();

        comm_listener.Stop();
        chat_listener.Stop();

        return;
    }

    public static Thread StartCommandServer(TcpListener listener) {
        Thread comm_server_thead = new Thread(() => {
            while (true) {
                Thread thread = new Thread(async () => {
                    try {
                        using Socket socket = await listener.AcceptSocketAsync();
                        IPEndPoint? conn = socket.RemoteEndPoint as IPEndPoint;
                        System.Console.WriteLine($"[COMM] Accepted connection from: {conn}");
                        CommandConnectionHandler handler = new CommandConnectionHandler(socket);
                        handler.Handle();
                        System.Console.WriteLine($"[COMM] Closing connection to: {conn}");
                    }
                    catch (Exception e) {
                        System.Console.WriteLine(e.Message);
                    }
                });
                thread.Start();
            }
        });

        comm_server_thead.Start();

        return comm_server_thead;
    }

    public static Thread StartChatServer(TcpListener listener) {

        Thread chat_server_thread = new Thread(() => {
            ChatServerService chat = new ChatServerService(listener);
            chat.Handle();
        });

        chat_server_thread.Start();

        return chat_server_thread;

    }

}

class ChatServerService {
    private readonly object connection_semaphor = new object();

    private readonly TcpListener Listener = null!;

    public List<Connection> Connections = new List<Connection>();

    public ChatServerService(TcpListener listener) => Listener = listener;

    public IEnumerable<Connection> GetIncommingReads() {
        lock (connection_semaphor) {
            foreach (Connection conn in Connections)
                if (conn.Stream.DataAvailable) yield return conn;
            yield break;
        }
    }

    public void Handle() {
        while (true) {
            GetNewConnections();
            HandleIncomingRequests();
        }
    }

    private void GetNewConnections() {
        while (Listener.Pending()) {
            Socket socket = Listener.AcceptSocket();

            Thread thread = new Thread(() => {
                Connection connection;
                try {
                    connection = new Connection(socket);
                } catch (Exception) {
                    Console.WriteLine("Could not get a connection");
                    return;
                }

                string name = connection.Name;
                lock (connection_semaphor) {
                    Connections.Add(connection);
                }
                Console.WriteLine($"Add conncetion to: {name}");
            });
            thread.Start();
        }
    }

    private void HandleIncomingRequests() {
            foreach (Connection conn in GetIncommingReads()) {

                if (!conn.TryReadData(out string msg)) {
                    if (!conn.Connected) {
                        Connections.Remove(conn);
                        Connections.AsParallel()
                            .ForAll(Connection => {
                                Connection.TryWriteDate($"{Connection.Name} left the chat");
                            });
                    }
                    else
                        conn.TryWriteDate("Message could not be read, try again");
                }

                Connections.AsParallel()
                    .ForAll(Connection => {
                        Connection.TryWriteDate($"{conn.Name}: {msg}");
                    });
            }
    }
}

readonly struct CommandConnectionHandler {
    readonly Socket Connection;

    private readonly string help_msg = "type: [command] {optional args}\n" +
                                       "    help:  show this message\n" +
                                       "    echo:  sends and messeag containing echo\n" +
                                       "    date:  sends a message with the date\n" +
                                       "    quit:  leave server\n" +
                                       "fg_color: change the forground color\n" +
                                       "bg_color: change background color\n";

    public CommandConnectionHandler(Socket socket) => Connection = socket;

    public void Handle() {
        using NetworkStream stream = new NetworkStream(Connection);
        using BinaryWriter writer = new BinaryWriter(stream);
        using BinaryReader reader = new BinaryReader(stream);
        bool run = true;

        while (run) {
            string? com = reader.ReadString();

            if (com == null) {
                writer.Write("send \"help\" for help");
                continue;
            }

            string[] comms = com.Split(' ');

            switch (comms[0].ToLower()) {
                case "date":
                    writer.Write($"The date is: {DateTime.Now:d}");
                    break;
                case "echo":
                    writer.Write(string.Join(' ', comms.Skip(1)));
                    break;
                case "help":
                    writer.Write(help_msg);
                    break;
                case "fg_color":
                    if (comms.Length < 2) {
                        writer.Write("You must provide a color");
                    }
                    ChangeFGColor(comms[1]);
                    writer.Write("done");
                    break;
                case "bg_color":
                    if (comms.Length < 2) {
                        writer.Write("You must provide a color");
                    }
                    ChangeBGColor(comms[1]);
                    writer.Write("done");
                    break;
                case "quit":
                    writer.Write("quiting");
                    run = false;
                    break;
                default:
                    writer.Write(help_msg);
                    break;
            }

            IPEndPoint? conn = Connection.RemoteEndPoint as IPEndPoint;
            System.Console.WriteLine($"[COMM] Connection: {conn} called command: {com}");
        }

        Connection.Close();
    }

    private void ChangeFGColor(string color) {
        var fg_color = color.ToLower() switch {
            "red" => ConsoleColor.Red,
            "white" => ConsoleColor.White,
            "black" => ConsoleColor.Black,
            "yellow" => ConsoleColor.Yellow,
            "blue" => ConsoleColor.Blue,
            "green" => ConsoleColor.Green,
            _ => ConsoleColor.White,
        };
        Console.ForegroundColor = fg_color;
    }

    private void ChangeBGColor(string color) {
        var bg_color = color.ToLower() switch {
            "red" => ConsoleColor.Red,
            "white" => ConsoleColor.White,
            "black" => ConsoleColor.Black,
            "yellow" => ConsoleColor.Yellow,
            "blue" => ConsoleColor.Blue,
            "green" => ConsoleColor.Green,
            _ => ConsoleColor.White,
        };
        Console.BackgroundColor = bg_color;
    }
}
