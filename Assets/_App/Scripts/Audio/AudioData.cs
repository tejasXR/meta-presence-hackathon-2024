using System;
using UnityEngine;

[Serializable]
public struct AudioData
{
    public AudioClip[] Clips;
    [Range(0F, 1F)] public float Volume;
}
