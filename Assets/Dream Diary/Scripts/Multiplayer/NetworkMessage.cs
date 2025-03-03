using System;

namespace Multiplayer {
    [Serializable]
    public abstract class NetworkMessage {
        public abstract MessageType Type { get; }
    }

    public enum MessageType {
        JoinRequest = 0,
        Init = 1,
        PositionUpdate = 2
    }
}