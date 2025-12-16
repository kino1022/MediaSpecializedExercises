using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using VContainer;

namespace Scr.GameManager {

    public interface ITimeManager {
        
        int CurrentCount { get; }
        
        void InitTimer (int MaxCount);

        void StartCount();
        
        void StopCount();

        void SetCurrentCount(int count);
        
        void SetPublishTimeOver (bool isPublish);
    }

    public readonly struct TimeOverEventBus {
        
    }

    public class TimeManager : ITimeManager {
        
        private int _maxCount;

        private int _currentCount = 100;
        
        private bool _isCounting = false;
        
        private bool _isPublishTimeOver = false;
        
        private IPublisher<TimeOverEventBus> _timeOverPublisher;

        private IObjectResolver _resolver;

        public int CurrentCount => _currentCount;
        
        public TimeManager(IObjectResolver resolver) {
            CountAsync().Forget();
        }

        public void InitTimer(int maxCount) {
            _maxCount = maxCount;
            _currentCount = maxCount;
        }

        public void StartCount() {
            _isCounting = true;
        }

        public void StopCount() {
            _isCounting = false;
        }

        public void SetCurrentCount(int count) {
            _currentCount = count;
        }

        public void SetPublishTimeOver(bool isPublish) {
            _isPublishTimeOver = isPublish;
        }

        private async UniTask CountAsync() {
            while (_currentCount <= 0) {
                await UniTask.WaitUntil(() => _isCounting == true);
                
                await UniTask.Delay(
                    TimeSpan.FromSeconds(1),
                    cancellationToken: CancellationToken.None
                );
                _currentCount--;
            }
            
            if (_currentCount <= 0 && _isPublishTimeOver) {
                _timeOverPublisher.Publish(new TimeOverEventBus());
            }
        }
    }
}