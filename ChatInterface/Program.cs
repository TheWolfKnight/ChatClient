
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;

namespace ChatClient.Client;

public class Program
{
    static void Main(string[] args)
    {
        TcpClient client = new TcpClient();
        System.Console.WriteLine("Type the ip to connect to");
        string? ip = Console.ReadLine();
        if (!IPAddress.TryParse(ip, out IPAddress? ipAddr))
        {
            System.Console.WriteLine("Could not parse the ip");
            return;
        }

        System.Console.WriteLine("Type the port");
        string? port = Console.ReadLine();
        if (!int.TryParse(port, out int port_nr))
        {
            System.Console.WriteLine("Could not parse port");
            return;
        }

        client.Connect(ipAddr, port_nr);

        Handle(client);

        client.Close();
    }

    private static void Handle(TcpClient client)
    {
        try
        {
            bool run = true;

            using NetworkStream stream = client.GetStream();
            using BinaryReader reader = new BinaryReader(stream);
            using BinaryWriter writer = new BinaryWriter(stream);

            System.Console.WriteLine("Type command \"help\" for commands");
            while (run)
            {
                string? com = System.Console.ReadLine();
                if (com == null)
                {
                    System.Console.WriteLine("type \"help\" for commands");
                    continue;
                }

                System.Console.WriteLine("Sending command");
                writer.Write(com);
                System.Console.WriteLine("Awaiting response");
                if (com == "quit") run = false;

                string? response = reader.ReadString();

                if (response == null)
                {
                    System.Console.WriteLine("Response lost");
                    continue;
                }

                System.Console.WriteLine($"Response: {response}");
            }
        }
        catch (Exception)
        {
            System.Console.WriteLine("Closing client");
        }
    }

}
