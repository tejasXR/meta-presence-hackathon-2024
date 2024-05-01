using System;
using Oculus.Interaction;
using UnityEngine;

public class NpcOptionsMenu : MonoBehaviour
{
    [SerializeField] private GameObject menuContainer;
    [SerializeField] private MenuOptionPositions leftMenuPositions;
    [SerializeField] private MenuOptionPositions rightMenuPositions;

    private PokeInteractable[] _pokeInteractables;

    private void Awake()
    {
        _pokeInteractables = menuContainer.GetComponentsInChildren<PokeInteractable>();
    }

    public void Show(NpcCaller.PoseOrientation poseOrientation)
    {
        menuContainer.SetActive(true);
        var positions = poseOrientation == NpcCaller.PoseOrientation.LeftHand ? leftMenuPositions : rightMenuPositions;
        PositionMenuOptions(positions);
    }

    public void Hide()
    {
        menuContainer.SetActive(false);
    }

    private void PositionMenuOptions(MenuOptionPositions menuOptionPositions)
    {
        if (menuOptionPositions.optionPositions.Length != _pokeInteractables.Length)
        {
            throw new ApplicationException(
                "There are more defined menu option positions then there are menu options! Aborting function!");
        }

        for (int i = 0; i < menuOptionPositions.optionPositions.Length; i++)
        {
            _pokeInteractables[i].transform.position = menuOptionPositions.optionPositions[i].position;
        }
    }
}

[Serializable]
public struct MenuOptionPositions
{
    public Transform[] optionPositions;
}
