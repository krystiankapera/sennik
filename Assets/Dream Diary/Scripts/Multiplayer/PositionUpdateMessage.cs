using System;
using UnityEngine;

namespace Multiplayer {
    [Serializable]
    public class PositionUpdateMessage : NetworkMessage {
        public override MessageType Type => MessageType.PositionUpdate;
        public float PositionX;
        public float PositionY;
        public float Rotation;

        public PositionUpdateMessage(Vector3 position, float rotation) {
            PositionX = position.x;
            PositionY = position.z;
            Rotation = rotation;
        }
    }
}