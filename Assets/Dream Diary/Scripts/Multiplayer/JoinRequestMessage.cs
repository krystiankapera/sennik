using System;

namespace Multiplayer {
    [Serializable]
    public class JoinRequestMessage : NetworkMessage {
        public override MessageType Type => MessageType.JoinRequest;
    }
}
