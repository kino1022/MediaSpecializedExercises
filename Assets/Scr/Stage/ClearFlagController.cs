using System;
using Cysharp.Threading.Tasks;
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
        private float m_downFlagSpeed = 1.0f;

        private Animator _cachedPlayerAnimator;

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
        }

        private void OnCollisionEnter(Collision other) {
            //衝突したオブジェクトがプレイヤーなら始動
            if (other.gameObject == m_player) {
                //処理の最後なのでForgetを安全に使用できる
                StartClearScript().Forget();
            }
        }

        private async UniTask StartClearScript() {
            //操作可能フラグを折っておく
            _playable.SetPlayable(false);
            
            //旗の根本に移動するのを待機
            await LocomotionFlagBottom();
        }

        private async UniTask LocomotionFlagBottom() {
            try {
                //プレイヤーから旗の根本への方向ベクトルを取得
                var velocity = (m_player.transform.position - m_flagBottom.transform.position).normalized;

                while (m_player.transform.position == m_flagBottom.transform.position || !this.GetCancellationTokenOnDestroy().IsCancellationRequested) {
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
                
            }
        }
    }
}