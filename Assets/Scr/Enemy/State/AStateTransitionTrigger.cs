using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using VContainer;

namespace Scr.Enemy.State {

    public interface IStateTransitionTrigger {
        
        /// <summary>
        /// この条件でステートが終了した際の次のステートID
        /// </summary>
        string NextStateId { get; }
        
        void Initialize(IObjectResolver resolver, GameObject enemy);

        void Enter();
        
        void Exit();
        
        /// <summary>
        /// 条件に合致しているかどうかを取得する
        /// </summary>
        /// <returns></returns>
        bool CheckTrigger();

    }
    
    public abstract class AStateTransitionTrigger : IStateTransitionTrigger {

        [OdinSerialize]
        [LabelText("次のステートID")]
        protected string nextStateId;
        
        protected IObjectResolver _resolver;
        
        protected GameObject _enemy;
        
        public string NextStateId => nextStateId;
        
        public virtual void Initialize(IObjectResolver resolver, GameObject enemy) {
            _resolver = resolver;
            _enemy = enemy;
        }
        
        public virtual void Enter() { }
        
        public virtual void Exit() { }
        
        public abstract bool CheckTrigger();
        
    }
}