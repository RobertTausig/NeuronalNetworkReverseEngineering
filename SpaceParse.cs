using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeuronalNetworkReverseEngineering
{
    record SpaceLineBundle
    {

        public SpaceLineBundle() { }
        public SpaceLineBundle(IList<SpaceLine> spaceLines)
        {
            this.SpaceLines = spaceLines;
        }
        public IList<SpaceLine> SpaceLines { get; set; } = new List<SpaceLine>();
    }
    record SpaceLine
    {
        public IList<SpaceLinePoint> SpaceLinePoints { get; set; } = new List<SpaceLinePoint>();
    }

    record SpaceLinePoint
    {
        public Matrix BoundaryPoint { set; get; }
        public decimal? SafeDistance { get; set; }
    }

}
