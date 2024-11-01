using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PassthroughController : MonoBehaviour
{
    [SerializeField] private OVRPassthroughLayer _ovrPassthroughLayer;
    [SerializeField] private List<Texture2D> _lutTextures;

    private readonly Dictionary<int, OVRPassthroughColorLut> _lutDictionary = new();

    private Coroutine _lutCoroutine;

    private OVRPassthroughColorLut _currentLut;

    void Awake()
    {
        if (_ovrPassthroughLayer == null)
        {
            _ovrPassthroughLayer = FindObjectOfType<OVRPassthroughLayer>();
        }
    }

    void Start()
    {
        for (int i = 0; i < _lutTextures.Count; i++)
        {
            Texture2D texture = _lutTextures[i];

            if (!OVRPassthroughColorLut.IsTextureSupported(texture, out var errorMessage))
            {
                Debug.LogWarning($"[{nameof(PassthroughController)}] {nameof(Start)}: LUT texture not supported: index={i}, name={texture.name}, {nameof(errorMessage)}={errorMessage}");
            }
            else
            {
                _lutDictionary[i] = new OVRPassthroughColorLut(texture);

                // Initialize color LUT
                _currentLut ??= _lutDictionary[i];
            }
        }
        _ovrPassthroughLayer.SetColorLut(_currentLut);
    }

    void OnDestroy()
    {
        if (_ovrPassthroughLayer != null)
        {
            _ovrPassthroughLayer.DisableColorMap();
        }
    }

    public void SetLut(int lutId, float transitionDuration = 1f, float targetWeight = 1f, bool interpolate = true)
    {
        if (_ovrPassthroughLayer == null)
        {
            Debug.LogError($"[{nameof(PassthroughController)}] {nameof(SetLut)}: Error: {nameof(OVRPassthroughLayer)} is null.");
            return;
        }

        if (_lutDictionary.TryGetValue(lutId, out OVRPassthroughColorLut lut))
        {
            OVRPassthroughColorLut previousLut = _currentLut;
            _currentLut = lut;

            if (!interpolate)
            {
                // Debug.Log($"[{nameof(PassthroughController)}] {nameof(SetLut)}: Setting LUT with {nameof(lutId)}={lutId}, {nameof(targetWeight)}={targetWeight}");
                _ovrPassthroughLayer.SetColorLut(previousLut, _currentLut, targetWeight);
            }
            else
            {
                StartInterpolation(
                    ref _lutCoroutine, 0f, targetWeight, transitionDuration,
                    weight =>
                    {
                        // Debug.Log($"[{nameof(PassthroughController)}] {nameof(SetLut)}: Setting LUT with {nameof(lutId)}={lutId}, {nameof(targetWeight)}={targetWeight}, {nameof(weight)}={weight}");
                        _ovrPassthroughLayer.SetColorLut(previousLut, _currentLut, weight);
                    });
            }
        }
        else
        {
            Debug.LogWarning($"[{nameof(PassthroughController)}] {nameof(SetLut)}: Couldn't find LUT with {nameof(lutId)}={lutId}");
        }
    }

    private void Reset()
    {
        SetLut(0, 1f, interpolate: false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void StartInterpolation(ref Coroutine coroutine, float startValue, float targetValue, float duration, Action<float> setValueAction)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(InterpolationUtils.InterpolateValue(startValue, targetValue, duration, setValueAction));
    }
}
