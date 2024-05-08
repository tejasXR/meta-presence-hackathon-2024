using System.Collections;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    [SerializeField] private AudioSource buildingModeAudioSource;
    [SerializeField] private AudioSource gazingModeAudioSource;
    [SerializeField] private float transitionTimeInSeconds = 2F;

    private float _buildingModeVolume;
    private float _gazingModeVolume;

    private IEnumerator _buildingModeMusicFadeRoutine;
    private IEnumerator _gazingModeMusicFadeRoutine;

    private void Awake()
    {
        _buildingModeVolume = buildingModeAudioSource.volume;
        _gazingModeVolume = gazingModeAudioSource.volume;
    }

    private void Start()
    {
        GameManager.Instance.OnGameModeChanged.AddListener(GameModeChanged);
        ChangeMusicMode(GameManager.Instance.CurrentGameMode, 0.001F);
    }

    private void GameModeChanged(GameMode gameMode) => ChangeMusicMode(gameMode, transitionTimeInSeconds);
    
    private void ChangeMusicMode(GameMode gameMode, float transitionTime)
    {
        if (_buildingModeMusicFadeRoutine != null)
            StopCoroutine(_buildingModeMusicFadeRoutine);

        if (_gazingModeMusicFadeRoutine != null)
            StopCoroutine(_gazingModeMusicFadeRoutine);

        var buildModeVolume = gameMode is GameMode.Lobby or GameMode.Building ? _buildingModeVolume : 0;
        var gazeModeVolume = gameMode is GameMode.Gazing ? _gazingModeVolume : 0;
        
        _buildingModeMusicFadeRoutine = AudioUtils.FadeToVolume(buildingModeAudioSource, buildModeVolume, transitionTime);
        _gazingModeMusicFadeRoutine = AudioUtils.FadeToVolume(gazingModeAudioSource, gazeModeVolume, transitionTime);
        
        StartCoroutine(_buildingModeMusicFadeRoutine);
        StartCoroutine(_gazingModeMusicFadeRoutine);
    }
}
