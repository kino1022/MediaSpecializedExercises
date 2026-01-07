using System.Linq;
using MessagePipe;
using R3;
using Scr.Audio;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using VContainer;

namespace Scr.Player.Action {

    public interface IPlayerAction {
        
        ReadOnlyReactiveProperty<bool> OnAction { get; }
        
        
    }
    
    /// <summary>
    /// 全てのプレイヤーアクションの基底クラス
    /// </summary>
    public abstract class PlayerActionBehaviour : SerializedMonoBehaviour {

        protected ReactiveProperty<bool> _onAction = new(false);
        
        [Title("参照")]
        
        [SerializeField]
        [LabelText("アニメーター")]
        [ReadOnly]
        protected Animator _animator;

        [SerializeField]
        [ReadOnly]
        protected Rigidbody _rigidbody;

        [OdinSerialize]
        [LabelText("操作可能フラグ")]
        [ReadOnly]
        protected IPlayableProvider _playable;
        
        protected IPublisher<AudioPlayEventBus> _audioPublisher;

        protected IObjectResolver _resolver;
        
        public ReadOnlyReactiveProperty<bool> OnAction => _onAction;

        [Inject]
        public void Construct(IObjectResolver resolver) {
            _resolver = resolver;
            Debug.Log($"[{GetType().Name}] Construct呼び出し: _resolver is null = {_resolver == null}");
        }

        private void Start() {
            Debug.Log($"[{GetType().Name}] Start開始");
            Debug.Log($"[{GetType().Name}] _resolver is null: {_resolver == null}");
            
            OnPreStart();

            // PlayerのLifetimeScopeから取得（ローカルなコンポーネント）
            var playerScope = gameObject.GetComponentInParent<VContainer.Unity.LifetimeScope>();
            Debug.Log($"[{GetType().Name}] PlayerのLifetimeScope取得: {playerScope != null}");
            
            if (playerScope != null) {
                _animator = playerScope.Container.Resolve<Animator>();
                Debug.Log($"[{GetType().Name}] Animator resolved from PlayerScope: {_animator != null}");
                
                _rigidbody = playerScope.Container.Resolve<Rigidbody>();
                Debug.Log($"[{GetType().Name}] Rigidbody resolved from PlayerScope: {_rigidbody != null}");
            }
            else {
                Debug.LogError($"[{GetType().Name}] PlayerのLifetimeScopeが見つかりません");
            }
            
            // 親のGameManagerLifetimeScopeから取得（グローバルなサービス）
            if (_resolver != null) {
                _playable = _resolver.Resolve<IPlayableProvider>();
                Debug.Log($"[{GetType().Name}] IPlayableProvider resolved: {_playable != null}, Playable: {_playable?.Playable}");
                
                _audioPublisher = _resolver.Resolve<IPublisher<AudioPlayEventBus>>();
                Debug.Log($"[{GetType().Name}] AudioPublisher resolved: {_audioPublisher != null}");
                
                // IInputModuleの初期化（重要！）
                InitializeInputModules();
            }
            else {
                Debug.LogError($"[{GetType().Name}] _resolverがnullのため、グローバルサービスを取得できません");
            }

            Debug.Log($"[{GetType().Name}] OnPostStart呼び出し前");
            OnPostStart();
            Debug.Log($"[{GetType().Name}] Start完了");
        }
        
        /// <summary>
        /// このクラスに定義されているIInputModuleフィールドを検出して初期化
        /// </summary>
        private void InitializeInputModules() {
            Debug.Log($"[{GetType().Name}] InitializeInputModules開始");
            
            // IInputStreamProviderを取得
            RinaInput.Provider.IInputStreamProvider provider = null;
            try {
                provider = _resolver.Resolve<RinaInput.Provider.IInputStreamProvider>();
                Debug.Log($"[{GetType().Name}] IInputStreamProvider取得成功");
            }
            catch (System.Exception ex) {
                Debug.LogWarning($"[{GetType().Name}] IInputStreamProviderの取得に失敗: {ex.Message}");
                Debug.LogWarning($"[{GetType().Name}] ControllerMonoBehaviourで初期化される可能性があります");
                return;
            }
            
            try {
                // このクラスの全フィールドを取得
                var fields = GetType().GetFields(
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Public
                );
                
                Debug.Log($"[{GetType().Name}] フィールド数: {fields.Length}");
                
                int moduleCount = 0;
                foreach (var field in fields) {
                    try {
                        // IInputModuleインターフェースを実装しているか確認
                        var fieldType = field.FieldType;
                        var interfaces = fieldType.GetInterfaces();
                        
                        bool isInputModule = interfaces.Any(i => 
                            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(RinaInput.Controller.Module.IInputModule<>));
                        
                        if (isInputModule)
                        {
                            moduleCount++;
                            var inputModule = field.GetValue(this);
                            if (inputModule != null) {
                                Debug.Log($"[{GetType().Name}] IInputModule初期化: {field.Name}");
                                
                                // GenerateStreamを呼び出し
                                var generateMethod = fieldType.GetMethod("GenerateStream");
                                if (generateMethod != null) {
                                    try {
                                        generateMethod.Invoke(inputModule, new object[] { provider });
                                        Debug.Log($"[{GetType().Name}] GenerateStream呼び出し完了: {field.Name}");
                                    }
                                    catch (System.Exception ex) {
                                        Debug.LogError($"[{GetType().Name}] GenerateStream呼び出し失敗 {field.Name}: {ex.Message}");
                                    }
                                }
                                
                                // Startを呼び出し（InputAction.Enable()を実行）
                                var startMethod = fieldType.GetMethod("Start");
                                if (startMethod != null) {
                                    try {
                                        startMethod.Invoke(inputModule, null);
                                        Debug.Log($"[{GetType().Name}] Start呼び出し完了（InputAction有効化）: {field.Name}");
                                    }
                                    catch (System.Exception ex) {
                                        Debug.LogError($"[{GetType().Name}] Start呼び出し失敗 {field.Name}: {ex.Message}");
                                    }
                                }
                            }
                            else {
                                Debug.LogWarning($"[{GetType().Name}] IInputModuleフィールド {field.Name} がnullです");
                            }
                        }
                    }
                    catch (System.Exception ex) {
                        Debug.LogError($"[{GetType().Name}] フィールド {field.Name} の処理中にエラー: {ex.Message}");
                    }
                }
                
                Debug.Log($"[{GetType().Name}] IInputModuleフィールド数: {moduleCount}");
            }
            catch (System.Exception ex) {
                Debug.LogError($"[{GetType().Name}] InitializeInputModules中に予期しないエラー: {ex.Message}");
                Debug.LogException(ex);
            }
            
            Debug.Log($"[{GetType().Name}] InitializeInputModules完了");
        }
        
        protected virtual void OnPreStart() {}
        
        protected virtual void OnPostStart() {}
    }
}