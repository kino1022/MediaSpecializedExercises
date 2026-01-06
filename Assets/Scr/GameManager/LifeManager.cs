using System;
using MessagePipe;
using UnityEngine.Pool;
using VContainer;

namespace Scr.GameManager {
    
    public struct GameOverEventBus {
        
    }
    
    public struct DecreaseLifeEventBus {
        
        public int LifeValue;

        public DecreaseLifeEventBus(int lifeValue) {
            LifeValue = lifeValue;
        }
        
    }

    public interface ILifeManager {
        
        int CurrentLife { get; }
        
        void SetLife(int value);
        
    }
    
    public class LifeManager : ILifeManager, IDisposable {
        
        private int _currentLife = 3;
        
        public int CurrentLife => _currentLife;
        
        private IPublisher<GameOverEventBus> _gameOverPublisher;
        
        private ISubscriber<GainLifeEventBus> _gainSubscriber;
        
        private IDisposable _gainSubscription;
        
        private ISubscriber<DecreaseLifeEventBus> _decreaseSubscriber;
        
        private IDisposable _decreaseSubscription;

        public LifeManager(IObjectResolver resolver) {
            if (resolver is not null) {
                _gameOverPublisher = resolver.Resolve<IPublisher<GameOverEventBus>>();
                _gainSubscriber = resolver.Resolve<ISubscriber<GainLifeEventBus>>();
                _decreaseSubscriber = resolver.Resolve<ISubscriber<DecreaseLifeEventBus>>();
                _gainSubscription = _gainSubscriber.Subscribe(OnGainLifeEventBus);
                _decreaseSubscription = _decreaseSubscriber.Subscribe(OnDecreaseLifeEventBus);
            }
        }

        public void Dispose() {
            _gainSubscription.Dispose();
            _decreaseSubscription.Dispose();
        }

        public void SetLife(int value) {
            _currentLife = value;
            if (CurrentLife <= 0) {
                _currentLife = 0;
                _gameOverPublisher.Publish(new GameOverEventBus());
            }
        }

        private void OnGainLifeEventBus(GainLifeEventBus bus) {
            SetLife(_currentLife + bus.LifeValue);
        }

        private void OnDecreaseLifeEventBus(DecreaseLifeEventBus bus) {
            SetLife(_currentLife - bus.LifeValue);
        }
    }
}