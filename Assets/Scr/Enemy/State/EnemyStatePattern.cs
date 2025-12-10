using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Scr.Enemy.State {
    
    [Serializable]
    public class EnemyStatePatternEntry {

        [OdinSerialize]
        [LabelText("ステートID（一意）")]
        public string StateId;

        [OdinSerialize]
        public IEnemyBehaviourState State;
        
        [OdinSerialize]
        public List<IStateTransitionTrigger> TransitionTriggers;
        
    }
    
    [CreateAssetMenu(menuName = "Project/Enemy/StatePattern")]
    public class EnemyStatePattern : SerializedScriptableObject {

        [OdinSerialize]
        [ListDrawerSettings(Expanded = true)]
        [LabelText("ステートパターンエントリ一覧")]
        public List<EnemyStatePatternEntry> StatePatternEntries = new List<EnemyStatePatternEntry>();
        
    }
}