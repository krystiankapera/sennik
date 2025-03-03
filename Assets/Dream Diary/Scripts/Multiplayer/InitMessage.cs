using System;
using UnityEngine;

namespace Multiplayer {
    [Serializable]
    public class InitMessage : NetworkMessage {
        public override MessageType Type => MessageType.Init;
        public float HostPositionX;
        public float HostPositionY;
        public float ClientPositionX;
        public float ClientPositionY;

        public InitMessage(Vector3 hostPosition, Vector3 clientPosition) {
            HostPositionX = hostPosition.x;
            HostPositionY = hostPosition.z;
            ClientPositionX = clientPosition.x;
            ClientPositionY = clientPosition.z;
        }
    }
}
