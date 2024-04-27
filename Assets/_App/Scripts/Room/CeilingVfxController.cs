using UnityEngine;

public class CeilingVfxController : MonoBehaviour
{
    [SerializeField] private ParticleSystem _dustVfx;
    [SerializeField] private float _particlesPerSquareMeter = 50;

    private bool _initialized = false;

    void Start()
    {
        _dustVfx.gameObject.SetActive(GameManager.Instance.CurrentGameMode == GameMode.Gazing);
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
    }

    private void InitializeVfx()
    {
        var shape = _dustVfx.shape;
        shape.scale = new Vector3(transform.localScale.x, transform.localScale.y, 0.01f);

        var main = _dustVfx.main;
        main.maxParticles = (int)(transform.localScale.x * transform.localScale.y * _particlesPerSquareMeter);
    }
}
