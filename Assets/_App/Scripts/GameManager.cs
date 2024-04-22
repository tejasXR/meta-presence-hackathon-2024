using UnityEngine;
using UnityEngine.Events;

public enum GameMode { Building, Gazing }

public class GameManager : Singleton<GameManager>
{
    public GameMode CurrentGameMode = GameMode.Building;

    [SerializeField] private PassthroughController _passthroughController;

    [Space]
    public UnityEvent<GameMode> OnGameModeChanged;

    private GameMode _lastValidatedGameMode = GameMode.Building;

    void Start()
    {
        GameModeChanged();
    }

    void OnValidate()
    {
        if (_lastValidatedGameMode != CurrentGameMode)
        {
            Debug.Log($"[{nameof(GameManager)}] {nameof(OnValidate)}: {nameof(GameMode)} changed: previous={_lastValidatedGameMode}, current={CurrentGameMode}");
            _lastValidatedGameMode = CurrentGameMode;
            GameModeChanged();
        }
    }

    private void GameModeChanged()
    {
        _passthroughController.SetLut((int)CurrentGameMode);
        OnGameModeChanged?.Invoke(CurrentGameMode);
    }
}
