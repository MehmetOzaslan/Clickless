using System;
using System.Drawing;
using System.IO;
using System.Net.WebSockets;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Clickless.src;
using Clickless.src.util.test;
using NUnit.Framework;
using static System.Net.Mime.MediaTypeNames;
using static Clickless.src.WindowInfoRetriever;
using Image = System.Drawing.Image;

namespace NUnit.Tests
{
    [TestFixture]
    public class TestMLWebsocketClient
    {
        private const string echoTextURL = "ws://localhost:8210/echotext";
        private const string echoBytesURL = "ws://localhost:8210/echobytes";
        private const string imageSaveURL = "ws://localhost:8210/imagesave";



        MLClientWebsocket websocket;

        [SetUp]
        public void Init()
        {
            websocket = new MLClientWebsocket();
        }

        /// <summary>
        /// Python server must be running.
        /// </summary>
        [Test]
        public async Task TestConnection()
        {

            await websocket.ConnectAsync(new Uri(echoTextURL));
            Assert.AreEqual(WebSocketState.Open, websocket.Client.State);

            await websocket.DisconnectAsync();
            Assert.AreEqual(WebSocketState.Closed, websocket.Client.State);
        }



        [Test]
        public async Task TestSend()
        {
            await websocket.ConnectAsync(new Uri(echoTextURL));
            Assert.AreEqual(WebSocketState.Open, websocket.Client.State);

            await websocket.SendMessageAsync("Hello!");
            await websocket.Abort();

            Assert.AreEqual(WebSocketState.Aborted, websocket.Client.State);
        }


        [Test]
        public async Task TestRecieve()
        {
            string sentData = "Hello!";
            await websocket.ConnectAsync(new Uri(echoTextURL));
            Assert.AreEqual(WebSocketState.Open, websocket.Client.State);

            await websocket.SendMessageAsync(sentData);

            var echoData = await websocket.ReceiveMessageAsync();

            Console.WriteLine(echoData);

            await Task.Delay(100);


            await websocket.DisconnectAsync();
            Assert.AreEqual(WebSocketState.Closed, websocket.Client.State);
            Assert.AreEqual(sentData, echoData);
        }

        [Test]
        public async Task TestRecieveLargeData()
    {
            await websocket.ConnectAsync(new Uri(echoTextURL));
            Assert.AreEqual(WebSocketState.Open, websocket.Client.State);

            StringBuilder sendData = new StringBuilder();

            for (int i = 0; i < 1024; i++)
            {
                sendData.Append(i.ToString() + "|");
            }

            await websocket.SendMessageAsync(sendData.ToString());

            var returnData = await websocket.ReceiveMessageAsync();

            Console.WriteLine("Rcvd: " + returnData);

            await Task.Delay(100);


            await websocket.DisconnectAsync();
            Assert.AreEqual(WebSocketState.Closed, websocket.Client.State);
            Assert.AreEqual(sendData.ToString(), returnData);
        }

        [Test]
        public async Task TestImageSend()
        {
            await websocket.ConnectAsync(new Uri(echoBytesURL));
            Assert.AreEqual(WebSocketState.Open, websocket.Client.State);

            Image img = ScreenController.CaptureDesktop();

            await websocket.SendImageAsync(img);

            await Task.Delay(100);


            await websocket.Abort();
            Assert.AreEqual(WebSocketState.Aborted, websocket.Client.State);
        }


        [Test]
        public async Task TestImageSave()
        {
            await websocket.ConnectAsync(new Uri(imageSaveURL));
            Assert.AreEqual(WebSocketState.Open, websocket.Client.State);

            Image img = ScreenController.CaptureDesktop();

            await websocket.SendImageAsync(img);

            await Task.Delay(100);

            await websocket.Abort();
            Assert.AreEqual(WebSocketState.Aborted, websocket.Client.State);
        }




        [Test]
        public async Task TestImageEcho()
        {
            await websocket.ConnectAsync(new Uri(echoBytesURL));
            Assert.AreEqual(WebSocketState.Open, websocket.Client.State);

            Image img = ScreenController.CaptureDesktop();

            //TODO: Change this
            byte[] imageBytes;
            using (var ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                imageBytes = ms.ToArray();
            }


            await websocket.SendImageAsync(img);

            var returnData = await websocket.ReceiveByteMessageAsync();

            Console.WriteLine("Rcvd: " + returnData.ToString());

            Assert.AreEqual(imageBytes, returnData);

            await Task.Delay(100);
            await websocket.Abort();
            Assert.AreEqual(WebSocketState.Closed, websocket.Client.State);

        }
    }



}

