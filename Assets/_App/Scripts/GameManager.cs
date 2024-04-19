using UnityEngine;
using UnityEngine.Events;

public class GameManager : Singleton<GameManager>
{
    public enum GameMode { None, Building, Gazing }

    [SerializeField] private GameMode _currentGameMode = GameMode.None;

    [Space]
    public UnityEvent<GameMode> OnGameModeChanged;

    private GameMode _lastValidatedGameMode = GameMode.None;

    void OnValidate()
    {
        if (_lastValidatedGameMode != _currentGameMode)
        {
            Debug.Log($"[{nameof(GameManager)}] {nameof(OnValidate)}: {nameof(GameMode)} changed: previous={_lastValidatedGameMode}, current={_currentGameMode}");
            _lastValidatedGameMode = _currentGameMode;
            OnGameModeChanged?.Invoke(_currentGameMode);
        }
    }
}
