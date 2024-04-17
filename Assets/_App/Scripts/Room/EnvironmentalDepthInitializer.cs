using Unity.XR.Oculus;
using UnityEngine;

public class EnvironmentalDepthInitializer : MonoBehaviour
{
    private void Start()
    {
        var supportedHeadsets = OVRPlugin.SystemHeadset.Meta_Link_Quest_3 | OVRPlugin.SystemHeadset.Meta_Quest_3;
        if (OVRPlugin.GetSystemHeadsetType() == supportedHeadsets)
        {
            var environmentalDepthParams = new Utils.EnvironmentDepthCreateParams()
            {
                removeHands = true
            };
            
            Utils.SetupEnvironmentDepth(environmentalDepthParams);
            Utils.SetEnvironmentDepthRendering(true);
            Debug.LogError("Environmental depth initialized");
        }
        else
        {
            Debug.LogError("Environmental depth is not initialized because it is not supported on this device.");
        }
    }

    private void OnDestroy()
    {
        Utils.ShutdownEnvironmentDepth();
    }
}
