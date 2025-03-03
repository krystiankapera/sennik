using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Runtime.Ads {
    public class AdController : MonoBehaviour {
        [SerializeField] GameObject adPanel;
        [SerializeField] float duration;

        CancellationTokenSource cts;
        readonly List<IShowAdCondition> conditions = new();

        void Start() {
            CloseAd();
        }

        void OnDestroy() {
            if (cts != null)
                cts.Cancel();

            foreach (var condition in conditions)
                condition.OnConditionMet -= ShowAd;
        }

        void ShowAd() {
            adPanel.SetActive(true);
            CloseAdAfterDelay();
        }

        async void CloseAdAfterDelay() {
            cts = new CancellationTokenSource(); 
            await UniTask.Delay((int)(duration * 1000), cancellationToken: cts.Token).SuppressCancellationThrow();

            if (cts.IsCancellationRequested)
                return;

            CloseAd();
        }

        void CloseAd() {
            adPanel.SetActive(false);
        }

        public void AddShowCondition(IShowAdCondition condition) {
            conditions.Add(condition);
            condition.OnConditionMet += ShowAd;
        }
    }
}
