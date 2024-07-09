using System;
using System.Drawing;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Clickless.src
{
    internal class MLClientWebsocket
    {
        private readonly ClientWebSocket _client;

        public MLClientWebsocket()
        {
            _client = new ClientWebSocket();
        }


        private async Task ReceiveMessages()
        {
            //Create a buffer and read the following amount per chunk.
            var buffer = new byte[1024 * 4];
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
            var bufferSize = 1024;
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



        public async Task DisconnectAsync()
        {
            await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", CancellationToken.None);
            _client.Dispose();
            Console.WriteLine("Disconnected!");
        }

    }
}
