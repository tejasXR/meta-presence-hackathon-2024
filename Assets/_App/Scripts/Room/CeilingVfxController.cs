using System.Collections;
using UnityEngine;

public class CeilingVfxController : MonoBehaviour
{
    [SerializeField] private ParticleSystem _dustVfx;

    void Start()
    {
        StartCoroutine(InitializeDustVfxShape());

        GameManager.Instance.OnGameModeChanged.AddListener(OnGameModeChanged);
        OnGameModeChanged(GameManager.Instance.CurrentGameMode);
    }

    private void OnGameModeChanged(GameMode mode)
    {
        _dustVfx.gameObject.SetActive(mode == GameMode.Gazing);
    }

    private IEnumerator InitializeDustVfxShape()
    {
        // TODO(yola): Figure out a way to access the ceiling mesh to use as particle system shape.
        yield return null;
    }
}
