﻿#if UNITY_EDITOR
using UnityEngine;

[CreateAssetMenu()]
public class DefaultSummonProperties : ScriptableObject {
    public Material fadeMaterial;
    public AnimationCurve growthCurveXZ, growthCurveY;
}
#endif