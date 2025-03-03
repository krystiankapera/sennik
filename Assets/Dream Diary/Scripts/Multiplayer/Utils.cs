using System;
using System.Buffers;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Multiplayer {
    public static class Utils {
        public static async UniTask ReadStream(Stream stream, NetworkPeer peer, CancellationToken cancellationToken) {
            if (stream.CanRead)
                await Read();
            else
                await UniTask.Yield();
            return;

            async UniTask Read() {
                var buffer = ArrayPool<byte>.Shared.Rent(1024);

                var byteCount = await stream.ReadAsync(buffer, cancellationToken);
                var output = new byte[byteCount];
                Array.Copy(sourceArray: buffer, destinationArray: output, length: byteCount);
                peer.PassReceivedData(output);

                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public static byte[] Serialize(NetworkMessage message) {
            using (MemoryStream stream = new MemoryStream()) {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, message);
                return stream.ToArray();
            }
        }

        public static NetworkMessage Deserialize(byte[] data) {
            if (data == null || data.Length == 0)
                return null;

            using (MemoryStream stream = new MemoryStream(data)) {
                BinaryFormatter formatter = new BinaryFormatter();
                return (NetworkMessage)formatter.Deserialize(stream);
            }
        }
    }
}