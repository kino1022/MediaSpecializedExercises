using Cysharp.Threading.Tasks;
using R3;
using RinaInput.Controller.Module;
using RinaInput.Signal;
using Scr.Player;
using Scr.Stage;
using Scr.Utility;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Unity.Cinemachine;
using VContainer;

namespace Scr.Stage {
    public class WarpPipeBehaviourController : SerializedMonoBehaviour {

        [SerializeField]
        private Transform _pipeCenter;

        [SerializeField]
        private Transform _standPoint;

        [SerializeField]
        private WarpPipeBehaviourController _warpPoint;

        [SerializeField]
        private Collider2D _nextCameraArea;

        [OdinSerialize]
        private IInputModule<Vector2> _moveModule;

        [SerializeField]
        private float _animationDuration = 0.5f;

        [SerializeField]
        private float _collisionRadius = 1.0f;

        [SerializeField]
        private float _inputThreshold = 20.0f;

        [SerializeField]
        [ReadOnly]
        private bool _onPlayerStanding = false;

        [SerializeField]
        [ReadOnly]
        private Scr.Player.Player _cachedPlayerObj = null;

        [SerializeField]
        [ReadOnly]
        private IPlayableManager _playable;
        
        [SerializeField]
        [ReadOnly]
        private CinemachineConfiner2D _cameraConfiner = null;

        private IObjectResolver _resolver;

        [SerializeField]
        private Collider[] _overlappedColliders = new Collider[10];
        private Vector3 _cachedCenterDireciton = Vector3.zero;
        private bool _registeredInput = false;
        private bool _isWarping = false;

        [Inject]
        public void Construct(IObjectResolver resolver) {
            _resolver = resolver;
        }

        private void Start() {
            _cachedCenterDireciton = _pipeCenter.position - _standPoint.position;
            _cameraConfiner = FindAnyObjectByType<CinemachineConfiner2D>();
            _playable = _resolver.Resolve<IPlayableManager>();
            Debug.Log($"[WarpPipe] 初期化完了: {gameObject.name}, 中心方向: {_cachedCenterDireciton}");
            RegisterWarpInput();
        }

        private void Update() {
            GetPlayerStand();
        }

        private void RegisterWarpInput () {

            float GetAngle (Vector2 dir) {
                return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            }

            bool GetWarpable (InputSignal<Vector2> signal) {
                float inputAngle = GetAngle(signal.Value);
                float centerAngle = GetAngle(_cachedCenterDireciton);
                float angleDiff = Mathf.Abs(Mathf.DeltaAngle(inputAngle, centerAngle));
                // 入力方向がドカンの中心方向に近い場合にワープ可能
                return angleDiff <= _inputThreshold;
            }

            if (_moveModule is null) {
                Debug.LogWarning($"[WarpPipe] MoveModuleがnullです: {gameObject.name}");
                return;
            }
            _moveModule
                .Stream
                .Where(x => {
                    bool canWarp = _onPlayerStanding && GetWarpable(x) && !_isWarping;
                    if (_onPlayerStanding && !canWarp) {
                        Debug.Log($"[WarpPipe] ワープ条件未達: Warpable={GetWarpable(x)}, IsWarping={_isWarping}");
                    }
                    return canWarp;
                })
                .Subscribe(x => {
                    Debug.Log($"[WarpPipe] ワープ開始: {gameObject.name} -> {_warpPoint?.gameObject.name}");
                    _isWarping = true;
                    EnterSequenceAsync().Forget();
                })
                .AddTo(this);
            _registeredInput = true;
            Debug.Log($"[WarpPipe] 入力登録完了: {gameObject.name}");
        }

        private void GetPlayerStand () {
            _overlappedColliders = new Collider[10];
            Physics.OverlapSphereNonAlloc(_standPoint.position, _collisionRadius, _overlappedColliders);
            if (_overlappedColliders.Length <= 0) {
                return;
            }
            for (int i = 0; i < _overlappedColliders.Length; i++) {
                var hit = _overlappedColliders[i];
                if (hit is null) {
                    continue;
                }
                var player = hit.gameObject.GetComponentFromWhole<Player.Player>();
                if (player is not null) {
                    if (!_onPlayerStanding) {
                        Debug.Log($"[WarpPipe] プレイヤーが乗った: {gameObject.name}");
                    }
                    _onPlayerStanding = true;
                    _cachedPlayerObj = player;
                    return;
                }
            }
            if (_onPlayerStanding) {
                Debug.Log($"[WarpPipe] プレイヤーが離れた: {gameObject.name}");
            }
            _onPlayerStanding = false;
            _cachedPlayerObj = null;
        }

