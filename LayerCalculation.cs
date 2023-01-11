using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NeuronalNetworkReverseEngineering
{
    class LayerCalculation
    {
        public LayerCalculation(Model model, SamplingSphere sphere)
        {
            this.model = model;
            this.sphere = sphere;
        }

        private Model model { get; }
        private SamplingSphere sphere { get; }
        private int stdMaxMagnitude = 8;
        private int stdNumTestPoints = 500;
        private int stdNumTestLines = 30;

        public List<Hyperplane> GetFirstLayer(List<Hyperplane> hyperPlanes, double testRadius)
        {
            var retVal = new List<Hyperplane>();

            foreach (var plane in hyperPlanes)
            {
                var temp = sphere.FirstLayerTest(plane, stdNumTestPoints, testRadius);
                if (temp.Count(x => x.Count == 1) > 0.8 * stdNumTestPoints) {
                    retVal.Add(plane);
                }
            }
            return retVal;
        }





    }
}
