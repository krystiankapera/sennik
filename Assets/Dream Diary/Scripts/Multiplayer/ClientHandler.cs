using Cysharp.Threading.Tasks;
using Runtime.Gameplay;
using System.Threading;
using UnityEngine;

namespace Multiplayer {
    public class ClientHandler : MultiplayerHandler {
        readonly Reflection reflection;
        Client client;

        public ClientHandler(Player player, Reflection reflection, CancellationToken cancellationToken) : base (player, cancellationToken) {
            this.reflection = reflection;
        }

        public override void Initialize() {
            client = new Client(ip: NetworkSettings.IP, port: NetworkSettings.Port);
            peer.OnDataReceived += HandleDataReceived;
            client.OnConnected += SendJoinMessage;
            client.Run(peer, cancellationToken).Forget();
        }

        void HandleDataReceived(byte[] data) {
            NetworkMessage message = Utils.Deserialize(data);

            if (message == null)
                return;

            switch (message.Type) {
                case MessageType.Init:
                    var initMessage = message as InitMessage;
                    player.transform.position = new Vector3(initMessage.ClientPositionX, 0, initMessage.ClientPositionY);
                    reflection.transform.position = new Vector3(initMessage.HostPositionX, 0, initMessage.HostPositionY);
                    StartSendingPosition().Forget();
                    break;

                case MessageType.PositionUpdate:
                    var positionUpdateMessage = message as PositionUpdateMessage;
                    reflection.ApplyPositionMessage(positionUpdateMessage);
                    break;
            }
        }

        void SendJoinMessage() {
            peer.SendData(Utils.Serialize(new JoinRequestMessage()));
            client.OnConnected -= SendJoinMessage;
        }
    }
}
