using System;
using MessagePipe;

namespace Scr.GameManager {
    
    public struct GetCoinEventBus {
        
        public int CoinValue;

        public GetCoinEventBus(int coinValue) {
            CoinValue = coinValue;
        }
        
    }
    
    public struct GainLifeEventBus {
        
        public int LifeValue;

        public GainLifeEventBus(int lifeValue) {
            LifeValue = lifeValue;
        }
        
    }
    
    public interface ICoinManager {
        
        int CurrentValue { get; }
        
    }
    
    public class CoinManager : ICoinManager, IDisposable{
        
        private int _currentValue = 0;
        
        public int CurrentValue => _currentValue;
        
        private IPublisher<GainLifeEventBus> _gainLifePublisher;
        
        private ISubscriber<GetCoinEventBus> _subscriber;
        
        private IDisposable _subscription;

        public CoinManager(ISubscriber<GetCoinEventBus> coinEventBus, IPublisher<GainLifeEventBus> gainLifePublisher) {
            _subscriber = coinEventBus;
            _gainLifePublisher = gainLifePublisher;
            _subscription = _subscriber.Subscribe(OnTakeEventBus);
        }

        public void Dispose() {
            _subscription.Dispose();
        }

        private void OnTakeEventBus(GetCoinEventBus getCoinEventBus) {
            ChangeCoinValue(_currentValue + getCoinEventBus.CoinValue);
        }
        
        private void ChangeCoinValue(int value) {
            var next = value;
            if (next >= 100) {
                var life = next / 100;
                next = next % 100;
                _gainLifePublisher.Publish(new GainLifeEventBus(life));
            }
            _currentValue = next;
        }
    }
}