using Runtime.Gameplay;
using UnityEngine;
using UnityEngine.Events;

namespace Runtime.Ads {
    public class PlayerDistanceTravelledShowAdCondition : IShowAdCondition {
        public event UnityAction OnConditionMet;

        Player player;
        float targetDistance;
        float distanceTravelled;

        public PlayerDistanceTravelledShowAdCondition(Player player, float distance) {
            this.player = player;
            this.targetDistance = distance;
            player.OnMove += HandlePlayerMove;
        }

        void HandlePlayerMove(Vector3 from, Vector3 to) {
            distanceTravelled += Vector3.Distance(from, to);

            if (distanceTravelled >= targetDistance) {
                player.OnMove -= HandlePlayerMove;
                OnConditionMet?.Invoke();
            }
        }
    }
}
