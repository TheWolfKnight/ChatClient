
using System.Net;
using System.Net.Sockets;

namespace ChatServer.Server.Services;

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

