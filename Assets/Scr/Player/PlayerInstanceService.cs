using Scr.Player.Action;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Scr.Player {

    public interface IPlayerInstanceService {

        event System.Action<Player> OnPlayerSpawned;
        
        bool Instanced { get; }
        
        Player InstancedPlayer { get; }
    }
    
    [DefaultExecutionOrder(-1000)]
    public class PlayerInstanceService : SerializedMonoBehaviour, IPlayerInstanceService {

        [SerializeField]
        [Required("Playerのプレハブを設定してください")]
        private Player _playerPrefab;
        
        [SerializeField]
        [Required("Playerのインスタンス生成位置を設定してください")]
        private Transform _instancePosition;
        
        [SerializeField]
        [ReadOnly]
        private bool _instanced = false;
        
        private Player _instancedPlayer;
        
        public event System.Action<Player> OnPlayerSpawned = delegate { };
        
        public Player InstancedPlayer => _instancedPlayer;
        
        public bool Instanced => _instanced;

        private IObjectResolver _resolver;

        [Inject]
        public void Construct(IObjectResolver resolver) {
            _resolver = resolver;
            Debug.Log($"[PlayerInstanceService] Construct呼び出し: _resolver is null = {_resolver == null}");
            Debug.Log($"[PlayerInstanceService] resolver.ApplicationOrigin: {resolver?.ApplicationOrigin}");
        }

        private void Start() {
            Debug.Log($"[PlayerInstanceService] Start開始");
            
            // resolverのチェック
            if (_resolver is null) {
                Debug.LogError($"[PlayerInstanceService] IObjectResolverが注入されていません。VContainerのセットアップを確認してください。", this);
                return;
            }
            
            // プレハブのチェック
            if (_playerPrefab == null) {
                Debug.LogError($"[PlayerInstanceService] PlayerPrefabが設定されていません。Inspectorで設定してください。", this);
                return;
            }
            
            // インスタンス位置のチェック
            if (_instancePosition == null) {
                Debug.LogError($"[PlayerInstanceService] InstancePositionが設定されていません。Inspectorで設定してください。", this);
                return;
            }
            
            try {
                Debug.Log($"[PlayerInstanceService] Playerプレハブの生成開始: {_playerPrefab.name}");
                Debug.Log($"[PlayerInstanceService] 生成位置: {_instancePosition.position}");
                Debug.Log($"[PlayerInstanceService] プレハブのアクティブ状態: {_playerPrefab.gameObject.activeSelf}");
                
                // プレハブを一時的に非アクティブにして、Startが呼ばれないようにする
                var wasActive = _playerPrefab.gameObject.activeSelf;
                _playerPrefab.gameObject.SetActive(false);
                
                // VContainerのInstantiateではなく、通常のInstantiateを使用してから手動で依存性注入
                // これにより、PlayerのLifetimeScopeが正しく初期化される
                var instance = UnityEngine.Object.Instantiate(
                    _playerPrefab, 
                    _instancePosition.position, 
                    Quaternion.identity
                );
                
                Debug.Log($"[PlayerInstanceService] Playerインスタンス生成完了: {instance.name}");
                Debug.Log($"[PlayerInstanceService] インスタンスのアクティブ状態: {instance.gameObject.activeSelf}");
                
                // PlayerのLifetimeScopeを確認
                var playerLifetimeScope = instance.GetComponent<VContainer.Unity.LifetimeScope>();
                Debug.Log($"[PlayerInstanceService] PlayerのLifetimeScope: {playerLifetimeScope != null}");
                
                // 生成されたPlayerに対して、親のResolverから依存性注入
                Debug.Log($"[PlayerInstanceService] 依存性注入開始");
                _resolver.InjectGameObject(instance.gameObject);
                Debug.Log($"[PlayerInstanceService] 依存性注入完了");
                
                // プレハブを元の状態に戻す
                _playerPrefab.gameObject.SetActive(wasActive);
                
                // インスタンスをアクティブ化（これでStartが呼ばれる）
                instance.gameObject.SetActive(true);
                Debug.Log($"[PlayerInstanceService] インスタンスをアクティブ化");
                
                _instancedPlayer = instance;
                _instanced = true;
                
                // PlayerActionBehaviourコンポーネントの確認
                var actionBehaviours = instance.GetComponentsInChildren<PlayerActionBehaviour>();
                Debug.Log($"[PlayerInstanceService] PlayerActionBehaviourの数: {actionBehaviours.Length}");
                foreach (var behaviour in actionBehaviours) {
                    Debug.Log($"[PlayerInstanceService] - {behaviour.GetType().Name} (enabled: {behaviour.enabled})");
                }
                
                OnPlayerSpawned.Invoke(_instancedPlayer);
                Debug.Log($"[PlayerInstanceService] Playerを生成しました: {_instancedPlayer.name}");
                
                // 生成後に少し待ってから状態確認
                DelayedStatusCheck().Forget();
            }
            catch (System.Exception ex) {
                Debug.LogError($"[PlayerInstanceService] Playerの生成中にエラーが発生しました: {ex.Message}", this);
                Debug.LogException(ex, this);
            }
        }
        
        private async Cysharp.Threading.Tasks.UniTaskVoid DelayedStatusCheck() {
            await Cysharp.Threading.Tasks.UniTask.Delay(System.TimeSpan.FromSeconds(0.5f));
            
            Debug.Log("========== Player生成後の状態確認 ==========");
            Debug.Log($"Player active: {_instancedPlayer.gameObject.activeSelf}");
            
            var moveAction = _instancedPlayer.GetComponentInChildren<Scr.Player.Action.MoveAction>();
            if (moveAction != null) {
                Debug.Log($"MoveAction found: enabled={moveAction.enabled}");
                
                // リフレクションで_inputフィールドを確認
                var inputField = moveAction.GetType().GetField("_input", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (inputField != null) {
                    var inputValue = inputField.GetValue(moveAction);
                    Debug.Log($"MoveAction._input is null: {inputValue == null}");
                }
                
                // _playableフィールドを確認
                var playableField = moveAction.GetType().BaseType?.GetField("_playable", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (playableField != null) {
                    var playableValue = playableField.GetValue(moveAction);
                    Debug.Log($"MoveAction._playable is null: {playableValue == null}");
                }
            }
            else {
                Debug.LogError("MoveAction not found!");
            }
            
            Debug.Log("==========================================");
        }
    }
}