using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Gameplay {
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimatorController : MonoBehaviour {
        const string VELOCITY_KEY = "velocity";

        Transform mTransform;
        Animator animator;
        Vector3 lastPosition;
        List<VelocitySample> velocitySamples = new List<VelocitySample>();

        void Awake() {
            animator = GetComponent<Animator>();
            mTransform = transform;
            lastPosition = mTransform.position;
        }

        void Update() {
            var delta = (mTransform.position - lastPosition).magnitude;
            var velocity = delta / Time.deltaTime;
            velocitySamples.Add(new VelocitySample { Time = Time.time, Velocity = velocity });

            for (int i = velocitySamples.Count - 1; i >= 0; i--)
                if (Time.time > velocitySamples[i].Time + 0.15f)
                    velocitySamples.RemoveAt(i);

            var avgVelocity = 0f;

            foreach (var sample in velocitySamples)
                avgVelocity += sample.Velocity / velocitySamples.Count;

            animator.SetFloat(VELOCITY_KEY, avgVelocity);
            lastPosition = mTransform.position;
        }

        private struct VelocitySample {
            public float Velocity;
            public float Time;
        }
    }
}
