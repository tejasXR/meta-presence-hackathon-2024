using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class IslandController : MonoBehaviour
{
    public Islands.IslandType Type;
    public Transform OriginSpawnPoint;

    [SerializeField] private MeshRenderer _meshRenderer;

    [Header("Island Emerging Effect")]
    [SerializeField] private float _emergeSpeed = 1f;
    [SerializeField] private bool _rumble = true;
    [SerializeField] private float _rumbleSpeed = 100f;
    [SerializeField] private float _rumbleIntensity = 0.5f;

    private Action _onEmergingCompleted;
    private Vector3 _originalLocalPosition;
    private bool _isEmerging = false;

    void Awake()
    {
        _originalLocalPosition = _meshRenderer.transform.localPosition;
        _meshRenderer.enabled = false;
    }

    void OnDestroy()
    {
        _onEmergingCompleted = null;
    }

    void Update()
    {
        if (_isEmerging)
        {
            Vector3 noise = Vector3.zero;
            if (_rumble)
            {
                float intensity = Mathf.Clamp01(Mathf.Abs(_meshRenderer.transform.localPosition.y - _originalLocalPosition.y)); // Intensity reduces as the island gets closer to its origin.
                float xNoise = Mathf.PerlinNoise(_rumbleSpeed * Time.time, 0);
                float zNoise = Mathf.PerlinNoise(0f, _rumbleSpeed * Time.time);

                noise.x = Random.Range(-intensity, intensity) * xNoise;
                noise.z = Random.Range(-intensity, intensity) * zNoise;
            }
            _meshRenderer.transform.localPosition = Vector3.Lerp(_meshRenderer.transform.localPosition, new(noise.x * _rumbleIntensity, _originalLocalPosition.y, noise.z * _rumbleIntensity), _emergeSpeed * Time.deltaTime);

            _isEmerging = Mathf.Abs(_meshRenderer.transform.localPosition.y - _originalLocalPosition.y) > 0.001f;
            if (!_isEmerging)
            {
                Debug.Log($"[{nameof(IslandController)}] {nameof(Update)}: Emerging completed");
                _meshRenderer.transform.localPosition = _originalLocalPosition;

                _onEmergingCompleted?.Invoke();
                _onEmergingCompleted = null;
            }
        }
    }

    public void StartEmerging(Action onEmergingCompleted)
    {
        _onEmergingCompleted = onEmergingCompleted;

        _meshRenderer.transform.localPosition = new(0f, -_meshRenderer.bounds.size.y, 0f);
        _meshRenderer.enabled = true;

        _isEmerging = true;
    }

    public void SetEmerged()
    {
        _meshRenderer.enabled = true;
    }

#if UNITY_EDITOR
    [Sirenix.OdinInspector.Button]
    public void TestStartEmerging()
    {
        if (Application.isPlaying)
        {
            StartEmerging(null);
        }
    }

    [Sirenix.OdinInspector.Button]
    public void TestSetEmerged()
    {
        if (Application.isPlaying)
        {
            SetEmerged();
        }
    }

    [Sirenix.OdinInspector.Button]
    public void Reset()
    {
        if (Application.isPlaying)
        {
            _meshRenderer.enabled = false;
            _meshRenderer.transform.localPosition = new(0f, -_meshRenderer.bounds.size.y, 0f);
        }
    }
#endif
}
