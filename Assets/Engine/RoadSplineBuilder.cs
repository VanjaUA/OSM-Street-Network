#region

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

#endregion

namespace OSMStreetNetwork
{
    public class RoadSplineBuilder
    {
        private GameObject _nodesHolder;
        private GameObject _waysHolder;

        private OSMParser _osmParser;

        public RoadSplineBuilder(OSMParser osmParser)
        {
            _osmParser = osmParser;

            _nodesHolder = new GameObject("Nodes");
            _waysHolder = new GameObject("Ways");

            CreateAllSplines();

            CreateAllNodes();
        }

        #region Splines

        private void CreateAllSplines()
        {
            CreateRoadSplines();
            CreateFootwaySplines();
        }

        private void CreateRoadSplines() 
        {
            foreach (var way in _osmParser.RoadWays)
            {
                List<Vector3> points = new List<Vector3>();
                foreach (long nodeId in way.NodeRefs)
                {
                    if (_osmParser.Nodes.TryGetValue(nodeId, out var node))
                        points.Add(_osmParser.LatLonToUnity(node.Lat, node.Lon));
                }

                if (points.Count >= 2)
                {
                    GameObject splineObject = CreateUnityBezierSpline(points, $"RoadSpline_{way.Id}");
                    AddRoadMeshVisualizer(splineObject, 4f, Color.black);
                }
            }
        }

        private void CreateFootwaySplines()
        {
            foreach (var way in _osmParser.FootwayWays)
            {
                List<Vector3> points = new List<Vector3>();
                foreach (long nodeId in way.NodeRefs)
                {
                    if (_osmParser.Nodes.TryGetValue(nodeId, out var node))
                        points.Add(_osmParser.LatLonToUnity(node.Lat, node.Lon));
                }

                if (points.Count >= 2)
                {
                    var smoothPoints = GetSmoothPathWithMidpoints(points);
                    GameObject splineObject = CreateUnityBezierSpline(smoothPoints, $"FootwaySpline_{way.Id}");
                    splineObject.transform.position += new Vector3(0,-0.01f,0);
                    AddRoadMeshVisualizer(splineObject, 2f, Color.white);
                }
            }
        }

        private GameObject CreateUnityBezierSpline(List<Vector3> points, string name = "Spline")
        {
            if (points.Count < 2) 
            {
                Debug.LogError("Points count is less then 2");
                return null;
            }

            GameObject splineObject = new GameObject(name);
            var container = splineObject.AddComponent<SplineContainer>();
            var spline = new Spline();

            for (int i = 0; i < points.Count; i++)
            {
                Vector3 pos = points[i];
                Vector3 inTangent = Vector3.zero;
                Vector3 outTangent = Vector3.zero;

                if (i > 0 && i < points.Count - 1)
                {
                    Vector3 prev = points[i - 1];
                    Vector3 next = points[i + 1];

                    Vector3 dir = (next - prev).normalized;
                    float angle = Vector3.Angle(next - pos, prev - pos);
                    float curveFactor = Mathf.InverseLerp(180f, 0f, angle); // 0 at straight, 1 at sharp
                    float magnitude = Vector3.Distance(prev, next) * Mathf.Lerp(0.1f, 0.4f, curveFactor);

                    inTangent = -dir * magnitude;
                    outTangent = dir * magnitude;
                }

                BezierKnot knot = new BezierKnot(pos, inTangent, outTangent);
                spline.Add(knot);
            }

            container.Spline = spline;
            splineObject.transform.parent = _waysHolder.transform;
            return splineObject;
        }

        #endregion


        private void AddRoadMeshVisualizer(GameObject splineObject, float roadWidth, Color roadColor) 
        {
            splineObject.AddComponent<MeshFilter>();
            splineObject.AddComponent<MeshRenderer>();
            RoadMeshBuilder meshBuilder = splineObject.AddComponent<RoadMeshBuilder>();
            meshBuilder.RoadWidth = roadWidth;
            meshBuilder.RoadColor = roadColor;
            meshBuilder.BuildRoadMesh();
        }

        #region Nodes

        private void CreateAllNodes() 
        {
            CreateRoadNodes();
            CreateFootwayNodes();
        }

        private void CreateRoadNodes()
        {
            foreach (var way in _osmParser.RoadWays)
            {
                foreach (var nodeRef in way.NodeRefs)
                {
                    var node = _osmParser.Nodes[nodeRef];
                    GameObject point = new GameObject($"RoadNode_{node.Id}");
                    point.transform.parent = _nodesHolder.transform;
                    point.transform.position = _osmParser.LatLonToUnity(node.Lat, node.Lon);
                }
            }
        }

        private void CreateFootwayNodes()
        {
            foreach (var way in _osmParser.FootwayWays)
            {
                foreach (var nodeRef in way.NodeRefs)
                {
                    var node = _osmParser.Nodes[nodeRef];
                    GameObject point = new GameObject($"FootwayNode_{node.Id}");
                    point.transform.parent = _nodesHolder.transform;
                    point.transform.position = _osmParser.LatLonToUnity(node.Lat, node.Lon);
                }
            }
        }

        #endregion

        private List<Vector3> GetSmoothPathWithMidpoints(List<Vector3> rawPoints, float angleThreshold = 30f)
        {
            List<Vector3> result = new List<Vector3>();

            if (rawPoints.Count < 3)
                return rawPoints;

            result.Add(rawPoints[0]);

            for (int i = 1; i < rawPoints.Count - 1; i++)
            {
                Vector3 prev = rawPoints[i - 1];
                Vector3 curr = rawPoints[i];
                Vector3 next = rawPoints[i + 1];

                Vector3 dir1 = (curr - prev).normalized;
                Vector3 dir2 = (next - curr).normalized;

                float angle = Vector3.Angle(dir1, dir2);

                result.Add(curr);

                if (angle < angleThreshold)
                {
                    // Insert two helper points for smoother turning arc
                    Vector3 mid1 = Vector3.Lerp(prev, curr, 0.75f); // closer to current
                    Vector3 mid2 = Vector3.Lerp(curr, next, 0.25f); // closer to current

                    result.Insert(result.Count - 1, mid1); // before curr
                    result.Add(mid2);                      // after curr
                }
            }

            result.Add(rawPoints[rawPoints.Count - 1]);
            return result;
        }

    }
}
