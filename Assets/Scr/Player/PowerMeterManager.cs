using System;
using Cysharp.Threading.Tasks;
using Scr.Player.Action;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Scr.Player {

    public interface IPowerMeterManager {
        
        int Power { get; }
        
        void SetPower(int power);
    }
    
    public class PowerMeterManager : SerializedMonoBehaviour, IPowerMeterManager {

        [SerializeField] [LabelText("現在パワー")]
        private int _power;

        [SerializeField] [LabelText("増加が始まるまでの時間")]
        private float _startDuration;
        
        [SerializeField]
        [LabelText("パワーの増える間隔")]
        private float _increaseInterval;
        
        [SerializeField]
        [LabelText("パワーの減る間隔")]
        private float _decreaseInterval;

        private bool _isIncreaseProcess = false;
        
        private bool _isDecreaseProcess = false;
        
        private Rigidbody _rigidbody;

        private IMoveAction _moveAction;
        
        private IGroundedManger _grounded;

        private IObjectResolver _resolver;
        
        public int Power => _power;

        [Inject]
        public void Construct(IObjectResolver resolver) {
            _resolver = resolver;
        }

        private void Start() {
            //各種コンポーネントの取得処理(エディタでアタッチする際は不要)
            _moveAction = _resolver.Resolve<IMoveAction>();
            _grounded = _resolver.Resolve<IGroundedManger>();
            _rigidbody = _resolver.Resolve<Rigidbody>();
        }

        private void FixedUpdate() {
            if (
                //着地していて
                _grounded.IsGrounded &&
                //走っていて
                _moveAction.IsSprint &&
                //既に増加タスクが走っていないなら
                _isIncreaseProcess
                ) {
                //増加タスクを走らせる
                IncreasePowerInSprint().Forget();
            }
            //それ以外の場合で現象処理が走っていないなら
            else if (_isDecreaseProcess) {
                //減少処理を走らせる
                DecreasePower().Forget();
            }
        }
        
        public void SetPower(int power) {
            _power = power;
        }

        private async UniTask IncreasePowerInSprint() {

            _isIncreaseProcess = true;
            try {
                //上昇が始まるまでの微妙な時間を再現
                await UniTask.Delay(TimeSpan.FromSeconds(_startDuration));

                if (!_moveAction.IsSprint) {
                    _isIncreaseProcess = false;
                    return;
                }

                while (_moveAction.IsSprint && !this.GetCancellationTokenOnDestroy().IsCancellationRequested) {
                    //待機前の進行方向を保存
                    var previousDirection = _rigidbody.linearVelocity.normalized;
                    //増加の間隔分待機
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(_increaseInterval),
                        cancellationToken: this.GetCancellationTokenOnDestroy()
                        );
                    //待機後の進行方向を取得
                    var currentDirection = _rigidbody.linearVelocity.normalized;

                    if (
                        //反転していた場合(走る方向が逆なので増加が止まる)
                        Math.Sign(previousDirection.x) != Math.Sign(currentDirection.x) ||
                        //着地していない場合も増加を止める
                        !_grounded.IsGrounded
                        ) {
                        _isIncreaseProcess = false;
                        return;
                    }

                    //方向が一致して居た場合はパワーを増やす
                    _power++;
                }
            }
            catch (OperationCanceledException) {
                
            }
            finally {
                _isIncreaseProcess = false;
            }
        }

        private async UniTask DecreasePower() {
            _isDecreaseProcess = true;
            try {
                while (_moveAction.IsSprint && !this.GetCancellationTokenOnDestroy().IsCancellationRequested) {
                    await UniTask.Delay(TimeSpan.FromSeconds(_decreaseInterval));
                    _power--;
                }
            }
            catch (OperationCanceledException) {
                
            }
            finally {
                _isDecreaseProcess = false;
            }
        }
    }
}