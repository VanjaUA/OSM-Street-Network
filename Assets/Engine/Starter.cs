#region

using OSMStreetNetwork.Graph;
using OSMStreetNetwork.AI;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Splines;

#endregion

namespace OSMStreetNetwork
{
    public class Starter : MonoBehaviour
    {
        private OSMParser _osmParser;
        private RoadSplineBuilder _roadSplineBuilder;
        private RoadGraph _roadGraph;

        [SerializeField] private GameObject carPrefab;
        private CarAI _car;

        private void Start()
        {
            _osmParser = new OSMParser("osm_zurich"); // put "osm_zurich2" for bigger map
            _roadSplineBuilder = new RoadSplineBuilder(_osmParser); // Creates road and sidewalks splines and visuals them
            _roadGraph = new RoadGraph(_osmParser); // Creates road graph and creates debug gameobjects


            var testPath = _roadGraph.GetPath(2202561100, 473403330); // Creates path with BFS between any two nodes(id)
            SplineBuilder.CreateSplineFromPoints(SplineBuilder.GenerateSmoothPath(testPath)); // Creates spline at that path

            //Creates path spline for car AI to move
            List<Vector3> path = SplineBuilder.GenerateSmoothPath(testPath);
            SplineContainer spline = SplineBuilder.CreateSplineContainer(path);
            _car = new CarAI(spline, carPrefab, 0.025f);
        }

        private void Update()
        {
            if (_car != null)
            {
                _car.Update(Time.deltaTime);
            }
        }
    }
}
