using UnityEngine;

public class PokeInteractableGameModeController : MonoBehaviour
{
    [SerializeField] private string _gazingModeLockedString;
    [SerializeField] private string _buildingModeUnlockedString;

    [Space]
    [SerializeField] private TMPro.TMP_Text _buttonText;

    void Start()
    {
        OnGameModeChanged(GameManager.Instance.CurrentGameMode);
        GameManager.Instance.OnGameModeChanged.AddListener(OnGameModeChanged);
    }

    public void WhenSelect()
    {
        switch (GameManager.Instance.CurrentGameMode)
        {
            case GameMode.Building:
                GameManager.Instance.SetGazingMode(locked: true);
                break;
            case GameMode.Gazing:
                GameManager.Instance.SetBuildingMode();
                break;
        }
    }

    private void OnGameModeChanged(GameMode mode)
    {
        _buttonText.text = GameManager.Instance.CurrentGameMode switch
        {
            GameMode.Building => _gazingModeLockedString,
            GameMode.Gazing => _buildingModeUnlockedString,
            _ => "",
        };
    }
}
