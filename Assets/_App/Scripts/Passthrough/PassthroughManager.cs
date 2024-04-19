using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PassthroughManager : MonoBehaviour
{
    [SerializeField] private OVRPassthroughLayer _passthroughLayer;
    [SerializeField] private List<Texture2D> _lutTextures;

    [SerializeField] private float _opacity = 1f;
    [SerializeField] private float _contrast = 0f;
    [SerializeField] private float _brightness = 0f;
    [SerializeField] private float _saturation = 0f;

    [SerializeField] private int _lutId = 0;
    [SerializeField] private float _lutBlend = 0f;

    [SerializeField] private float _opacityTransitionDuration = 1f;
    [SerializeField] private float _contrastTransitionDuration = 1f;
    [SerializeField] private float _brightnessTransitionDuration = 1f;
    [SerializeField] private float _saturationTransitionDuration = 1f;
    [SerializeField] private float _lutTransitionDuration = 1f;

    private readonly Dictionary<int, OVRPassthroughColorLut> _lutDictionary = new();
    private OVRPassthroughColorLut _currentColorLut;
    private float _currentBlend;

    private Coroutine _opacityCoroutine;
    private Coroutine _contrastCoroutine;
    private Coroutine _brightnessCoroutine;
    private Coroutine _saturationCoroutine;
    private Coroutine _lutCoroutine;

    public void Awake()
    {
        if (_passthroughLayer == null)
        {
            _passthroughLayer = FindObjectOfType<OVRPassthroughLayer>();
        }
    }

    private void Start()
    {
        // Initialize passthrough with desired settings.
        UpdatePassthroughSetup();

        // Ensure the textures are supported for the LUT
        for (int i = 0; i < _lutTextures.Count; i++)
        {
            Texture2D texture = _lutTextures[i];

            if (!OVRPassthroughColorLut.IsTextureSupported(texture, out var errorMessage))
            {
                Debug.LogError($"[{nameof(PassthroughManager)}] {nameof(Start)}: LUT texture not supported: index={i}, name={texture.name}, {nameof(errorMessage)}={errorMessage}");
            }
            else
            {
                _lutDictionary[i] = new OVRPassthroughColorLut(texture);
            }
        }
    }

    private void OnDisable()
    {
        SetLut(0, 1f);
    }

    private void SetPassthroughColorLut(OVRPassthroughColorLut lut)
    {
        _passthroughLayer.SetColorLut(lut);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdatePassthroughSetup()
    {
        _passthroughLayer.SetBrightnessContrastSaturation(_brightness, _contrast, _saturation);
        _passthroughLayer.textureOpacity = _opacity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IEnumerator InterpolateValue(float startValue, float targetValue, float duration, Action<float> setValueAction)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / duration);
            setValueAction(Mathf.Lerp(startValue, targetValue, t));
            yield return null;
        }

        setValueAction(targetValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void StartInterpolation(ref Coroutine coroutine, float startValue, float targetValue, float duration, Action<float> setValueAction)
    {
        if (!_passthroughLayer)
        {
            Debug.LogError("No passthrough layer specifier for PassthroughConfigurator." +
                           "Unable to update passthrough setup!");
            return;
        }

        // If there's already an interpolation coroutine for the variable, stop it.
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }

        // Start the new interpolation coroutine.
        coroutine = StartCoroutine(InterpolateValue(startValue, targetValue, duration, setValueAction));
    }

    public void ResetPassthrough()
    {
        if (!_passthroughLayer)
        {
            Debug.LogError("No passthrough layer specifier for PassthroughConfigurator." +
                           "Unable to reset passthrough!");
            return;
        }
        _passthroughLayer.DisableColorMap();
    }

    public void SetOpacity(float opacity)
    {
        if (!_passthroughLayer)
        {
            Debug.LogError("No passthrough layer specifier for PassthroughConfigurator." +
                           "Unable to change opacity!");
            return;
        }

        // Smooth change of opacity
        StartInterpolation(ref _opacityCoroutine, _opacity, opacity, _opacityTransitionDuration,
        value =>
        {
            _opacity = value;
            UpdatePassthroughSetup();
        });
    }

    public void SetContrast(float contrast)
    {
        // Smooth change of contrast
        StartInterpolation(ref _contrastCoroutine, _contrast, contrast, _contrastTransitionDuration,
        value =>
        {
            _contrast = value;
            UpdatePassthroughSetup();
        });
    }

    public void SetBrightness(float brightness)
    {
        // Smooth change of brightness
        StartInterpolation(ref _brightnessCoroutine, _brightness, brightness, _brightnessTransitionDuration,
        value =>
        {
            _brightness = value;
            UpdatePassthroughSetup();
        });
    }

    public void SetSaturation(float saturation)
    {
        // Smooth change of saturation
        StartInterpolation(ref _saturationCoroutine, _saturation, saturation, _saturationTransitionDuration,
        value =>
        {
            _saturation = value;
            UpdatePassthroughSetup();
        });
    }

    public void SetLut(int lutId, float targetBlend)
    {
        if (_lutDictionary.TryGetValue(lutId, out OVRPassthroughColorLut lut))
        {
            _lutId = lutId;
            _currentColorLut = lut;
            _lutBlend = 0;

            // Smooth change of Lut, going from current blend to target blend from previous to next LUT
            StartInterpolation(
                ref _lutCoroutine, _lutBlend, targetBlend, _lutTransitionDuration,
                value =>
                {
                    _lutBlend = value;
                    SetPassthroughColorLut(lut);
                });
        }
    }

#if UNITY_EDITOR
    [Sirenix.OdinInspector.LabelText("$CurrentLut")]
    public string CurrentLut;

    [Sirenix.OdinInspector.Button("Set Previous Color LUT")]
    public void SetPreviousLut()
    {
        _lutId = (_lutId - 1) % _lutTextures.Count;
        while (!_lutDictionary.ContainsKey(_lutId))
        {
            Debug.LogError($"[{nameof(PassthroughManager)}] {nameof(SetNextLut)}: id={_lutId} not supported");
            _lutId = (_lutId - 1) % _lutTextures.Count;
        }
        CurrentLut = $"[{_lutId}] {_lutTextures[_lutId].name}";
        SetLut(_lutId, _lutBlend);
    }

    [Sirenix.OdinInspector.Button("Set Next Color LUT")]
    public void SetNextLut()
    {
        _lutId = (_lutId + 1) % _lutTextures.Count;
        while (!_lutDictionary.ContainsKey(_lutId))
        {
            Debug.LogError($"[{nameof(PassthroughManager)}] {nameof(SetNextLut)}: id={_lutId} not supported");
            _lutId = (_lutId + 1) % _lutTextures.Count;
        }
        CurrentLut = $"[{_lutId}] {_lutTextures[_lutId].name}";
        SetLut(_lutId, 1f);
    }
#endif
}
