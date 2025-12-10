using System;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Scr.Enemy.State.Asset {
    [Serializable]
    public class TimeTransitionTrigger : AStateTransitionTrigger {

        [SerializeField]
        private float _timeElapsed = 3.0f;
        
        [SerializeField]
        private bool _isElapsed = false;

        public override void Enter() {
            base.Enter();
            StartTimer().Forget();
        }

        public override void Exit() {
            base.Exit();
            _isElapsed = false;
        }
        
        private async UniTask StartTimer() {
            _isElapsed = false;
            await UniTask.Delay(TimeSpan.FromSeconds(_timeElapsed));
            OnTimerElapsed();
        }
        
        private void OnTimerElapsed() {
            _isElapsed = true;
        }

        public override bool CheckTrigger() => _isElapsed;
    }
}