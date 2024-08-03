using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// See https://github.com/EpicGames/UnrealEngine/blob/release/Engine/Plugins/Experimental/PCG/Source/PCG/Public/Elements/PCGSurfaceSampler.h
/// </summary>
namespace Achioto.Gamespace_PCG.Runtime.PCG.PointSamplers
{
    public abstract class PCGPointSampler : MonoBehaviour
    {
        public abstract IEnumerable<PCGPoint> SamplePoints();
    }
   
}
