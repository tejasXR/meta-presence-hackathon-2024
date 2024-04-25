using System;
using Oculus.Interaction;
using UnityEngine;

public class DebugGestureSelectorState : MonoBehaviour
{
   [SerializeField] private ActiveStateSelector activeStateSelector;

   private void Awake()
   {
      activeStateSelector.WhenSelected += GestureActive;
      activeStateSelector.WhenUnselected += GestureInactive;
   }

   private void OnDestroy()
   {
      if (activeStateSelector)
      {
         activeStateSelector.WhenSelected -= GestureActive;
         activeStateSelector.WhenUnselected -= GestureInactive;
      }
   }

   private void GestureActive()
   {
      Debug.Log($"Gesture for selector {nameof(activeStateSelector)} is ACTIVE");
   }

   private void GestureInactive()
   {
      Debug.Log($"Gesture for selector {nameof(activeStateSelector)} is INACTIVE");
   }
}
