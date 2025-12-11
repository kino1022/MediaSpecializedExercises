using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scr.GameManager {
    public class GameUIManager : SerializedMonoBehaviour {
        
        [SerializeField]
        private UIDocument uiDocument;

        [SerializeField]
        [LabelText("スコアのラベル")]
        private Label _scoreLabel;
        
        private void Start() {
            if (uiDocument == null) {
                Debug.LogError("UIDocument is not assigned.");
                return;
            }

            var root = uiDocument.rootVisualElement;
            var uiScreenView = new Scr.Utility.PlayUIScreenView(root);
            _scoreLabel = uiScreenView.ScoreLabel;
        }

        private void Update() {
            _scoreLabel.text = ScoreManager.Score.ToString();
        }
    }
}