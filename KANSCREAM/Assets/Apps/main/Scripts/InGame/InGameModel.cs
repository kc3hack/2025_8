using R3;

public class InGameModel
{
    public enum GameState
    {
        Start,
        BeforeScream,
        AfterScream,
        Result,
    }

    private readonly ReactiveProperty<GameState> _gameState;
    public ReadOnlyReactiveProperty<GameState> GameStateProp => _gameState;
    public GameState CurrentGameStateProp => _gameState.Value;

    public InGameModel()
    {
        _gameState = new ReactiveProperty<GameState>(GameState.Start);
    }
}