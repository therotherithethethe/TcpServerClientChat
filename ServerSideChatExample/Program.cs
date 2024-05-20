using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace ServerSideChatExample;

internal class Program
{
    /*public static void Main(string[] args)
    {
        try
        {
            var listener = new TcpListener(IPAddress.Any, 13337);
            listener.Start();
            Console.WriteLine("Server is listening...");

            while (true)
            {
                using var client = listener.AcceptTcpClient();
                Console.WriteLine("Client connected.");
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream);
                using var writer = new StreamWriter(stream) { AutoFlush = true };

                var hostAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

                while (client.Connected)
                {
                    var message = reader.ReadLine();
                    if (message == null)
                        break;

                    Console.WriteLine($"{hostAddress}: {message}");

                    writer.WriteLine($"{hostAddress}: {message}");
                }

                Console.WriteLine($"Client disconnected. {hostAddress}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }*/
    private static ConcurrentDictionary<string, TcpClient> clients = new ConcurrentDictionary<string, TcpClient>();

    public static async Task Main(string[] args)
    {
        try
        {
            var listener = new TcpListener(IPAddress.Any, 13337);
            listener.Start();
            Console.WriteLine("Server is listening...");

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                var clientEndpoint = client.Client.RemoteEndPoint.ToString();
                clients.TryAdd(clientEndpoint, client);
                Console.WriteLine("Client connected.");
                _ = Task.Run(() => HandleClientAsync(client));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private static async Task HandleClientAsync(TcpClient client)
    {
        var hostAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
        var clientEndpoint = client.Client.RemoteEndPoint.ToString();

        try
        {
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream);
            using var writer = new StreamWriter(stream) { AutoFlush = true };

            while (client.Connected)
            {
                var message = await reader.ReadLineAsync();
                if (message == null)
                    break;

                Console.WriteLine($"{hostAddress}: {message}");

                // Отправка сообщения всем подключенным клиентам
                foreach (var kvp in clients)
                {
                    if (kvp.Key != clientEndpoint) // не отправлять сообщение обратно отправителю
                    {
                        var otherClient = kvp.Value;
                        if (otherClient.Connected)
                        {
                            using var otherStream = otherClient.GetStream();
                            using var otherWriter = new StreamWriter(otherStream) { AutoFlush = true };
                            await otherWriter.WriteLineAsync($"{hostAddress}: {message}");
                        }
                    }
                }

                await writer.WriteLineAsync($"{hostAddress}: {message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred with client {hostAddress}: {ex.Message}");
        }
        finally
        {
            Console.WriteLine($"Client disconnected. {hostAddress}");
            clients.TryRemove(clientEndpoint, out _);
            client.Close();
        }
    }
}