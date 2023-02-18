using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeuronalNetworkReverseEngineering
{
    public record SpaceLineBundle
    {

        public SpaceLineBundle() { }
        public SpaceLineBundle(IList<SpaceLine> spaceLines)
        {
            this.SpaceLines = spaceLines;
        }
        public IList<SpaceLine> SpaceLines { get; set; } = new List<SpaceLine>();
    }
    public record SpaceLine
    {
        public IList<SpaceLinePoint> SpaceLinePoints { get; set; } = new List<SpaceLinePoint>();
    }

    public record SpaceLinePoint
    {
        public Matrix BoundaryPoint { get; set; }
        public double? SafeDistance { get; set; }
    }

    public record HyperplaneIdentity
    {
        public Matrix Parameters { get; set; }
        public double? Intercept { get; set;}  
    }

}
