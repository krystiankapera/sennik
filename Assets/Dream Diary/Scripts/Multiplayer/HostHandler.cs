using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Multiplayer;
using Runtime.Gameplay;
using System.Threading;

namespace Multiplayer {
    public class HostHandler : MultiplayerHandler {
        readonly Reflection reflection;
        Host host;

        public HostHandler(Player player, Reflection reflection, CancellationToken cancellationToken) : base (player, cancellationToken) {
            this.reflection = reflection;
        }

        public override void Initialize() {
            host = new Host(port: NetworkSettings.Port);
            peer.OnDataReceived += HandleDataReceived;
            host.Run(peer, cancellationToken).Forget();
        }

        void HandleDataReceived(byte[] data) {
            NetworkMessage message = Utils.Deserialize(data);

            if (message == null)
                return;

            switch (message.Type) {
                case MessageType.JoinRequest:
                    var initMessage = new InitMessage(player.Position, reflection.transform.position);
                    peer.SendData(Utils.Serialize(initMessage));
                    StartSendingPosition().Forget();
                    break;

                case MessageType.PositionUpdate:
                    var positionUpdateMessage = message as PositionUpdateMessage;
                    reflection.ApplyPositionMessage(positionUpdateMessage);
                    break;
            }
        }
    }
}
