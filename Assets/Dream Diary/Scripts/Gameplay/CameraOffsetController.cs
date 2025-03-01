using UnityEngine;

namespace Runtime.Gameplay {
    public class CameraOffsetController : MonoBehaviour {
        [SerializeField] Player player;
        [SerializeField] float minDistance = 0.3f;
        [SerializeField] float smoothSpeed = 5.0f;
        [SerializeField] LayerMask obstacleLayerMask;

        Transform mTransform;
        Vector3 cameraOffset;
        float heightOffset;
        float defaultDistance;

        void Awake() {
            mTransform = transform;
            heightOffset = mTransform.localPosition.y;
            defaultDistance = -mTransform.localPosition.z;
            cameraOffset = new Vector3(0, heightOffset, -defaultDistance);
        }

        void LateUpdate() {
            if (!player)
                return;

            var desiredPosition = player.Position + player.Rotation * cameraOffset;
            var adjustedPosition = desiredPosition;
            var castOrigin = player.Position + Vector3.up * heightOffset;
            if (Physics.SphereCast(castOrigin, 0.2f, -player.Forward, out RaycastHit hit, defaultDistance, obstacleLayerMask, QueryTriggerInteraction.Ignore)) {
                float distance = Vector3.Distance(castOrigin, hit.point) - 0.15f;
                distance = Mathf.Clamp(distance, minDistance, defaultDistance);
                adjustedPosition = player.Position + player.Rotation * new Vector3(0, heightOffset, -distance);
            }

            mTransform.position = Vector3.Lerp(mTransform.position, adjustedPosition, Time.deltaTime * smoothSpeed);
            mTransform.LookAt(player.Position + Vector3.up * heightOffset);
        }
    }
}
