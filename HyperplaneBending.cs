using MathNet.Numerics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NeuronalNetworkReverseEngineering
{
    class HyperplaneBending
    {
        public HyperplaneBending(Model model)
        {
            this.model = model;
            this.sampler = new SamplingLine(model);
            this.ransac = new RansacAlgorithm(model);
            this.salt = model.RandomGenerator.Next();
        }

        private Model model { get; }
        private SamplingLine sampler { get; }
        private RansacAlgorithm ransac { get; }
        private int stdMaxMagnitude = 10;
        private int salt;
        private int saltIncreasePerUsage = 1_000;

        public List<Hyperplane> MoveAlongBend(Hyperplane startPlane, List<Hyperplane> firstLayerPlanes)
        {
            var first = FindClosestBoundaryBend(startPlane, firstLayerPlanes);
            firstLayerPlanes.RemoveAt(first.intersectionIndex);
            var second = FindClosestBoundaryBend(first.afterBendPlane, firstLayerPlanes);



            return null;
        }
        private (Hyperplane afterBendPlane, int intersectionIndex)  FindClosestBoundaryBend(Hyperplane beforeBendPlane, List<Hyperplane> firstLayerPlanes)
        {
            /* Step 1: Find intersection.
             * Step 2: Overstep intersection by a small amount, from the direction of the startPlane. The new point is called "anchorPoint".
             * Step 3: Chose 2 random points on intersectionPlane, and make direction vector.
             * Step 4: Repeat (3) many times, vary the anchorPoint, and make sampling lines. Collect all sampling points.
             * Step 5: Remove wrong sampling points by:
             *      Step 5.1: Checking whether they are on one of the two known hyperplanes.
             *      Step 5.2: Using RANSAC.
             * Step 6: Create new hyperplane "newPlane".
             * Step 7: Remove used hyperplane firstLayerPlanes, and repeat (1-6) with newPlane.
             */

            // 1
            var startPoint = beforeBendPlane.originalBoundaryPoint ?? beforeBendPlane.pointsOnPlane.First();
            var spaceDim = startPoint.numRow + startPoint.numCol - 1;
            var iterations = (spaceDim + 1) * 8;

            var intersection = HyperplaneIntersection.FindClosestIntersection(beforeBendPlane, startPoint, firstLayerPlanes);
            double overStepDistance = (double)Matrix.GetEuclideanNormForVector(intersection.intersectPoint) / 250;

            // 2
            var directionVector = Matrix.Substraction(intersection.intersectPoint, startPoint);
            directionVector = Matrix.NormalizeVector(directionVector, (double)Matrix.GetEuclideanNormForVector(directionVector) + overStepDistance);
            var anchorPoint = Matrix.Addition(startPoint, directionVector);

            // 3&4
            var intersectionPlane = firstLayerPlanes[intersection.intersectIndex];
            var bag = new ConcurrentBag<List<Matrix>>();
            var result = Parallel.For(0, 10_000, (index, state) =>
            {
                var tempModel = model.Copy(index + salt);
                var tempSampler = new SamplingLine(tempModel);

                var randomParallelDirectionVector = intersectionPlane.GenerateRandomNormalizedVectorOnPlane();
                var tempDirectionVector = new Matrix(startPoint.numRow, startPoint.numCol);
                tempDirectionVector.PopulateAllRandomlyFarFromZero(tempModel.RandomGenerator);
                tempDirectionVector = Matrix.NormalizeVector(tempDirectionVector, overStepDistance / 4);
                var variedAnchorPoint = Matrix.Addition(anchorPoint, tempDirectionVector);

                var samplePoints = tempSampler.BidirectionalLinearRegionChanges(variedAnchorPoint, Matrix.NormalizeVector(randomParallelDirectionVector, overStepDistance / Math.Pow(2, stdMaxMagnitude - 3)), stdMaxMagnitude);
                if(samplePoints.Count == 1)
                {
                    bag.Add(samplePoints);
                    if (bag.Count > iterations)
                    {
                        state.Break();
                    }
                }
            });

            salt += saltIncreasePerUsage;
            if (result.IsCompleted)
            {
                throw new Exception("CW24");
            }
            else
            {
                var samples = bag.SelectMany(y => y).ToList();
                // 5.1
                foreach(var sample in samples.ToArray())
                {
                    if (true == intersectionPlane.IsPointOnPlane(sample, 0.05))
                    {
                        samples.Remove(sample);
                    }
                }
                // 5.2&6
                var newPlane = new Hyperplane(model, ransac, samples, false);
                return (newPlane, intersection.intersectIndex);
            }
        }



    }
}
