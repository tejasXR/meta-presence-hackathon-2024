using UnityEngine;
using UnityEngine.Events;

public enum GameMode { Lobby, Building, Gazing }

public class GameManager : Singleton<GameManager>
{
    public GameMode CurrentGameMode = GameMode.Lobby;

    [Header("Gazing Mode Config")]
    [SerializeField] private float _gazingPositionThreshold = 0.01f;
    [SerializeField] private float _gazingTimeThreshold = 3f;

    [Header(header: "Plants Growth Config")]
    [SerializeField] private float _plantsGrowthFrequency = 0.01f;

    [SerializeField] private float _awayPlantsGrowthSpeed = 0.25f;
    [SerializeField] private float _lobbyPlantsGrowthSpeed = 0f;
    [SerializeField] private float _buildingPlantsGrowthSpeed = 1f;
    [SerializeField] private float _gazingPlantsGrowthSpeed = 10f;

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
    private Vector3 _lastCameraPosition;
    private float _gazingTimer = 0f;

#if UNITY_EDITOR
    private GameMode _lastValidatedGameMode = GameMode.Lobby;
#endif

    void Start()
    {
        _cameraTransform = Camera.main.transform;
        _lastCameraPosition = _cameraTransform.position;
    }

    void Update()
    {
        if (CurrentGameMode == GameMode.Lobby)
        {
            return;
        }

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

    /// <summary>
    /// Called by Room Manager's game object <see cref="Meta.XR.MRUtilityKit.MRUKStart"/> component (see inspector).
    /// </summary>
    public void Initialize() => GameModeChanged(GameMode.Building);

    private void GameModeChanged(GameMode mode)
    {
        CurrentGameMode = mode;

        _passthroughController.SetLut((int)mode - 1);
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