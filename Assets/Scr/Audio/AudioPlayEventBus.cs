using UnityEngine;

namespace Scr.Audio {
    /// <summary>
    /// 外部のクラスに対して音声再生を依頼するためのイベントバス
    /// </summary>
    public readonly struct AudioPlayEventBus {

        /// <summary>
        /// 音を発するオブジェクト
        /// </summary>
        private readonly GameObject _emitter;
        
        /// <summary>
        /// 音に対して適用する特徴
        /// </summary>
        private readonly AudioPlayContext _context;

        /// <summary>
        /// 再生する音声クリップ
        /// </summary>
        private readonly AudioClip _clip;
        
        public GameObject Emitter => _emitter;
        
        public AudioPlayContext Context => _context;
        
        public AudioClip Clip => _clip;

        public AudioPlayEventBus(AudioPlayContext context, AudioClip clip, GameObject emitter) {
            
            _emitter = emitter;
            
            _context = context;
            
            _clip = clip;
            
        }
        
    }

    /// <summary>
    /// 再生する音の特徴を指定するコンテキスト
    /// </summary>
    public struct AudioPlayContext {

        /// <summary>
        /// 音量
        /// </summary>
        private float _volume;

        /// <summary>
        /// ループさせるかどうか
        /// </summary>
        private bool _loop;
        
        public AudioPlayContext(float volume = (float)1, bool loop = false) {
            
            _volume = volume;
            
            _loop = loop;
            
        }

        /// <summary>
        /// コンテキストの内容を引数として受け取ったAudioSourceに対して適用する
        /// </summary>
        /// <param name="source"></param>
        public void ApplyContext(AudioSource source) {
            //音量設定の適用
            source.volume = _volume;
            //ループ設定の適用
            source.loop = _loop;
        }
        
    }
}