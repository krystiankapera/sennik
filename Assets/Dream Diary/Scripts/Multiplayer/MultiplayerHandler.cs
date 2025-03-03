using Cysharp.Threading.Tasks;
using System.Threading;
using Runtime.Gameplay;

namespace Multiplayer {
    public abstract class MultiplayerHandler {
        protected readonly Player player;
        protected readonly CancellationToken cancellationToken;
        protected readonly Peer peer;

        public MultiplayerHandler(Player player, CancellationToken cancellationToken) {
            this.player = player;
            this.cancellationToken = cancellationToken;
            peer = new Peer();
        }

        public abstract void Initialize();

        protected async UniTask StartSendingPosition() {
            while (!cancellationToken.IsCancellationRequested) {
                await UniTask.Delay(25, cancellationToken: cancellationToken);
                peer.SendData(Utils.Serialize(new PositionUpdateMessage(player.Position, player.transform.eulerAngles.y)));
            }
        }
    }
}
