using System.Collections;
using UnityEngine;

public class PlantAudio : MonoBehaviour
{
    [SerializeField] private PlantController plantController;
    [Space]
    [SerializeField] private AudioSource plantGrowthAudioSource;
    [SerializeField] private AudioSource plantChargeUpAudioSource;
    [SerializeField] private AudioSource plantSpawnSeedsAudioSource;

    private IEnumerator _plantChargingRoutine;
    
    private float _startingPlantChargeVolume;
    private float _startingPlantGrowthVolume;
    
    private void Awake()
    {
        plantController.PlantChargingUp.AddListener(OnPlantChargingUp);
        plantController.PlantChargingDown.AddListener(OnPlantChargeDown);
        plantController.SeedSpawningTriggered.AddListener(OnSeedSpawnedFromPlant);

        _startingPlantChargeVolume = plantChargeUpAudioSource.volume;
        _startingPlantGrowthVolume = plantGrowthAudioSource.volume;

        plantChargeUpAudioSource.volume = 0;
        plantGrowthAudioSource.volume = 0;
    }

    private void OnDestroy()
    {
        if (plantController)
        {
            plantController.PlantChargingUp.RemoveListener(OnPlantChargingUp);
            plantController.PlantChargingDown.RemoveListener(OnPlantChargeDown);
            plantController.SeedSpawningTriggered.RemoveListener(OnSeedSpawnedFromPlant);
        }
    }

    private void Update()
    {
        plantGrowthAudioSource.volume = _startingPlantGrowthVolume * plantController.BasePlantGrowth;
    }

    private void OnPlantChargingUp()
    {
        if (!plantChargeUpAudioSource.isPlaying)
            plantChargeUpAudioSource.Play();
        
        ChangePlantChargeAudioVolume(_startingPlantChargeVolume);
    }

    private void OnPlantChargeDown()
    {
        ChangePlantChargeAudioVolume(0);
    }

    private void ChangePlantChargeAudioVolume(float volume)
    {
        if (_plantChargingRoutine != null)
            StopCoroutine(_plantChargingRoutine);
        
        _plantChargingRoutine = AudioUtils.FadeToVolume(plantChargeUpAudioSource, volume, .8F);
        StartCoroutine(_plantChargingRoutine);
        
        Debug.Log( $"[{nameof(PlantAudio)}] {nameof(ChangePlantChargeAudioVolume)}: Volume = {volume}");
    }

    private void OnSeedSpawnedFromPlant(PlantController _)
    {
        if (_plantChargingRoutine != null)
            StopCoroutine(_plantChargingRoutine);
        
        _plantChargingRoutine = AudioUtils.FadeToVolume(plantChargeUpAudioSource, 0, .2F, true);
        StartCoroutine(_plantChargingRoutine);
        
        plantSpawnSeedsAudioSource.Play();
    }
}
