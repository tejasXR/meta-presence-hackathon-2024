using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CeilingVfxController : MonoBehaviour
{
    [Header("Water VFX")]
    [SerializeField] private MeshRenderer _waterVfx;
    [SerializeField] private Color _deepWaterColorGazingMode = Color.black;
    [SerializeField] private Color _deepWaterColorBuildingMode = Color.white;

    [Header("Particles VFX")]
    [SerializeField] private ParticleSystem _dustVfx;
    [SerializeField] private float _particlesPerSquareMeter = 50;

    private const string DEEP_WATER_PROPERTY = "_DeepWater";

    private bool _initialized = false;
    private Coroutine _deepWaterColorInterpolationCoroutine;
    private Color _deepWaterColorCurrent;

    void Start()
    {
        _dustVfx.gameObject.SetActive(GameManager.Instance.CurrentGameMode == GameMode.Gazing);

        _deepWaterColorCurrent = GameManager.Instance.CurrentGameMode == GameMode.Gazing ? _deepWaterColorGazingMode : _deepWaterColorBuildingMode;
        _waterVfx.material.SetColor(DEEP_WATER_PROPERTY, _deepWaterColorCurrent);

        GameManager.Instance.OnGameModeChanged.AddListener(OnGameModeChanged);
    }

    private void OnGameModeChanged(GameMode mode)
    {
        if (!_initialized)
        {
            InitializeVfx();
            _initialized = true;
        }

        _dustVfx.gameObject.SetActive(mode == GameMode.Gazing);

        Color targetColor = mode == GameMode.Gazing ? _deepWaterColorGazingMode : _deepWaterColorBuildingMode;
        float transitionDuration = mode == GameMode.Gazing ? GameManager.Instance.GazingTransitionDuration : GameManager.Instance.BuildingTransitionDuration;

        StartInterpolation(
                    ref _deepWaterColorInterpolationCoroutine, _deepWaterColorCurrent, targetColor, transitionDuration,
                    color =>
                    {
                        _deepWaterColorCurrent = color;
                        _waterVfx.material.SetColor(DEEP_WATER_PROPERTY, _deepWaterColorCurrent);
                    });
    }

    private void InitializeVfx()
    {
        var shape = _dustVfx.shape;
        shape.scale = new Vector3(transform.localScale.x, transform.localScale.y, 0.01f);

        var main = _dustVfx.main;
        main.maxParticles = (int)(transform.localScale.x * transform.localScale.y * _particlesPerSquareMeter);
    }

    #region Interpolation

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IEnumerator InterpolateColor(Color startColor, Color targetColor, float duration, Action<Color> setValueAction)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            setValueAction(Color.Lerp(startColor, targetColor, Mathf.Clamp01(elapsedTime / duration)));

            yield return null;
        }

        setValueAction(targetColor);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void StartInterpolation(ref Coroutine coroutine, Color startColor, Color targetColor, float duration, Action<Color> setColorAction)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(InterpolateColor(startColor, targetColor, duration, setColorAction));
    }

    #endregion
}
