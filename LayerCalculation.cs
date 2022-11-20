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

        public List<Hyperplane> GetFirstLayer(List<Hyperplane> hyperPlanes)
        {
            var retVal = new List<Hyperplane>();

            foreach (var plane in hyperPlanes)
            {
                var aa = sphere.FirstLayerTest(plane, 500, 1_000);
                var bb = aa.Where(x => x.Count == 1).Count();
            }


            //for (int i = 0; i < hyperPlanes.Count; i++)
            //{
            //    if (papap[i] > 0.9 * stdNumTestPoints)
            //    {
            //        retVal.Add(hyperPlanes[i]);
            //    }
            //}
            return retVal;
        }





    }
}
