using R3;

namespace refactor
{
    public class InGameModel
    {
        public enum GameState
        {
            Start,
            BeforeScream,
            AfterScream,
            Result,
        }

        private ReactiveProperty<GameState> _gameState;
        public ReactiveProperty<GameState> GameStateProp => _gameState;
        public GameState CurrentGameStateProp => _gameState.Value;

        public InGameModel()
        {
            _gameState = new ReactiveProperty<GameState>(GameState.Start);
        }
    }
}