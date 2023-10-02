
using System.Net;
using System.Net.Sockets;

using ChatAbastraction.Models;

namespace ChatServer.Server.Services;

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
                                Connection.TryWriteData($"{Connection.Name} left the chat");
                            });
                    }
                    else
                        conn.TryWriteData("Message could not be read, try again");
                }

                Connections.AsParallel()
                    .ForAll(Connection => {
                        Connection.TryWriteData($"{conn.Name}: {msg}");
                    });
            }
    }
}
