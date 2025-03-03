using Multiplayer;
using UnityEngine;
using UnityEngine.Events;

namespace Runtime.Gameplay {
    public class Reflection : MonoBehaviour {
        [SerializeField] Transform visualsRoot;

        Transform mTransform;
        Vector3 lastPosition;

        public UnityAction OnPlayerCollision;

        void Awake() {
            mTransform = GetComponent<Transform>();
            lastPosition = mTransform.position;
        }

        void OnTriggerEnter(Collider other) {
            var player = other.GetComponent<Player>();
            if (!player)
                return;

            OnPlayerCollision?.Invoke();
        }

        public void ApplyPositionMessage(PositionUpdateMessage message) {
            mTransform.position = new Vector3(message.PositionX, 0, message.PositionY);
            mTransform.rotation = Quaternion.Euler(0, message.Rotation, 0);
            var motion = mTransform.position - lastPosition;

            if (!Mathf.Approximately(motion.magnitude, 0f))
                visualsRoot.LookAt(mTransform.position + motion);
            else
                visualsRoot.LookAt(mTransform.position + mTransform.forward);

            lastPosition = mTransform.position;
        }
    }
}
