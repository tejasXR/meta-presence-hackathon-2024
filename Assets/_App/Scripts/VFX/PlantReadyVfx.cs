using UnityEngine;

public class PlantReadyVfx : MonoBehaviour
{
    
    [SerializeField] private ParticleSystem plantReadyParticles;
    [SerializeField] private ParticleSystem seedFormingParticles;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color hoverColor;

    private PlantController _plant;

    private void Awake()
    {
        _plant = GetComponentInParent<PlantController>();
        _plant.PlantReadyToBeHarvested.AddListener(OnPlantReadyToBeHarvested);
        _plant.PlantChargingUp.AddListener(OnPlantCharging);
        _plant.PlantChargingDown.AddListener(OnPlantDefault);
        _plant.PlantGlowingBeforeSeedSpawn.AddListener(OnPlantGlowing);
        _plant.SeedSpawningTriggered.AddListener(OnSeedSpawned);
    }

    private void OnDestroy()
    {
        if (_plant)
        {
            _plant.PlantReadyToBeHarvested.RemoveListener(OnPlantReadyToBeHarvested);
            _plant.PlantChargingUp.RemoveListener(OnPlantCharging);
            _plant.PlantChargingDown.RemoveListener(OnPlantDefault);
            _plant.PlantGlowingBeforeSeedSpawn.RemoveListener(OnPlantGlowing);
            _plant.SeedSpawningTriggered.RemoveListener(OnSeedSpawned);
        }
    }


    private void OnPlantReadyToBeHarvested()
    {
        if (!plantReadyParticles.isPlaying)
            plantReadyParticles.Play();
    }

    private void OnPlantDefault()
    {
        ChangePlantReadyParticlesStartColor(defaultColor);
    }
    
    private void OnPlantCharging()
    {
        ChangePlantReadyParticlesStartColor(hoverColor);
    }

    private void OnPlantGlowing()
    {
        seedFormingParticles.Play();
    }

    private void OnSeedSpawned(PlantController plantController)
    {
        seedFormingParticles.Stop();
        plantReadyParticles.Stop();
    }

    private void ChangePlantReadyParticlesStartColor(Color startColor)
    {
        var main = plantReadyParticles.main;
        main.startColor = startColor;
    }
}
