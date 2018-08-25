using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using SC2APIProtocol;

namespace Bot {
    public class ProtobufProxy {
        private ClientWebSocket clientSocket;
        private readonly CancellationToken token = new CancellationTokenSource().Token;

        public async Task Connect(string address, int port) {
            clientSocket = new ClientWebSocket();
            var adr = string.Format("ws://{0}:{1}/sc2api", address, port);
            var uri = new Uri(adr);
            await clientSocket.ConnectAsync(uri, token);

            await Ping();
        }

        public async Task Ping() {
            var request = new Request();
            request.Ping = new RequestPing();
            var response = await SendRequest(request);
        }

        public async Task<Response> SendRequest(Request request) {
            await WriteMessage(request);
            return await ReadMessage();
        }

        public async Task Quit() {
            var quit = new Request();
            quit.Quit = new RequestQuit();
            await WriteMessage(quit);
        }

        private async Task WriteMessage(Request request) {
            var sendBuf = new byte[1024 * 1024];
            var outStream = new CodedOutputStream(sendBuf);
            request.WriteTo(outStream);
            await clientSocket.SendAsync(new ArraySegment<byte>(sendBuf, 0, (int) outStream.Position),
                WebSocketMessageType.Binary, true, token);
        }

        private async Task<Response> ReadMessage() {
            var receiveBuf = new byte[1024 * 1024];
            var finished = false;
            var curPos = 0;
            while (!finished) {
                var left = receiveBuf.Length - curPos;
                if (left < 0) {
                    // No space left in the array, enlarge the array by doubling its size.
                    var temp = new byte[receiveBuf.Length * 2];
                    Array.Copy(receiveBuf, temp, receiveBuf.Length);
                    receiveBuf = temp;
                    left = receiveBuf.Length - curPos;
                }

                var result = await clientSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuf, curPos, left), token);
                if (result.MessageType != WebSocketMessageType.Binary)
                    throw new Exception("Expected Binary message type.");

                curPos += result.Count;
                finished = result.EndOfMessage;
            }

            var response = Response.Parser.ParseFrom(new MemoryStream(receiveBuf, 0, curPos));

            return response;
        }
    }
}