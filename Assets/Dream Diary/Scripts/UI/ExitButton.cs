using UnityEngine;
using Runtime.Gameplay;

namespace Runtime.UI {
    public class ExitButton : UIButton {
        [SerializeField] Main main;

        protected override void HandleClick() {
            main.Quit();
        }
    }
}
