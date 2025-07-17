#region

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

#endregion

namespace OSMStreetNetwork.Graph
{
    public static class SplineBuilder
    {
        public static GameObject CreateSplineFromPoints(List<Vector3> points, string name = "RoadSpline")
        {
            if (points == null || points.Count < 2)
            {
                Debug.LogWarning("Invalid points to build spline.");
                return null;
            }

            GameObject splineGO = new GameObject(name);
            var splineContainer = splineGO.AddComponent<SplineContainer>();
            Spline spline = new();

            for (int i = 0; i < points.Count; i++)
            {
                Vector3 position = points[i];

                Vector3 forward = Vector3.zero;
                if (i > 0 && i < points.Count - 1)
                {
                    Vector3 prev = points[i - 1];
                    Vector3 next = points[i + 1];
                    forward = (next - prev).normalized;
                }
                else if (i > 0)
                {
                    forward = (position - points[i - 1]).normalized;
                }
                else if (i < points.Count - 1)
                {
                    forward = (points[i + 1] - position).normalized;
                }

                float tangentMag = 2f;
                Vector3 tangent = forward * tangentMag;

                BezierKnot knot = new(position, -tangent, tangent);
                spline.Add(knot);
            }

            splineContainer.Spline = spline;
            return splineGO;
        }

        public static List<Vector3> GenerateSmoothPath(List<GraphNode> path, int turnPointCount = 3,
            float turnRadius = 6f, float overshootFactor = 0.15f)
        {
            var result = new List<Vector3>();

            for (int i = 0; i < path.Count; i++)
            {
                GraphNode current = path[i];
                Vector3 currentPos = current.WorldPosition;

                bool isIntersection = current.Connections.Count >= 3;

                if (isIntersection && i > 0 && i < path.Count - 1)
                {
                    Vector3 prev = path[i - 1].WorldPosition;
                    Vector3 next = path[i + 1].WorldPosition;

                    Vector3 inDir = (currentPos - prev).normalized;
                    Vector3 outDir = (next - currentPos).normalized;

                    Vector3 cross = Vector3.Cross(inDir, outDir);
                    float turnDirection = Mathf.Sign(cross.y); // +1 = left, -1 = right

                    Vector3 overshootDir = Vector3.Cross(inDir, Vector3.up) * -turnDirection;

                    float distToPrev = Vector3.Distance(currentPos, prev);
                    float distToNext = Vector3.Distance(currentPos, next);

                    float entryOffset = Mathf.Min(turnRadius, distToPrev * 0.5f);
                    float exitOffset = Mathf.Min(turnRadius, distToNext * 0.5f);

                    Vector3 entryPoint = currentPos - inDir * entryOffset + overshootDir * entryOffset * -overshootFactor;
                    Vector3 exitPoint = currentPos + outDir * exitOffset;


                    for (int t = 0; t < turnPointCount; t++)
                    {
                        float ratio = t / (float)(turnPointCount - 1);
                        Vector3 point = Vector3.Lerp(
                            Vector3.Lerp(entryPoint, currentPos, ratio),
                            Vector3.Lerp(currentPos, exitPoint, ratio),
                            ratio
                        );
                        result.Add(point);
                    }
                }
                else
                {
                    result.Add(currentPos);
                }
            }

            return result;
        }

        public static SplineContainer CreateSplineContainer(List<Vector3> points, string name = "RuntimeSpline")
        {
            GameObject go = new GameObject(name);
            var container = go.AddComponent<SplineContainer>();

            Spline spline = new Spline();
            foreach (var pt in points)
            {
                spline.Add(new BezierKnot(pt));
            }

            container.Spline = spline;
            return container;
        }
    }
}