        private async UniTask EnterSequenceAsync () {
            Debug.Log($"[WarpPipe] 入場シーケンス開始: {gameObject.name}");
            
            if (_cachedPlayerObj is null || _warpPoint is null) {
                Debug.LogWarning($"[WarpPipe] 入場シーケンス中断: CachedPlayer={_cachedPlayerObj != null}, WarpPoint={_warpPoint != null}");
                _isWarping = false;
                return;
            }
            var enterPlayer = _cachedPlayerObj;
            
            var rigidBody = enterPlayer.gameObject.GetComponentFromWhole<Rigidbody>();
            if (rigidBody is not null) {
                Debug.Log($"[WarpPipe] Rigidbodyをキネマティックに設定");
                rigidBody.isKinematic = true;
                rigidBody.linearVelocity = Vector3.zero;
            }
            
            // アニメーターを取得してパラメータをリセット
            var animator = enterPlayer.gameObject.GetComponentFromWhole<Animator>();
            if (animator is not null) {
                Debug.Log($"[WarpPipe] アニメーターパラメータ設定: しゃがみ=true");
                animator.SetFloat("speed", 0f);
                animator.SetBool("jump", false);
                animator.SetBool("squat", true); // ドカンに入る時はしゃがみ状態
            }
            
            _playable ??= _resolver.Resolve<IPlayableManager>();
            _playable?.SetPlayable(false);
            Debug.Log($"[WarpPipe] プレイヤー操作を無効化");

            Debug.Log($"[WarpPipe] 入場アニメーション開始: {_standPoint.position} -> {_pipeCenter.position}");
            await MoveTransitionAsync(
                enterPlayer.transform,
                _standPoint.position,
                _pipeCenter.position,
                _animationDuration
                );
            Debug.Log($"[WarpPipe] 入場アニメーション完了");

            _cameraConfiner ??= FindAnyObjectByType<CinemachineConfiner2D>();
            if (_nextCameraArea is not null && _cameraConfiner is not null) {
                Debug.Log($"[WarpPipe] カメラ範囲変更: {_nextCameraArea.name}");
                _cameraConfiner.BoundingShape2D = _nextCameraArea;
            }
            
            Debug.Log($"[WarpPipe] 出口シーケンス呼び出し: {_warpPoint.gameObject.name}");
            await _warpPoint.ExitSequenceAsync(enterPlayer);
            
            Debug.Log($"[WarpPipe] ワープ完了: {gameObject.name}");
            _isWarping = false;
        }
        
        private async UniTask ExitSequenceAsync (Player.Player playerObj) {
            Debug.Log($"[WarpPipe] 出口シーケンス開始: {gameObject.name}");
            
            if (playerObj is null) {
                Debug.LogWarning($"[WarpPipe] 出口シーケンス中断: PlayerObj={playerObj != null}");
                _isWarping = false;
                return;
            }
            
            var rigidBody = playerObj.gameObject.GetComponentFromWhole<Rigidbody>();
            if (rigidBody is not null) {
                Debug.Log($"[WarpPipe] Rigidbodyをキネマティックに設定（出口）");
                rigidBody.isKinematic = true;
                rigidBody.linearVelocity = Vector3.zero;
            }
            
            // アニメーターをリセットしてEntryステートに戻す
            var animator = playerObj.gameObject.GetComponentFromWhole<Animator>();
            if (animator is not null) {
                Debug.Log($"[WarpPipe] アニメーターリセット: Rebind実行");
                // 全てのパラメータをリセット
                animator.SetBool("squat", false);
                animator.SetFloat("speed", 0f);
                animator.SetBool("jump", false);
                
                // アニメーターを完全にリセットしてEntryステートに戻す
                animator.Rebind();
                animator.Update(0f);
            }
            
            _playable ??= _resolver.Resolve<IPlayableManager>();
            _playable?.SetPlayable(false);
            Debug.Log($"[WarpPipe] プレイヤー操作を無効化（出口）");

            Debug.Log($"[WarpPipe] 出口アニメーション開始: {_pipeCenter.position} -> {_standPoint.position}");
            await MoveTransitionAsync(
                playerObj.transform,
                _pipeCenter.position,
                _standPoint.position,
                _animationDuration
                );
            Debug.Log($"[WarpPipe] 出口アニメーション完了");

            if (rigidBody is not null) {
                Debug.Log($"[WarpPipe] Rigidbodyを物理演算に戻す");
                rigidBody.isKinematic = false;
            }
            
            // 出口でプレイヤーを操作可能に戻す
            _playable?.SetPlayable(true);
            Debug.Log($"[WarpPipe] プレイヤー操作を有効化");
            
            Debug.Log($"[WarpPipe] 出口シーケンス完了: {gameObject.name}");
            _isWarping = false;
        }

        private async UniTask MoveTransitionAsync (Transform target, Vector3 from, Vector3 to, float duration) {
            //最初に強制的に移動させる
            Debug.Log($"[WarpPipe] 移動トランジション: {from} -> {to}, 時間={duration}秒");
            
            var rigidBody = target.gameObject.GetComponentFromWhole<Rigidbody>();
            var characterController = target.gameObject.GetComponentFromWhole<CharacterController>();
            
            // CharacterControllerを無効化
            if (characterController is not null) {
                Debug.Log($"[WarpPipe] CharacterControllerを無効化");
                characterController.enabled = false;
            }
            
            // 初期位置に設定
            if (rigidBody is not null) {
                rigidBody.position = from;
            } else {
                target.position = from;
            }
            
            float time = 0;
            while (time < duration) {
                time += Time.deltaTime;
                float t = time / duration;
                // 簡易イージング (SmoothStep)
                t = t * t * (3f - 2f * t);
                Vector3 newPosition = Vector3.Lerp(from, to, t);
                
                if (rigidBody is not null) {
                    rigidBody.position = newPosition;
                } else {
                    target.position = newPosition;
                }
                
                await UniTask.Yield(
                    PlayerLoopTiming.FixedUpdate,
                    this.GetCancellationTokenOnDestroy()
                    );
            }
            
            // 最終位置に確実に設定
            if (rigidBody is not null) {
                rigidBody.position = to;
            } else {
                target.position = to;
            }
            
            // CharacterControllerを再有効化
            if (characterController is not null) {
                Debug.Log($"[WarpPipe] CharacterControllerを再有効化");
                characterController.enabled = true;
            }
            
            Debug.Log($"[WarpPipe] 移動トランジション完了: 最終位置={to}");
        }
    }
}