using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using VContainer;

namespace Scr.Enemy.State {

    public interface IEnemyBehaviourState {
        
        /// <summary>
        /// ステートが正常終了した際の次のステートID
        /// </summary>
        string NextStateId { get; }
        
        /// <summary>
        /// ステートが終了しているかどうかのフラグ
        /// </summary>
        bool StateEnd { get; }

        /// <summary>
        /// ステートの初期化処理
        /// </summary>
        /// <param name="resolver"></param>
        /// <param name="enemy"></param>
        void Initialize(IObjectResolver resolver, GameObject enemy);

        void Enter();
        
        void FixedUpdate(float deltaTime);
        
        void Update(float deltaTime);
        
        void Exit();
    }
    
    [Serializable]
    public abstract class AEnemyBehaviourState : IEnemyBehaviourState {

        [OdinSerialize]
        [LabelText("次のステートID")]
        protected string nextStateId;
        
        [OdinSerialize]
        [LabelText("ステート終了フラグ")]
        [ReadOnly]
        protected bool stateEnd = false;
        
        protected IObjectResolver _resolver;
        
        protected GameObject _enemy;
        
        public string NextStateId => nextStateId;
        
        public bool StateEnd => stateEnd;
        
        public virtual void Initialize(IObjectResolver resolver, GameObject enemy) {
            _resolver = resolver;
            _enemy = enemy;
        }
        
        public virtual void Enter() {}

        public virtual void FixedUpdate(float deltaTime) {
            //ステートが終了していたら処理を抜ける
            if (stateEnd) {
                return;
            }
        }

        public virtual void Update(float deltaTime) {
            //ステートが終了していたら処理を抜ける
            if (stateEnd) {
                return;
            }
        }
        
        public virtual void Exit() {}
    }
}