using UnityEngine;

public static class OVRSkeletonUtil
{
    /// <summary>
    /// <see cref="OVRSkeleton.InitializeCapsules"/>, line 588
    /// </summary>
    private const string OVR_SKELETON_CAPSULE_COLLIDER_SUFFIX = "_CapsuleCollider";

    public static bool BelongsToOVRSkeleton(this Collider collider, out OVRSkeleton ovrSkeleton)
    {
        ovrSkeleton = collider.GetComponentInParent<OVRSkeleton>();
        return ovrSkeleton != null;
    }

    public static OVRBone GetOVRBone(this Collider collider, OVRSkeleton ovrSkeleton)
    {
        Debug.Assert(ovrSkeleton != null, $"[{nameof(OVRSkeletonUtil)}] {nameof(GetOVRBone)} failed: {nameof(OVRSkeleton)} is null");

        // Find bone ID.
        OVRSkeleton.BoneId boneId = BoneIdFromBoneLabel(
            skeletonType: ovrSkeleton.GetSkeletonType(),
            boneId: collider.gameObject.name.Replace(OVR_SKELETON_CAPSULE_COLLIDER_SUFFIX, ""));

        if (boneId == OVRSkeleton.BoneId.Invalid)
        {
            Debug.LogError($"[{nameof(OVRSkeletonUtil)}] {nameof(GetOVRBone)} failed: {nameof(OVRSkeleton.BoneId)} is invalid.");
            return null;
        }

        // Find bone.
        foreach (var bone in ovrSkeleton.Bones)
        {
            if (boneId.ToString() == bone.Id.ToString()) // Use name comparison because a lot of enum types have the same value.
            {
                return bone;
            }
        }

        return null;
    }

    public static bool BelongsToOVRSkeleton(this Collision collision, out OVRSkeleton ovrSkeleton) => collision.collider.BelongsToOVRSkeleton(out ovrSkeleton);

    public static OVRBone GetOVRBone(this Collision collision, OVRSkeleton ovrSkeleton) => collision.collider.GetOVRBone(ovrSkeleton);

