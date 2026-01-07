using Cysharp.Threading.Tasks;
using R3;
using RinaInput.Controller.Module;
using RinaInput.Signal;
using Scr.Player;
using Scr.Stage;
using Scr.Utility;
using UnityEngine;
using Sirenix.Serialization;
using Sirenix.OdinInspector;
using Unity.Cinemachine;

namespace Assets.Scr.Stage {
    public class WarpPipeBehaviourController : MonoBehaviour {

        [SerializeField]
        private Transform _pipeCenter;

        [SerializeField]
        private Transform _standPoint;

        [SerializeField]
        private WarpPipeBehaviourController _warpPoint;

        [SerializeField]
        private Collider2D _nextCameraArea;

        [SerializeField]
        private IInputModule<Vector2> _moveModule;

        [SerializeField]
        private float _animationDuration = 0.5f;

        [SerializeField]
        private float _collisionRadius = 1.0f;

        [SerializeField]
        private float _inputThreshold = 20.0f;

        [SerializeField]
        private SceneEnum _nextScene = SceneEnum.TitleScene;

        [SerializeField]
        [ReadOnly]
        private bool _onPlayerStanding = false;

        [SerializeField]
        [ReadOnly]
        private Player _cachedPlayerObj = null;
        
        [SerializeField]
        [ReadOnly]
        private CinemachineConfiner2D _cameraConfiner = null;

        private Collider[] _overlappedColliders = new Collider[] { };
        private Vector3 _cachedCenterDireciton = Vector3.zero;
        private bool _registeredInput = false;


        private void Start() {
            _cachedCenterDireciton = _pipeCenter.position - _standPoint.position;
            _cameraConfiner = FindAnyObjectByType<CinemachineConfiner2D>();
            RegisterWarpInput();
        }

        private void Update() {
            if (!_registeredInput) {
                RegisterWarpInput();
            }
            GetPlayerStand();
        }

        private void RegisterWarpInput () {

            float GetAngle (Vector2 dir) {
                return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            }

            bool GetWarpable (InputSignal<Vector2> signal) {
                return GetAngle(signal.Value) >= GetAngle(_cachedCenterDireciton) + _inputThreshold ||
                    GetAngle(signal.Value) <= GetAngle(_cachedCenterDireciton) - _inputThreshold;
            }

            if (_moveModule is null) {
                return;
            }
            _moveModule
                .Stream
                .Where(x => _onPlayerStanding && GetWarpable(x))
                .Subscribe(x => {

                })
                .AddTo(this);
            _registeredInput = true;
        }

        private void GetPlayerStand () {
            Physics.OverlapSphereNonAlloc(_pipeCenter.position, _collisionRadius, _overlappedColliders);
            for (int i = 0; i < _overlappedColliders.Length; i++) {
                var hit = _overlappedColliders[i];
                var player = hit.gameObject.GetComponentFromWhole<Player>();
                if (player is not null) {
                    _onPlayerStanding = true;
                    _cachedPlayerObj = player;
                    return;
                }
            }
            _onPlayerStanding = false;
            _cachedPlayerObj = null;
        }

        private async UniTask EnterSequenceAsync () {
            if (_cachedPlayerObj is null) {
                return;
            }
            var rigidBody = _cachedPlayerObj.gameObject.GetComponentFromWhole<Rigidbody>();
            if (rigidBody is not null) {
                rigidBody.isKinematic = true;
            }

            await MoveTransitionAsync(
                _cachedPlayerObj.transform,
                _standPoint.position,
                _pipeCenter.position,
                _animationDuration
                );

            _cameraConfiner ??= FindAnyObjectByType<CinemachineConfiner2D>();
            _cameraConfiner.BoundingShape2D = _nextCameraArea;
            await _warpPoint.ExitSequenceAsync();

        }
        
        private async UniTask ExitSequenceAsync () {
            if (_cachedPlayerObj is null || _warpPoint is null) {
                return;
            }
            var rigidBody = _cachedPlayerObj.gameObject.GetComponentFromWhole<Rigidbody>();
            if (rigidBody is not null) {
                rigidBody.isKinematic = true;
            }

            await MoveTransitionAsync(
                _cachedPlayerObj.transform,
                _warpPoint._pipeCenter.position,
                _warpPoint._standPoint.position,
                _animationDuration
                );

            if (rigidBody is not null) {
                rigidBody.isKinematic = false;
            }
        }

        private async UniTask MoveTransitionAsync (Transform target, Vector3 from, Vector3 to, float duration) {
            //最初に強制的に移動させる
            target.position = from;
            float time = 0;
            while (time < duration) {
                time += Time.deltaTime;
                float t = time / duration;
                // 簡易イージング (SmoothStep)
                t = t * t * (3f - 2f * t); 
                target.position = Vector3.Lerp(from, to, t);
                await UniTask.Yield(
                    PlayerLoopTiming.Update,
                    this.GetCancellationTokenOnDestroy()
                    );
            }
            target.position = to;
        }
    }
}