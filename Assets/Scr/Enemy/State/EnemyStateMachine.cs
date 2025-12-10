using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using VContainer;

namespace Scr.Enemy.State {
    public class EnemyStateMachine : SerializedMonoBehaviour {
        
        [SerializeField]
        [LabelText("実行する行動パターン")]
        private EnemyStatePattern _statePattern;
        
        [OdinSerialize]
        [LabelText("現在の行動パターン")]
        private IEnemyBehaviourState _currentState;
        
        [OdinSerialize]
        [LabelText("現在の遷移トリガー")]
        private List<IStateTransitionTrigger> _currentTriggers = new();
        
        private Dictionary<string, EnemyStatePatternEntry> _stateEntryMap = new();
        
        private IObjectResolver _resolver;

        [Inject]
        public void Construct(IObjectResolver resolver) {
            _resolver = resolver;
        }
        
        private void Start() {
            BuildStateEntryMap();
            InitializeStates();
            InitializeTriggers();

            var firstState = _statePattern.StatePatternEntries.FirstOrDefault();
            SetNewStateEntry(firstState);
        }

        private void FixedUpdate() {
            _currentState?.FixedUpdate(Time.fixedDeltaTime);
            if (CheckStateTransition()) {
                var nextEntry = GetStateEntryById(_currentState?.NextStateId);
                SetNewStateEntry(nextEntry);
            }
        }

        private void Update() {
            _currentState?.Update(Time.fixedDeltaTime);
            if (CheckStateTransition()) {
                var nextEntry = GetStateEntryById(_currentState?.NextStateId);
                SetNewStateEntry(nextEntry);
            }
            var triggerResult = CheckTransitionTriggers();
            if (triggerResult.Triggered && !string.IsNullOrEmpty(triggerResult.NextStateId)) {
                var nextEntry = GetStateEntryById(triggerResult.NextStateId);
                SetNewStateEntry(nextEntry);
            }
        }
        
        private void SetNewStateEntry(EnemyStatePatternEntry nextEntry) {
            if (nextEntry is null) {
                return;
            }
            SetNewState(nextEntry.State);
            SetNewTriggers(nextEntry.TransitionTriggers);
        }

        private void SetNewState(IEnemyBehaviourState nextState) {
            if (nextState is null) {
                return;
            }
            if (_currentState is not null) {
                _currentState.Exit();
            }
            _currentState = nextState;
            nextState.Enter();
        }
        
        private void SetNewTriggers(List<IStateTransitionTrigger> nextTriggers) {
            if (nextTriggers.Count is 0) {
                return;
            }
            
            foreach (var previousTrigger in _currentTriggers) {
                previousTrigger?.Exit();
            }
            
            _currentTriggers = nextTriggers;

            foreach (var trigger in _currentTriggers) {
                trigger.Enter();
            }
        }

        private void InitializeStates() {
            
            if (_statePattern is null) {
                return;
            }
            
            var list = _statePattern.StatePatternEntries.Select(x => x.State).ToList();

            if (list.Count is 0) {
                return;
            }
            
            foreach (var state in list) {
                if (state is null) {
                    continue;
                }
                state.Initialize(_resolver, gameObject.transform.root.gameObject);
            }
        }

        private void InitializeTriggers() {
            
            if (_statePattern is null) {
                return;
            }
            
            var triggers = _statePattern.StatePatternEntries.SelectMany(x => x.TransitionTriggers).ToList();

            if (triggers.Count is 0) {
                return;
            }

            foreach (var trigger in triggers) {
                if (trigger is null) {
                    continue;
                }
                trigger.Initialize(_resolver, gameObject.transform.root.gameObject);
            }
        }

        private bool CheckStateTransition() {
            return _currentState.StateEnd;
        }
        
        private (bool Triggered, string NextStateId) CheckTransitionTriggers() {

            if (_currentTriggers.Count is 0) {
                return (false, null);
            }
            
            foreach (var trigger in _currentTriggers) {
                
                if (trigger is null) {
                    continue;
                }
                
                if (trigger.CheckTrigger()) {
                    return (true, trigger.NextStateId);
                }
            }
            return (false, null);
        }

        /// <summary>
        /// ステートIDからエントリを検索するための辞書を構築
        /// </summary>
        private void BuildStateEntryMap() {
            _stateEntryMap.Clear();
            
            if (_statePattern is null || _statePattern.StatePatternEntries is null) {
                return;
            }
            
            foreach (var entry in _statePattern.StatePatternEntries) {
                if (entry is null || string.IsNullOrEmpty(entry.StateId)) {
                    continue;
                }
                
                if (_stateEntryMap.ContainsKey(entry.StateId)) {
                    Debug.LogWarning($"重複したステートID: {entry.StateId}");
                    continue;
                }
                
                _stateEntryMap[entry.StateId] = entry;
            }
        }

        /// <summary>
        /// ステートIDからエントリを取得
        /// </summary>
        /// <param name="stateId">検索するステートID</param>
        /// <returns>見つかったエントリ、または null</returns>
        private EnemyStatePatternEntry GetStateEntryById(string stateId) {
            if (string.IsNullOrEmpty(stateId)) {
                return null;
            }
            
            if (_stateEntryMap.TryGetValue(stateId, out var entry)) {
                return entry;
            }
            
            Debug.LogWarning($"ステートID '{stateId}' が見つかりません");
            return null;
        }
    }
}