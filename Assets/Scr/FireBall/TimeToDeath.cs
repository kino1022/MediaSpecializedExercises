using System;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using Observable = R3.Observable;

namespace Scr.FireBall {
    
    public class TimeToDeath : SerializedMonoBehaviour {

        [SerializeField]
        [LabelText("生存時間")]
        private float _lifetime = 10.0f;

        private void Start() {
            Observable
                .Timer(TimeSpan.FromSeconds(_lifetime))
                .Subscribe(_ => {
                    Destroy(gameObject);
                })
                .AddTo(this);
        }
    }
}