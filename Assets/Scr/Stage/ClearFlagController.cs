using System;
using Cysharp.Threading.Tasks;
using Scr.GameManager;
using Scr.Player;
using Scr.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Scr.Stage {

    public enum ClearPerformanceEnum {
        None,
        RideFlag,
        WalkStageOut
    }
    
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

        private ClearPerformanceEnum _performanceEnum = ClearPerformanceEnum.None;
        
        private ITimeManager _timeManager;
        
        private IScoreManager _scoreManager;

        [SerializeField]
        private Animator _cachedPlayerAnimator;
        
        [SerializeField]
        private Rigidbody _cachedPlayerRigidbody;

        private IPlayableManager _playable;

        private IObjectResolver _resolver;
        
        private LifetimeScope _playerResolver;

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
            _playerResolver = m_player.gameObject.GetComponentFromWhole<LifetimeScope>();
            
            //プレイヤーのアニメーターを取得
            _cachedPlayerAnimator = _playerResolver.Container.Resolve<Animator>();
            //取得できなかった場合は全体からコンポーネント取得
            _cachedPlayerAnimator ??= gameObject.GetComponentFromWhole<Animator>();
            
            //プレイヤーのリジッドボディを取得
            _cachedPlayerRigidbody = _playerResolver.Container.Resolve<Rigidbody>();
            //取得できなかった場合は全体からコンポーネント取得
            _cachedPlayerRigidbody ??= gameObject.GetComponentFromWhole<Rigidbody>();
            
            _timeManager = _resolver.Resolve<ITimeManager>();
            
            _scoreManager = _resolver.Resolve<IScoreManager>();
        }

        private void Update() {
            
            if (_performanceEnum is ClearPerformanceEnum.None) {
                return;
            }
            
            if (_performanceEnum is ClearPerformanceEnum.RideFlag) {
                _cachedPlayerAnimator.SetBool("ride",true);
                return;
            }
            else {
                _cachedPlayerAnimator.SetBool("ride",false);
            }

            if (_performanceEnum is ClearPerformanceEnum.WalkStageOut) {
                _cachedPlayerAnimator.SetFloat("speed", 1);
                _cachedPlayerAnimator.SetBool("move", true);
            }

        }

        private void OnTriggerEnter(Collider other) {
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
            
            _cachedPlayerAnimator.Rebind();
            _performanceEnum = ClearPerformanceEnum.RideFlag;
            
            _scoreManager.AddScore(CalculateScore());
            
            //旗の根本に移動するのを待機
            await LocomotionFlagBottom();
            
            _performanceEnum = ClearPerformanceEnum.None;
            
            //残り時間をスコアに加算する演出
            await AddTimeScore();
            
            _performanceEnum = ClearPerformanceEnum.WalkStageOut;
            
            //とぼとぼ歩いていく演出
            await LocomotionStageOut();
            
            _performanceEnum = ClearPerformanceEnum.None;
            
            //次のシーンへ遷移
            m_nextScene.LoadScene();
        }

        private async UniTask LocomotionFlagBottom() {
            try {
                //プレイヤーから旗の根本への方向ベクトルを取得
                var velocity = (m_flagBottom.transform.position - m_player.transform.position).normalized;

                while (m_player.transform.position.y > m_flagBottom.transform.position.y) {
                    
                    
                    //一気に動かないように一定時間待機
                    await UniTask.Delay(
                        //待機時間は0.1秒に指定(これは本来なら変数にするべき)
                        TimeSpan.FromSeconds(0.1f),
                        cancellationToken: this.GetCancellationTokenOnDestroy()
                    );
                    
                    //次の座標を算出
                    var nextPosition = m_player.transform.position + velocity * m_downFlagSpeed;
                    //演出なのでTransformに対して直で代入する
                    m_player.transform.position = nextPosition;
                }
                
            }
            catch (OperationCanceledException) {
                Debug.LogWarning("クリア演出中に処理がキャンセルされました。テストモードを落としてください");
            }
            finally {
            }
            
        }

        private async UniTask AddTimeScore() {
            try {
                _timeManager.StopCount();
                while (_timeManager.CurrentCount > 1) {
                    //待機時間を設定
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(0.01f),
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
                
                var velocity = (m_walkEndPoint.transform.position - m_player.transform.position).normalized;
                
                while (m_player.transform.position.x <= m_walkEndPoint.transform.position.x) {

                    m_player.transform.localScale = velocity.x < 0.0f
                        ? new Vector3(-1.0f, 1.0f, 1.0f)
                        : new Vector3(1.0f, 1.0f, 1.0f);
                    
                    //一気に動かないように一定時間待機
                    await UniTask.Delay(
                        //待機時間は0.1秒に指定(これは本来なら変数にするべき)
                        TimeSpan.FromSeconds(0.01f),
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
            }
        }

        private int CalculateScore() {
            float deltaHeight = m_player.transform.position.y - m_flagBottom.transform.position.y;
            
            float flagHeight = m_flagTop.transform.position.y - m_flagBottom.transform.position.y;

            float divValue = flagHeight / 5;

            if (deltaHeight >= divValue * 4) {
                return 5000;
            }
            if (deltaHeight >= divValue * 3) {
                return 4000;
            }
            if (deltaHeight >= divValue * 2) {
                return 3000;
            }
            if (deltaHeight >= divValue * 1) {
                return 2000;
            }

            return 1000;
        }
    }
}