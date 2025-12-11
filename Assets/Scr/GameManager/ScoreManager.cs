namespace Scr.GameManager {
    public static class ScoreManager {

        private static int _score = 0;

        public static int Score => _score;

        
        public static void SetScore(int value) {
            if (value < 0) {
                _score = 0;
                return;
            }
            _score = value;
        }
        
        public static void AddScore(int value) {
            SetScore(_score + value);
        }
    }
}