    /// <summary>
    /// Reverse <see cref="OVRSkeleton.BoneLabelFromBoneId(OVRSkeleton.SkeletonType, OVRSkeleton.BoneId)"/>.
    /// </summary>
    private static OVRSkeleton.BoneId BoneIdFromBoneLabel(OVRSkeleton.SkeletonType skeletonType, string boneId)
    {
        if (skeletonType == OVRSkeleton.SkeletonType.Body)
        {
            return boneId switch
            {
                "Body_Root" => OVRSkeleton.BoneId.Body_Root,
                "Body_Hips" => OVRSkeleton.BoneId.Body_Hips,
                "Body_SpineLower" => OVRSkeleton.BoneId.Body_SpineLower,
                "Body_SpineMiddle" => OVRSkeleton.BoneId.Body_SpineMiddle,
                "Body_SpineUpper" => OVRSkeleton.BoneId.Body_SpineUpper,
                "Body_Chest" => OVRSkeleton.BoneId.Body_Chest,
                "Body_Neck" => OVRSkeleton.BoneId.Body_Neck,
                "Body_Head" => OVRSkeleton.BoneId.Body_Head,
                "Body_LeftShoulder" => OVRSkeleton.BoneId.Body_LeftShoulder,
                "Body_LeftScapula" => OVRSkeleton.BoneId.Body_LeftScapula,
                "Body_LeftArmUpper" => OVRSkeleton.BoneId.Body_LeftArmUpper,
                "Body_LeftArmLower" => OVRSkeleton.BoneId.Body_LeftArmLower,
                "Body_LeftHandWristTwist" => OVRSkeleton.BoneId.Body_LeftHandWristTwist,
                "Body_RightShoulder" => OVRSkeleton.BoneId.Body_RightShoulder,
                "Body_RightScapula" => OVRSkeleton.BoneId.Body_RightScapula,
                "Body_RightArmUpper" => OVRSkeleton.BoneId.Body_RightArmUpper,
                "Body_RightArmLower" => OVRSkeleton.BoneId.Body_RightArmLower,
                "Body_RightHandWristTwist" => OVRSkeleton.BoneId.Body_RightHandWristTwist,
                "Body_LeftHandPalm" => OVRSkeleton.BoneId.Body_LeftHandPalm,
                "Body_LeftHandWrist" => OVRSkeleton.BoneId.Body_LeftHandWrist,
                "Body_LeftHandThumbMetacarpal" => OVRSkeleton.BoneId.Body_LeftHandThumbMetacarpal,
                "Body_LeftHandThumbProximal" => OVRSkeleton.BoneId.Body_LeftHandThumbProximal,
                "Body_LeftHandThumbDistal" => OVRSkeleton.BoneId.Body_LeftHandThumbDistal,
                "Body_LeftHandThumbTip" => OVRSkeleton.BoneId.Body_LeftHandThumbTip,
                "Body_LeftHandIndexMetacarpal" => OVRSkeleton.BoneId.Body_LeftHandIndexMetacarpal,
                "Body_LeftHandIndexProximal" => OVRSkeleton.BoneId.Body_LeftHandIndexProximal,
                "Body_LeftHandIndexIntermediate" => OVRSkeleton.BoneId.Body_LeftHandIndexIntermediate,
                "Body_LeftHandIndexDistal" => OVRSkeleton.BoneId.Body_LeftHandIndexDistal,
                "Body_LeftHandIndexTip" => OVRSkeleton.BoneId.Body_LeftHandIndexTip,
                "Body_LeftHandMiddleMetacarpal" => OVRSkeleton.BoneId.Body_LeftHandMiddleMetacarpal,
                "Body_LeftHandMiddleProximal" => OVRSkeleton.BoneId.Body_LeftHandMiddleProximal,
                "Body_LeftHandMiddleIntermediate" => OVRSkeleton.BoneId.Body_LeftHandMiddleIntermediate,
                "Body_LeftHandMiddleDistal" => OVRSkeleton.BoneId.Body_LeftHandMiddleDistal,
                "Body_LeftHandMiddleTip" => OVRSkeleton.BoneId.Body_LeftHandMiddleTip,
                "Body_LeftHandRingMetacarpal" => OVRSkeleton.BoneId.Body_LeftHandRingMetacarpal,
                "Body_LeftHandRingProximal" => OVRSkeleton.BoneId.Body_LeftHandRingProximal,
                "Body_LeftHandRingIntermediate" => OVRSkeleton.BoneId.Body_LeftHandRingIntermediate,
                "Body_LeftHandRingDistal" => OVRSkeleton.BoneId.Body_LeftHandRingDistal,
                "Body_LeftHandRingTip" => OVRSkeleton.BoneId.Body_LeftHandRingTip,
                "Body_LeftHandLittleMetacarpal" => OVRSkeleton.BoneId.Body_LeftHandLittleMetacarpal,
                "Body_LeftHandLittleProximal" => OVRSkeleton.BoneId.Body_LeftHandLittleProximal,
                "Body_LeftHandLittleIntermediate" => OVRSkeleton.BoneId.Body_LeftHandLittleIntermediate,
                "Body_LeftHandLittleDistal" => OVRSkeleton.BoneId.Body_LeftHandLittleDistal,
                "Body_LeftHandLittleTip" => OVRSkeleton.BoneId.Body_LeftHandLittleTip,
                "Body_RightHandPalm" => OVRSkeleton.BoneId.Body_RightHandPalm,
                "Body_RightHandWrist" => OVRSkeleton.BoneId.Body_RightHandWrist,
                "Body_RightHandThumbMetacarpal" => OVRSkeleton.BoneId.Body_RightHandThumbMetacarpal,
                "Body_RightHandThumbProximal" => OVRSkeleton.BoneId.Body_RightHandThumbProximal,
                "Body_RightHandThumbDistal" => OVRSkeleton.BoneId.Body_RightHandThumbDistal,
                "Body_RightHandThumbTip" => OVRSkeleton.BoneId.Body_RightHandThumbTip,
                "Body_RightHandIndexMetacarpal" => OVRSkeleton.BoneId.Body_RightHandIndexMetacarpal,
                "Body_RightHandIndexProximal" => OVRSkeleton.BoneId.Body_RightHandIndexProximal,
                "Body_RightHandIndexIntermediate" => OVRSkeleton.BoneId.Body_RightHandIndexIntermediate,
                "Body_RightHandIndexDistal" => OVRSkeleton.BoneId.Body_RightHandIndexDistal,
                "Body_RightHandIndexTip" => OVRSkeleton.BoneId.Body_RightHandIndexTip,
                "Body_RightHandMiddleMetacarpal" => OVRSkeleton.BoneId.Body_RightHandMiddleMetacarpal,
                "Body_RightHandMiddleProximal" => OVRSkeleton.BoneId.Body_RightHandMiddleProximal,
                "Body_RightHandMiddleIntermediate" => OVRSkeleton.BoneId.Body_RightHandMiddleIntermediate,
                "Body_RightHandMiddleDistal" => OVRSkeleton.BoneId.Body_RightHandMiddleDistal,
                "Body_RightHandMiddleTip" => OVRSkeleton.BoneId.Body_RightHandMiddleTip,
                "Body_RightHandRingMetacarpal" => OVRSkeleton.BoneId.Body_RightHandRingMetacarpal,
                "Body_RightHandRingProximal" => OVRSkeleton.BoneId.Body_RightHandRingProximal,
                "Body_RightHandRingIntermediate" => OVRSkeleton.BoneId.Body_RightHandRingIntermediate,
                "Body_RightHandRingDistal" => OVRSkeleton.BoneId.Body_RightHandRingDistal,
                "Body_RightHandRingTip" => OVRSkeleton.BoneId.Body_RightHandRingTip,
                "Body_RightHandLittleMetacarpal" => OVRSkeleton.BoneId.Body_RightHandLittleMetacarpal,
                "Body_RightHandLittleProximal" => OVRSkeleton.BoneId.Body_RightHandLittleProximal,
                "Body_RightHandLittleIntermediate" => OVRSkeleton.BoneId.Body_RightHandLittleIntermediate,
                "Body_RightHandLittleDistal" => OVRSkeleton.BoneId.Body_RightHandLittleDistal,
                "Body_RightHandLittleTip" => OVRSkeleton.BoneId.Body_RightHandLittleTip,
                _ => OVRSkeleton.BoneId.Invalid
            };
        }
        else if (skeletonType == OVRSkeleton.SkeletonType.FullBody)
        {
            return boneId switch
            {
                "FullBody_Root" => OVRSkeleton.BoneId.FullBody_Root,
                "FullBody_Hips" => OVRSkeleton.BoneId.FullBody_Hips,
                "FullBody_SpineLower" => OVRSkeleton.BoneId.FullBody_SpineLower,
                "FullBody_SpineMiddle" => OVRSkeleton.BoneId.FullBody_SpineMiddle,
                "FullBody_SpineUpper" => OVRSkeleton.BoneId.FullBody_SpineUpper,
                "FullBody_Chest" => OVRSkeleton.BoneId.FullBody_Chest,
                "FullBody_Neck" => OVRSkeleton.BoneId.FullBody_Neck,
                "FullBody_Head" => OVRSkeleton.BoneId.FullBody_Head,
                "FullBody_LeftShoulder" => OVRSkeleton.BoneId.FullBody_LeftShoulder,
                "FullBody_LeftScapula" => OVRSkeleton.BoneId.FullBody_LeftScapula,
                "FullBody_LeftArmUpper" => OVRSkeleton.BoneId.FullBody_LeftArmUpper,
                "FullBody_LeftArmLower" => OVRSkeleton.BoneId.FullBody_LeftArmLower,
                "FullBody_LeftHandWristTwist" => OVRSkeleton.BoneId.FullBody_LeftHandWristTwist,
                "FullBody_RightShoulder" => OVRSkeleton.BoneId.FullBody_RightShoulder,
                "FullBody_RightScapula" => OVRSkeleton.BoneId.FullBody_RightScapula,
                "FullBody_RightArmUpper" => OVRSkeleton.BoneId.FullBody_RightArmUpper,
                "FullBody_RightArmLower" => OVRSkeleton.BoneId.FullBody_RightArmLower,
                "FullBody_RightHandWristTwist" => OVRSkeleton.BoneId.FullBody_RightHandWristTwist,
                "FullBody_LeftHandPalm" => OVRSkeleton.BoneId.FullBody_LeftHandPalm,
                "FullBody_LeftHandWrist" => OVRSkeleton.BoneId.FullBody_LeftHandWrist,
                "FullBody_LeftHandThumbMetacarpal" => OVRSkeleton.BoneId.FullBody_LeftHandThumbMetacarpal,
                "FullBody_LeftHandThumbProximal" => OVRSkeleton.BoneId.FullBody_LeftHandThumbProximal,
                "FullBody_LeftHandThumbDistal" => OVRSkeleton.BoneId.FullBody_LeftHandThumbDistal,
                "FullBody_LeftHandThumbTip" => OVRSkeleton.BoneId.FullBody_LeftHandThumbTip,
                "FullBody_LeftHandIndexMetacarpal" => OVRSkeleton.BoneId.FullBody_LeftHandIndexMetacarpal,
                "FullBody_LeftHandIndexProximal" => OVRSkeleton.BoneId.FullBody_LeftHandIndexProximal,
                "FullBody_LeftHandIndexIntermediate" => OVRSkeleton.BoneId.FullBody_LeftHandIndexIntermediate,
                "FullBody_LeftHandIndexDistal" => OVRSkeleton.BoneId.FullBody_LeftHandIndexDistal,
                "FullBody_LeftHandIndexTip" => OVRSkeleton.BoneId.FullBody_LeftHandIndexTip,
                "FullBody_LeftHandMiddleMetacarpal" => OVRSkeleton.BoneId.FullBody_LeftHandMiddleMetacarpal,
                "FullBody_LeftHandMiddleProximal" => OVRSkeleton.BoneId.FullBody_LeftHandMiddleProximal,
                "FullBody_LeftHandMiddleIntermediate" => OVRSkeleton.BoneId.FullBody_LeftHandMiddleIntermediate,
                "FullBody_LeftHandMiddleDistal" => OVRSkeleton.BoneId.FullBody_LeftHandMiddleDistal,
                "FullBody_LeftHandMiddleTip" => OVRSkeleton.BoneId.FullBody_LeftHandMiddleTip,
                "FullBody_LeftHandRingMetacarpal" => OVRSkeleton.BoneId.FullBody_LeftHandRingMetacarpal,
                "FullBody_LeftHandRingProximal" => OVRSkeleton.BoneId.FullBody_LeftHandRingProximal,
                "FullBody_LeftHandRingIntermediate" => OVRSkeleton.BoneId.FullBody_LeftHandRingIntermediate,
                "FullBody_LeftHandRingDistal" => OVRSkeleton.BoneId.FullBody_LeftHandRingDistal,
                "FullBody_LeftHandRingTip" => OVRSkeleton.BoneId.FullBody_LeftHandRingTip,
                "FullBody_LeftHandLittleMetacarpal" => OVRSkeleton.BoneId.FullBody_LeftHandLittleMetacarpal,
                "FullBody_LeftHandLittleProximal" => OVRSkeleton.BoneId.FullBody_LeftHandLittleProximal,
                "FullBody_LeftHandLittleIntermediate" => OVRSkeleton.BoneId.FullBody_LeftHandLittleIntermediate,
                "FullBody_LeftHandLittleDistal" => OVRSkeleton.BoneId.FullBody_LeftHandLittleDistal,
                "FullBody_LeftHandLittleTip" => OVRSkeleton.BoneId.FullBody_LeftHandLittleTip,
                "FullBody_RightHandPalm" => OVRSkeleton.BoneId.FullBody_RightHandPalm,
                "FullBody_RightHandWrist" => OVRSkeleton.BoneId.FullBody_RightHandWrist,
                "FullBody_RightHandThumbMetacarpal" => OVRSkeleton.BoneId.FullBody_RightHandThumbMetacarpal,
                "FullBody_RightHandThumbProximal" => OVRSkeleton.BoneId.FullBody_RightHandThumbProximal,
                "FullBody_RightHandThumbDistal" => OVRSkeleton.BoneId.FullBody_RightHandThumbDistal,
                "FullBody_RightHandThumbTip" => OVRSkeleton.BoneId.FullBody_RightHandThumbTip,
                "FullBody_RightHandIndexMetacarpal" => OVRSkeleton.BoneId.FullBody_RightHandIndexMetacarpal,
                "FullBody_RightHandIndexProximal" => OVRSkeleton.BoneId.FullBody_RightHandIndexProximal,
                "FullBody_RightHandIndexIntermediate" => OVRSkeleton.BoneId.FullBody_RightHandIndexIntermediate,
                "FullBody_RightHandIndexDistal" => OVRSkeleton.BoneId.FullBody_RightHandIndexDistal,
                "FullBody_RightHandIndexTip" => OVRSkeleton.BoneId.FullBody_RightHandIndexTip,
                "FullBody_RightHandMiddleMetacarpal" => OVRSkeleton.BoneId.FullBody_RightHandMiddleMetacarpal,
                "FullBody_RightHandMiddleProximal" => OVRSkeleton.BoneId.FullBody_RightHandMiddleProximal,
                "FullBody_RightHandMiddleIntermediate" => OVRSkeleton.BoneId.FullBody_RightHandMiddleIntermediate,
                "FullBody_RightHandMiddleDistal" => OVRSkeleton.BoneId.FullBody_RightHandMiddleDistal,
                "FullBody_RightHandMiddleTip" => OVRSkeleton.BoneId.FullBody_RightHandMiddleTip,
                "FullBody_RightHandRingMetacarpal" => OVRSkeleton.BoneId.FullBody_RightHandRingMetacarpal,
                "FullBody_RightHandRingProximal" => OVRSkeleton.BoneId.FullBody_RightHandRingProximal,
                "FullBody_RightHandRingIntermediate" => OVRSkeleton.BoneId.FullBody_RightHandRingIntermediate,
                "FullBody_RightHandRingDistal" => OVRSkeleton.BoneId.FullBody_RightHandRingDistal,
                "FullBody_RightHandRingTip" => OVRSkeleton.BoneId.FullBody_RightHandRingTip,
                "FullBody_RightHandLittleMetacarpal" => OVRSkeleton.BoneId.FullBody_RightHandLittleMetacarpal,
                "FullBody_RightHandLittleProximal" => OVRSkeleton.BoneId.FullBody_RightHandLittleProximal,
                "FullBody_RightHandLittleIntermediate" => OVRSkeleton.BoneId.FullBody_RightHandLittleIntermediate,
                "FullBody_RightHandLittleDistal" => OVRSkeleton.BoneId.FullBody_RightHandLittleDistal,
                "FullBody_RightHandLittleTip" => OVRSkeleton.BoneId.FullBody_RightHandLittleTip,
                "FullBody_LeftUpperLeg" => OVRSkeleton.BoneId.FullBody_LeftUpperLeg,
                "FullBody_LeftLowerLeg" => OVRSkeleton.BoneId.FullBody_LeftLowerLeg,
                "FullBody_LeftFootAnkleTwist" => OVRSkeleton.BoneId.FullBody_LeftFootAnkleTwist,
                "FullBody_LeftFootAnkle" => OVRSkeleton.BoneId.FullBody_LeftFootAnkle,
                "FullBody_LeftFootSubtalar" => OVRSkeleton.BoneId.FullBody_LeftFootSubtalar,
                "FullBody_LeftFootTransverse" => OVRSkeleton.BoneId.FullBody_LeftFootTransverse,
                "FullBody_LeftFootBall" => OVRSkeleton.BoneId.FullBody_LeftFootBall,
                "FullBody_RightUpperLeg" => OVRSkeleton.BoneId.FullBody_RightUpperLeg,
                "FullBody_RightLowerLeg" => OVRSkeleton.BoneId.FullBody_RightLowerLeg,
                "FullBody_RightFootAnkleTwist" => OVRSkeleton.BoneId.FullBody_RightFootAnkleTwist,
                "FullBody_RightFootAnkle" => OVRSkeleton.BoneId.FullBody_RightFootAnkle,
                "FullBody_RightFootSubtalar" => OVRSkeleton.BoneId.FullBody_RightFootSubtalar,
                "FullBody_RightFootTransverse" => OVRSkeleton.BoneId.FullBody_RightFootTransverse,
                "FullBody_RightFootBall" => OVRSkeleton.BoneId.FullBody_RightFootBall,
                _ => OVRSkeleton.BoneId.Invalid
            };
        }
        else if (skeletonType == OVRSkeleton.SkeletonType.HandLeft || skeletonType == OVRSkeleton.SkeletonType.HandRight)
        {
            return boneId switch
            {
                "Hand_WristRoot" => OVRSkeleton.BoneId.Hand_WristRoot,
                "Hand_ForearmStub" => OVRSkeleton.BoneId.Hand_ForearmStub,
                "Hand_Thumb0" => OVRSkeleton.BoneId.Hand_Thumb0,
                "Hand_Thumb1" => OVRSkeleton.BoneId.Hand_Thumb1,
                "Hand_Thumb2" => OVRSkeleton.BoneId.Hand_Thumb2,
                "Hand_Thumb3" => OVRSkeleton.BoneId.Hand_Thumb3,
                "Hand_Index1" => OVRSkeleton.BoneId.Hand_Index1,
                "Hand_Index2" => OVRSkeleton.BoneId.Hand_Index2,
                "Hand_Index3" => OVRSkeleton.BoneId.Hand_Index3,
                "Hand_Middle1" => OVRSkeleton.BoneId.Hand_Middle1,
                "Hand_Middle2" => OVRSkeleton.BoneId.Hand_Middle2,
                "Hand_Middle3" => OVRSkeleton.BoneId.Hand_Middle3,
                "Hand_Ring1" => OVRSkeleton.BoneId.Hand_Ring1,
                "Hand_Ring2" => OVRSkeleton.BoneId.Hand_Ring2,
                "Hand_Ring3" => OVRSkeleton.BoneId.Hand_Ring3,
                "Hand_Pinky0" => OVRSkeleton.BoneId.Hand_Pinky0,
                "Hand_Pinky1" => OVRSkeleton.BoneId.Hand_Pinky1,
                "Hand_Pinky2" => OVRSkeleton.BoneId.Hand_Pinky2,
                "Hand_Pinky3" => OVRSkeleton.BoneId.Hand_Pinky3,
                "Hand_ThumbTip" => OVRSkeleton.BoneId.Hand_ThumbTip,
                "Hand_IndexTip" => OVRSkeleton.BoneId.Hand_IndexTip,
                "Hand_MiddleTip" => OVRSkeleton.BoneId.Hand_MiddleTip,
                "Hand_RingTip" => OVRSkeleton.BoneId.Hand_RingTip,
                "Hand_PinkyTip" => OVRSkeleton.BoneId.Hand_PinkyTip,
                _ => OVRSkeleton.BoneId.Invalid,
            };
        }
        else
        {
            return OVRSkeleton.BoneId.Invalid;
        }
    }
}
