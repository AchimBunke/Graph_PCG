using System;
using UnityEngine;
using UnityUtilities.Attributes;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Interpolation

{
    [Serializable]
    public struct HGraphInterpolationConfiguration
    {
        [SerializeField] public HGraphInterpolationType InterpolationType;

        [SerializeField,
            ShowIf(ShowIfAttribute.EvaluationModeType.OR,
            HideInInspector: true,
            nameof(ShowPowerParameter))]
        public float PowerParameter;
        private bool ShowPowerParameter() => InterpolationType switch
        {
            HGraphInterpolationType.InverseDistanceWeighting => true,
            HGraphInterpolationType.SpaceAdjusted_InverseDistanceWeighting => true,
            _ => false
        };


        public HGraphAttributeInterpolationMethod Create()
        {
            switch (InterpolationType)
            {
                case HGraphInterpolationType.Voronoi:
                    return new VoronoiInterpolation();
                case HGraphInterpolationType.InverseDistanceWeighting:
                    return new InverseDistanceWeighting(PowerParameter);
                case HGraphInterpolationType.SpaceAdjusted_InverseDistanceWeighting:
                    return new SpaceAdjusted_InverseDistanceWeighting(PowerParameter);
                default:
                    return default;
            }
        }
    }
    public enum HGraphInterpolationType
    {
        Voronoi,
        InverseDistanceWeighting,
        SpaceAdjusted_InverseDistanceWeighting
    }
}