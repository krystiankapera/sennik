using UnityEngine;
using UnityEngine.Events;

namespace Runtime.Gameplay {
    public class Player : MonoBehaviour {
        [SerializeField] CharacterController characterController;
        [SerializeField] Transform visualsRoot;

        Transform mTransform;
        bool stationary;

        public UnityAction<Vector3, Vector3> OnMove;
        public Vector3 Position => mTransform.position;
        public Vector3 Forward => mTransform.forward;
        public Quaternion Rotation => mTransform.rotation;

        void Awake() {
            mTransform = transform;
        }

        void LateUpdate() {
            if (stationary)
                visualsRoot.LookAt(mTransform.position + mTransform.forward);
            else
                stationary = true;
        }

        public void Move(float diff, Vector2 moveDirection) {
            var startingPosition = mTransform.position;
            var motion = (mTransform.forward * moveDirection.y + mTransform.right * moveDirection.x) * diff;
            characterController.Move(motion);
            visualsRoot.LookAt(mTransform.position + motion);
            stationary = false;
            OnMove?.Invoke(startingPosition, Position);
        }

        public void Rotate(float diff) {
            mTransform.Rotate(axis: transform.up, angle: diff);
        }

        public void Teleport(Vector3 position, Vector3 forward) {
            characterController.enabled = false;
            transform.position = position;
            transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            characterController.enabled = true;
        }
    }
}
