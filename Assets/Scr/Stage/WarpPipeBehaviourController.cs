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

namespace Assets.Scr.Stage {
    public class WarpPipeBehaviourController : MonoBehaviour {

        [SerializeField]
        private Transform _pipeCenter;

        [SerializeField]
        private Transform _standPoint;

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

        private Vector3 _cachedCenterDireciton = Vector3.zero;
        private bool _registeredInput = false;


        private void Start() {
            _cachedCenterDireciton = _pipeCenter.position - _standPoint.position;
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

            if (_moveModule is not null) {
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
            var hits = Physics.OverlapSphere(_standPoint.position, _collisionRadius);
            for (int i = 0; i < hits.Length; i++) {
                var hit = hits[i];
                var player = hit.gameObject.GetComponentFromWhole<Player>();
                if (player is not null) {
                    _onPlayerStanding = true;
                    _cachedPlayerObj = player;
                    return;
                }
                else {
                    _onPlayerStanding = false;
                    _cachedPlayerObj = null;
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

            await MoveTransitionAsync();

        }

        private async UniTask MoveTransitionAsync () {

        }
    }
}