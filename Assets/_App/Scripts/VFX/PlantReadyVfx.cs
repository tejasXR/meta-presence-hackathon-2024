using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantReadyVfx : MonoBehaviour
{
    [SerializeField] private ParticleSystem plantReadyParticles;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color hoverColor;

    public void StartParticles()
    {
        if (!plantReadyParticles.isPlaying)
            plantReadyParticles.Play();
    }

    public void StopParticles()
    {
        if (plantReadyParticles.isPlaying)
            plantReadyParticles.Stop();
    } 

    public void DefaultState()
    {
        ChangeStartColor(defaultColor);
    }
    
    public void HoverState()
    {
        ChangeStartColor(hoverColor);
    }

    private void ChangeStartColor(Color startColor)
    {
        var main = plantReadyParticles.main;
        main.startColor = startColor;
    }

}
