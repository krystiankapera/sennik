using UnityEngine;
using Runtime.Gameplay;

namespace Runtime.UI {
    public class ReplayButton : UIButton {
        [SerializeField] Main main;

        protected override void HandleClick() {
            main.Restart();
        }
    }
}
