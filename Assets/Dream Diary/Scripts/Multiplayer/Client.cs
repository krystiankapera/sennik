using System.Net.Sockets;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;

namespace Multiplayer {
    public class Client {
        readonly string ip;
        readonly int port;

        public UnityAction OnConnected;

        public Client(string ip, int port) {
            this.ip = ip;
            this.port = port;
        }

        public async UniTask Run(NetworkPeer peer, CancellationToken cancellationToken) {
            using var client = new TcpClient();
            await client.ConnectAsync(ip, port);
            using var stream = client.GetStream();
            peer.OnDataToSend += SendData;
            OnConnected?.Invoke();

            while (client.Connected && !cancellationToken.IsCancellationRequested) {
                await Utils.ReadStream(stream, peer, cancellationToken);
            }
            peer.OnDataToSend -= SendData;
            return;

            void SendData(byte[] data) {
                stream.WriteAsync(data);
            }
        }
    }
}
