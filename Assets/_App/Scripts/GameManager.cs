using UnityEngine;
using UnityEngine.Events;

public enum GameMode { Lobby, Building, Gazing }

public class GameManager : Singleton<GameManager>
{
    public enum ModeChange { PlayerPosition, HandsPosition, PinchGesture }

    public GameMode CurrentGameMode = GameMode.Lobby;

    [Header("Gazing Mode Config")]
    [SerializeField] private ModeChange _modeChange = ModeChange.PlayerPosition;
    [SerializeField] private float _gazingPositionThreshold = 0.01f;
    [SerializeField] private float _gazingTimeThreshold = 3f;
    [SerializeField] private float _gazingTransitionDuration = 5f;
    [SerializeField] private float _buildingTransitionDuration = 1f;

    [Header(header: "Plants Growth Config")]
    [SerializeField] private float _plantsGrowthFrequency = 0.01f;

    [SerializeField] private float _awayPlantsGrowthSpeed = 0.25f;
    [SerializeField] private float _lobbyPlantsGrowthSpeed = 0f;
    [SerializeField] private float _buildingPlantsGrowthSpeed = 1f;
    [SerializeField] private float _gazingPlantsGrowthSpeed = 10f;

    [Header("Hands")]
    [SerializeField] private Transform _leftHandAnchor;
    [SerializeField] private Transform _righHandAnchor;

    [Space]
    [SerializeField] private PassthroughController _passthroughController;

    [Space]
    public UnityEvent<GameMode> OnGameModeChanged;

    public float PlantsGrowthFrequency => _plantsGrowthFrequency;
    public float PlantsGrowthSpeed => CurrentGameMode switch
    {
        GameMode.Building => _buildingPlantsGrowthSpeed,
        GameMode.Gazing => _gazingPlantsGrowthSpeed,
        _ => _lobbyPlantsGrowthSpeed,
    };

    public float AwayPlantsGrowthSpeed => _awayPlantsGrowthSpeed;

    private Transform _cameraTransform;
    private Vector3 _lastCameraPosition, _lastLeftHandPosition, _lastRightHandPosition;
    private float _gazingTimer = 0f;
    private bool _gazingModeLocked = false;

#if UNITY_EDITOR
    private GameMode _lastValidatedGameMode = GameMode.Lobby;
#endif

    void Start()
    {
        _cameraTransform = Camera.main.transform;
        _lastCameraPosition = _cameraTransform.position;

        if (_leftHandAnchor != null)
        {
            _lastLeftHandPosition = _leftHandAnchor.position;
        }
        if (_righHandAnchor != null)
        {
            _lastRightHandPosition = _righHandAnchor.position;
        }
    }

    void Update()
    {
        if (CurrentGameMode == GameMode.Lobby || _gazingModeLocked) return;

        float deltaPos = _modeChange switch
        {
            ModeChange.PlayerPosition => Vector3.Distance(_lastCameraPosition, _cameraTransform.position),
            ModeChange.HandsPosition => (Vector3.Distance(_lastLeftHandPosition, _leftHandAnchor.position) + Vector3.Distance(_lastRightHandPosition, _righHandAnchor.position)) / 2f,
            _ => 0f,
        };

        if (deltaPos < _gazingPositionThreshold)
        {
            _gazingTimer += Time.deltaTime;
            if (_gazingTimer >= _gazingTimeThreshold) SetGazingMode();
        }
        else SetBuildingMode();

        _lastCameraPosition = _cameraTransform.position;

        if (_leftHandAnchor != null)
        {
            _lastLeftHandPosition = _leftHandAnchor.position;
        }
        if (_righHandAnchor != null)
        {
            _lastRightHandPosition = _righHandAnchor.position;
        }
    }

    /// <summary>
    /// Called by Room Manager's game object <see cref="Meta.XR.MRUtilityKit.MRUK"/> child component (see inspector).
    /// </summary>
    public void Initialize() => SetBuildingMode();

    public void SetBuildingMode(bool force = false)
    {
        if (!force && _gazingModeLocked) return;

        _gazingTimer = 0f;
        _gazingModeLocked = false;

        if (CurrentGameMode != GameMode.Building)
        {
            GameModeChanged(GameMode.Building);
        }
    }

    public void SetGazingMode(bool locked = false)
    {
        _gazingModeLocked = locked;

        if (CurrentGameMode != GameMode.Gazing)
        {
            GameModeChanged(GameMode.Gazing);
        }
    }

    private void GameModeChanged(GameMode mode)
    {
        if (mode == GameMode.Lobby) return;

        CurrentGameMode = mode;

        _passthroughController.SetLut((int)mode - 1, mode == GameMode.Building ? _buildingTransitionDuration : _gazingTransitionDuration);
        OnGameModeChanged?.Invoke(mode);
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
}
