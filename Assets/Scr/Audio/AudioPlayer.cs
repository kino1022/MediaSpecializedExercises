using System;
using MessagePipe;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Scr.Audio {
    /// <summary>
    /// 音声を再生するクラス
    /// </summary>
    public class AudioPlayer : SerializedMonoBehaviour {

        private AudioSource _source;

        private IObjectResolver _resolver;

        private ISubscriber<AudioPlayEventBus> _audioSubscriber;

        private IDisposable _audioSubscription;

        [Inject]
        public void Construct(IObjectResolver resolver) {
            _resolver = resolver;
        }

        private void Start() {
            
            _source = _resolver.Resolve<AudioSource>();
            
            _audioSubscriber = 
                _resolver
                    .Resolve<ISubscriber<AudioPlayEventBus>>();

            _audioSubscription = 
                _audioSubscriber
                    //音声再生依頼を受け取った際の処理を登録
                    .Subscribe(OnTakeEventBus)
                    //オブジェクトが死ぬとこの購読も共倒れするようにする
                    .AddTo(this);
        }

        //音声再生依頼を受け取った場合の処理
        private void OnTakeEventBus(AudioPlayEventBus bus) {
            
            Debug.Log("音がきたんですよ");

            //Emitterに指定されたオブジェクトが自分の親子になければスルーする
            //するーするってかwwwwww
            if (!bus.Emitter.transform.root.IsChildOf(transform)) {
                Debug.Log("でも管轄外なんですよ");
                return;
            }
            
            //AudioSourceがなかった場合は再取得を試行する
            _source ??= _resolver.Resolve<AudioSource>();

            //AudioSourceが取得できなかった場合はぬるりを投げる
            if (_source is null) {
                throw new NullReferenceException();
            }

            Debug.Log("音が鳴るんですよ");
            
            //コンテキストの適用処理
            bus.Context.ApplyContext(_source);
            //音声再生処理
            _source.PlayOneShot(bus.Clip);
            
        }
    }
}