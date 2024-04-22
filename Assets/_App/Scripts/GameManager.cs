using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public enum GameMode { Building, Gazing }

    [SerializeField] private GameMode _currentGameMode = GameMode.Building;

    [SerializeField] private PassthroughController _passthroughController;

    [Space]
    public UnityEvent<GameMode> OnGameModeChanged;

    private GameMode _lastValidatedGameMode = GameMode.Building;

    void OnValidate()
    {
        if (_lastValidatedGameMode != _currentGameMode)
        {
            Debug.Log($"[{nameof(GameManager)}] {nameof(OnValidate)}: {nameof(GameMode)} changed: previous={_lastValidatedGameMode}, current={_currentGameMode}");
            _lastValidatedGameMode = _currentGameMode;
            GameModeChanged();
        }
    }

    private void GameModeChanged()
    {
        _passthroughController.SetLut((int)_currentGameMode);
        OnGameModeChanged?.Invoke(_currentGameMode);
    }
}
