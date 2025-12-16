namespace Scr.GameManager {

    public interface IScoreManager {
        
        int Score { get; }
        
        void SetScore(int value);
        
        void AddScore(int value);
    }
    
    public class ScoreManager : IScoreManager {

        private int _score = 0;

        public int Score => _score;

        public ScoreManager() {
            
        }
        
        public void SetScore(int value) {
            if (value < 0) {
                _score = 0;
                return;
            }
            _score = value;
        }
        
        public void AddScore(int value) {
            SetScore(_score + value);
        }
    }
}