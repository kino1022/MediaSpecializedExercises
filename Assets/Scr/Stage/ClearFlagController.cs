using System;
using Cysharp.Threading.Tasks;
using Scr.GameManager;
using Scr.Player;
using Scr.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Scr.Stage {
    public class ClearFlagController : SerializedMonoBehaviour {

        /// <summary>
        /// 旗の頂点の座標を保持するためのGameObject
        /// </summary>
        [SerializeField]
        private GameObject m_flagTop;
        
        /// <summary>
        /// 旗の根本の座標を保持するためのGameObject
        /// </summary>
        [SerializeField]
        private GameObject m_flagBottom;
        
        /// <summary>
        /// 歩行演出が終了する座標
        /// </summary>
        [SerializeField]
        private GameObject m_walkEndPoint;

        /// <summary>
        /// プレイヤーのゲームオブジェクト
        /// </summary>
        [SerializeField]
        private GameObject m_player;

        /// <summary>
        /// クリア演出時の歩行速度
        /// </summary>
        [SerializeField]
        private float m_clearWalkSpeed = 10.0f;
        
        /// <summary>
        /// 旗を滑る際の移動速度
        /// </summary>
        [SerializeField]
        private float m_downFlagSpeed = 1.0f;
        
        /// <summary>
        /// 残り時間1秒あたりのスコア加算量
        /// </summary>
        [SerializeField]
        private int m_scorePerTime = 100;
        
        /// <summary>
        /// 次のシーンのEnum
        /// </summary>
        [SerializeField]
        private SceneEnum m_nextScene;
        
        private ITimeManager _timeManager;
        
        private IScoreManager _scoreManager;

        private Animator _cachedPlayerAnimator;
        
        private Rigidbody _cachedPlayerRigidbody;

        private IPlayableManager _playable;

        private IObjectResolver _resolver;
        
        private IObjectResolver _playerResolver;

        [Inject]
        public void Construct(IObjectResolver resolver) {
            _resolver = resolver;
        }

        private void Start() {
            //コンテナからPlayableフラグを管理するオブジェクトを取得
            _playable = _resolver.Resolve<IPlayableManager>();
            //取得できなかった場合は全体からコンポーネント取得
            _playable ??= gameObject.GetComponentFromWhole<IPlayableManager>();
            
            //プレイヤーのコンテナを取得
            _playerResolver = m_player.gameObject.GetComponentFromWhole<IObjectResolver>();
            
            //プレイヤーのアニメーターを取得
            _cachedPlayerAnimator = _playerResolver.Resolve<Animator>();
            //取得できなかった場合は全体からコンポーネント取得
            _cachedPlayerAnimator ??= gameObject.GetComponentFromWhole<Animator>();
            
            //プレイヤーのリジッドボディを取得
            _cachedPlayerRigidbody = _playerResolver.Resolve<Rigidbody>();
            //取得できなかった場合は全体からコンポーネント取得
            _cachedPlayerRigidbody ??= gameObject.GetComponentFromWhole<Rigidbody>();
            
            _timeManager = _resolver.Resolve<ITimeManager>();
            
            _scoreManager = _resolver.Resolve<IScoreManager>();
        }

        private void OnCollisionEnter(Collision other) {
            //衝突したオブジェクトがプレイヤーなら始動
            if (other.gameObject == m_player) {
                //処理の最後なのでForgetを安全に使用できる
                StartClearScript().Forget();
            }
        }

        private async UniTask StartClearScript() {
            //RigidBodyが本当にあるかどうかの最終確認となかった場合の取得
            _cachedPlayerRigidbody ??= m_player.GetComponentFromWhole<Rigidbody>();
            if (_cachedPlayerRigidbody is null) {
                return;
            }
            
            //移動量をゼロにする
            _cachedPlayerRigidbody.linearVelocity = Vector3.zero;
            _cachedPlayerRigidbody.angularVelocity = Vector3.zero;
            //物理挙動を殺す
            _cachedPlayerRigidbody.isKinematic = true;
            
            //操作可能フラグを折っておく
            _playable.SetPlayable(false);
            
            //旗の根本に移動するのを待機
            await LocomotionFlagBottom();
            
            //残り時間をスコアに加算する演出
            await AddTimeScore();
            
            //とぼとぼ歩いていく演出
            await LocomotionStageOut();
            
            //次のシーンへ遷移
            m_nextScene.LoadScene();
        }

        private async UniTask LocomotionFlagBottom() {
            try {
                
                _cachedPlayerAnimator ??= m_player.GetComponentFromWhole<Animator>();
                if (_cachedPlayerAnimator is null) {
                    return;
                }
                
                //プレイヤーから旗の根本への方向ベクトルを取得
                var velocity = (m_player.transform.position - m_flagBottom.transform.position).normalized;

                while (m_player.transform.position == m_flagBottom.transform.position || !this.GetCancellationTokenOnDestroy().IsCancellationRequested) {
                    
                    //アニメーションの再生
                    _cachedPlayerAnimator.SetBool("Bear_ride", true);
                    
                    //一気に動かないように一定時間待機
                    await UniTask.Delay(
                        //待機時間は0.1秒に指定(これは本来なら変数にするべき)
                        TimeSpan.FromSeconds(0.1f),
                        cancellationToken: this.GetCancellationTokenOnDestroy()
                    );
                    
                    //次の座標を算出
                    var nextPosition = m_player.transform.position + velocity * m_clearWalkSpeed;
                    //演出なのでTransformに対して直で代入する
                    m_player.transform.position = nextPosition;
                }
            }
            catch (OperationCanceledException) {
                Debug.LogWarning("クリア演出中に処理がキャンセルされました。テストモードを落としてください");
            }
            finally {
                //アニメーションの再生を終了する
                _cachedPlayerAnimator?.SetBool("Bear_ride", false);
            }
        }

        private async UniTask AddTimeScore() {
            try {
                while (_timeManager.CurrentCount <= 0 || !this.GetCancellationTokenOnDestroy().IsCancellationRequested) {
                    //待機時間を設定
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(0.1f),
                        cancellationToken: this.GetCancellationTokenOnDestroy()
                    );
                    
                    //時間を1減らす
                    _timeManager.SetCurrentCount(_timeManager.CurrentCount - 1);
                    
                    //スコアに加算する処理をここに追加
                    _scoreManager.AddScore(m_scorePerTime);
                }
            }
            catch (OperationCanceledException) {

            }
            finally {
                
            }
        }

        private async UniTask LocomotionStageOut() {
            try {
                var velocity = (m_player.transform.position - m_flagBottom.transform.position).normalized;
                
                _cachedPlayerAnimator.SetFloat("Speed", 1.0f);
                _cachedPlayerAnimator.SetBool("Walk", true);
                
                while (m_player.transform.position == m_walkEndPoint.transform.position || !this.GetCancellationTokenOnDestroy().IsCancellationRequested) {
                    
                    //アニメーションの再生
                    _cachedPlayerAnimator.SetBool("Walk", true);
                    
                    //一気に動かないように一定時間待機
                    await UniTask.Delay(
                        //待機時間は0.1秒に指定(これは本来なら変数にするべき)
                        TimeSpan.FromSeconds(0.1f),
                        cancellationToken: this.GetCancellationTokenOnDestroy()
                    );
                    
                    //次の座標を算出
                    var nextPosition = m_player.transform.position + velocity * m_clearWalkSpeed;
                    //演出なのでTransformに対して直で代入する
                    m_player.transform.position = nextPosition;
                }
            }
            catch (OperationCanceledException) {

            }
            finally {
                _cachedPlayerAnimator.SetFloat("Speed", 0.0f);
                _cachedPlayerAnimator.SetBool("Walk", false);
            }
        }
    }
}