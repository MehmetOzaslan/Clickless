using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Clickless
{
    public class MLClientWebsocket
    {
        public readonly static string echoTextURL = "ws://localhost:8210/echotext";
        public readonly static string echoBytesURL = "ws://localhost:8210/echobytes";
        public readonly static string imageSaveURL = "ws://localhost:8210/imagesave";
        public readonly static string imagemlURL = "ws://localhost:8210/imageml";

        private readonly ClientWebSocket _client;

        public ClientWebSocket Client => _client;

        public MLClientWebsocket()
        {
            _client = new ClientWebSocket();
        }

        public async Task ConnectAsync(Uri uri)
        {
            await _client.ConnectAsync(uri, CancellationToken.None);
            Console.WriteLine("Connected to " + uri.ToString());
        }

        public async Task DisconnectAsync()
        {
            await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client Closed", CancellationToken.None);
            _client.Dispose();
            Console.WriteLine("Client Disconnected");
        }

        public async Task Abort()
        {
            await _client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Client Closed", CancellationToken.None);
            _client.Dispose();
            Console.WriteLine("Client Disconnected");
        }

        public async Task ReceiveMessages()
        {
            //Create a buffer and read the following amount per chunk.
            var buffer = new byte[1024];
            while (_client.State == WebSocketState.Open)
            {
                var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    //TODO: Determine type / parse json objects. Probably use newtonsoft for this.
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received: {message}");
                }
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[1024 * 4];

            while (Client.State == WebSocketState.Open)
            {
                var result = await Client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await Client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    ProcessMessage(message);
                }
            }
        }


        private ConcurrentDictionary<string, TaskCompletionSource<string>> _responseHandlers = new ConcurrentDictionary<string, TaskCompletionSource<string>>();

        public async Task<string> SendImageAndWaitForResponseAsync(string message, string identifier)
        {
            var tcs = new TaskCompletionSource<string>();
            _responseHandlers[identifier] = tcs;

            await SendMessageAsync(message);

            return await tcs.Task; // Wait for response
        }

        private void ProcessMessage(string message)
        {
            // Assuming the message contains an identifier to correlate responses
            var identifier = message.GetHashCode().ToString();
            if (_responseHandlers.TryRemove(identifier, out var tcs))
            {
                tcs.SetResult(message);
            }
            else
            {
                // Handle general messages or log unexpected ones
                Console.WriteLine("Received: " + message);
            }
        }


        public async Task<string> ReceiveMessageAsync()
        {
            var buffer = new byte[1024 * 30];
            var result = await Client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            return Encoding.UTF8.GetString(buffer, 0, result.Count);
        }

        public async Task<byte[]> ReceiveByteMessageAsync()
        {
            var buffer = new byte[1024 * 1000 * 10];
            var result = await Client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var receivedBytes = new byte[result.Count];
            Array.Copy(buffer, receivedBytes, result.Count);
            return receivedBytes;
        }

        public async Task SendMessageAsync(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await _client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine($"Sent: {message}");
        }

        public async Task SendImageAsync(System.Drawing.Image image)
        {
            //Convert it to a byte array.
            byte[] imageBytes;
            using (var ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                imageBytes = ms.ToArray();
            }

            //Write it to the websocket in chunks.
            var bufferSize = 1024*4;
            var totalSent = 0;

            while (totalSent < imageBytes.Length)
            {
                var chunkSize = Math.Min(bufferSize, imageBytes.Length - totalSent);
                var buffer = new ArraySegment<byte>(imageBytes, totalSent, chunkSize);
                await _client.SendAsync(buffer, WebSocketMessageType.Binary, chunkSize == imageBytes.Length - totalSent, CancellationToken.None);
                totalSent += chunkSize;
            }
            Console.WriteLine("Sent image");
        }


    }
}
