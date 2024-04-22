using UnityEngine;
using UnityEngine.Events;

public enum GameMode { Building, Gazing }

public class GameManager : Singleton<GameManager>
{
    public GameMode CurrentGameMode = GameMode.Building;

    [Header("Gazing Mode Config")]
    [SerializeField] private float _gazingPositionThreshold = 0.01f;
    [SerializeField] private float _gazingTimeThreshold = 3f;

    [Space]
    [SerializeField] private PassthroughController _passthroughController;

    [Space]
    public UnityEvent<GameMode> OnGameModeChanged;

    private Transform _cameraTransform;
    private Vector3 _lastCameraPosition;
    private float _gazingTimer = 0f;

#if UNITY_EDITOR
    private GameMode _lastValidatedGameMode = GameMode.Building;
#endif

    void Start()
    {
        _cameraTransform = Camera.main.transform;
        _lastCameraPosition = _cameraTransform.position;

        GameModeChanged(CurrentGameMode);
    }

    void Update()
    {
        float deltaPos = Vector3.Distance(_lastCameraPosition, _cameraTransform.position);

        if (deltaPos < _gazingPositionThreshold)
        {
            _gazingTimer += Time.deltaTime;
            if (_gazingTimer >= _gazingTimeThreshold && CurrentGameMode != GameMode.Gazing)
            {
                GameModeChanged(GameMode.Gazing);
            }
        }
        else
        {
            _gazingTimer = 0f;
            if (CurrentGameMode != GameMode.Building)
            {
                GameModeChanged(GameMode.Building);
            }
        }

        _lastCameraPosition = _cameraTransform.position;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (_lastValidatedGameMode != CurrentGameMode)
        {
            Debug.Log(message: $"[{nameof(GameManager)}] {nameof(OnValidate)}: {nameof(GameMode)} changed: previous={_lastValidatedGameMode}, current={CurrentGameMode}");
            _lastValidatedGameMode = CurrentGameMode;

            GameModeChanged(CurrentGameMode);
        }
    }
#endif

    private void GameModeChanged(GameMode mode)
    {
        CurrentGameMode = mode;

        _passthroughController.SetLut((int)mode);
        OnGameModeChanged?.Invoke(mode);
    }
}
