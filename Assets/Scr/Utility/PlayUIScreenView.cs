using UnityEngine.UIElements;

namespace Scr.Utility {
    public class PlayUIScreenView {
        
        public Label CoinLabel { get; }

        public Label ScoreLabel { get; }

        public Label TimeLabel { get; }
        
        public Label NameLabel { get; }

        public PlayUIScreenView(VisualElement root) {
            ScoreLabel = root.Q<Label>("PlayerScore");
            TimeLabel = root.Q<Label>("PlayerTime");
            NameLabel = root.Q<Label>("PlayerName");
        }
        
    }
}