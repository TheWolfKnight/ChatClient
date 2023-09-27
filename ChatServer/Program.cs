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
        Thread chat_thread = StartChatServer(chat_listener);

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

        });

        chat_server_thread.Start();

        return chat_server_thread;

    }

}

class ChatServerService {

    private readonly TcpListener Listener = null!;
    private readonly List<Connection> Connections = new List<Connection>();

    public ChatServerService(TcpListener listener) {
        Listener = listener;
    }

    public IEnumerable<Connection> GetIncommingReads() {
        foreach (Connection connection in Connections) {
            if (connection.Stream.DataAvailable) yield return connection;
        }
        yield break;
    }

    /*
     COMMS SPECS:
        HEADER DEFINITION:
            <{SCOPE} {INSTRUCTION}>{OPTIOANL DATA}</{SCOPE} {INSTRUCTION}>
            SCOPE:
                LEN: 3 char
                SCOPES: -HDR header
                        -PUB public
                        -PRI private
                        -SND sender
                        -RCV reciver
                        -USR user
            INSTRUCTION
                LEN: 3 char
                INSTRUCTIONS: -MSG message
                              -CNT count
                              -LVN leaving
                              -JON Join
        EXAMPLE HEADERS
            <HDR CNT>3</HDR CNT>
            <PUB MSG/>
            <SND MSG>User123</SND MSG>
            {MESSAGE FOR HERE ON OUT}
     */

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
        ConsoleColor fg_color;
        switch (color.ToLower()) {
            case "red":
                fg_color = ConsoleColor.Red;
                break;
            case "white":
                fg_color = ConsoleColor.White;
                break;
            case "black":
                fg_color = ConsoleColor.Black;
                break;
            case "yellow":
                fg_color = ConsoleColor.Yellow;
                break;
            case "blue":
                fg_color = ConsoleColor.Blue;
                break;
            case "green":
                fg_color = ConsoleColor.Green;
                break;
            default:
                fg_color = ConsoleColor.White;
                break;
        }
        Console.ForegroundColor = fg_color;
    }

    private void ChangeBGColor(string color) {
        ConsoleColor bg_color;
        switch (color.ToLower()) {
            case "red":
                bg_color = ConsoleColor.Red;
                break;
            case "white":
                bg_color = ConsoleColor.White;
                break;
            case "black":
                bg_color = ConsoleColor.Black;
                break;
            case "yellow":
                bg_color = ConsoleColor.Yellow;
                break;
            case "blue":
                bg_color = ConsoleColor.Blue;
                break;
            case "green":
                bg_color = ConsoleColor.Green;
                break;
            default:
                bg_color = ConsoleColor.White;
                break;
        }
        Console.BackgroundColor = bg_color;
    }
}
