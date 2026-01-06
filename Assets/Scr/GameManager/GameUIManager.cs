using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Scr.GameManager {
    public class GameUIManager : SerializedMonoBehaviour {
        
        [SerializeField]
        private UIDocument uiDocument;

        [SerializeField]
        [LabelText("スコアのラベル")]
        private Label _scoreLabel;

        private Label _timeLabel;
        
        private Label _coinLabel;
        
        private Label _lifeLabel;
        
        private ITimeManager _timeManager;
        
        private IScoreManager _scoreManager;
        
        private ILifeManager _lifeManager;
        
        private ICoinManager _coinManager;
        
        private IObjectResolver _resolver;
        
        [Inject]
        public void Construct (IObjectResolver resolver) {
            _resolver = resolver;
        }
        
        private void Start() {
            
            _scoreManager = _resolver.Resolve<IScoreManager>();
            
            _timeManager = _resolver.Resolve<ITimeManager>();
            
            _lifeManager = _resolver.Resolve<ILifeManager>();
            
            _coinManager = _resolver.Resolve<ICoinManager>();
            
            if (uiDocument == null) {
                Debug.LogError("UIDocument is not assigned.");
                return;
            }

            var root = uiDocument.rootVisualElement;
            var uiScreenView = new Scr.Utility.PlayUIScreenView(root);
            _scoreLabel = uiScreenView.ScoreLabel;
            _timeLabel = uiScreenView.TimeLabel;
            _lifeLabel = root.Q<Label>("life");
            _coinLabel = root.Q<Label>("coin");
        }

        private void Update() {
            
            if (_scoreLabel != null && _scoreManager != null) {
                _scoreLabel.text = $"{_scoreManager.Score}";
            }
            
            _timeLabel.text = $"{_timeManager.CurrentCount}";
            
            _coinLabel.text = $"{_coinManager.CurrentValue}";
            
            _lifeLabel.text = $"{_lifeManager.CurrentLife}";
            
        }
    }
}