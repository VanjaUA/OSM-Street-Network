#region

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

namespace OSMStreetNetwork.Graph
{
    public class RoadGraph
    {
        private OSMParser _osmParser;
        private Dictionary<long, GraphNode> _graphNodes = new();

        private GameObject _graphNodesHolder;

        public RoadGraph(OSMParser osmParser)
        {
            _osmParser = osmParser;
            _graphNodesHolder = new GameObject("GraphNodesHolder");
            BuildGraphNodes();
        }

        private void BuildGraphNodes()
        {
            foreach (var way in _osmParser.RoadWays)
            {
                var nodeRefs = way.NodeRefs;

                for (int i = 0; i < nodeRefs.Count; i++)
                {
                    long nodeId = nodeRefs[i];

                    // Create node if doesn't exist
                    if (!_graphNodes.TryGetValue(nodeId, out GraphNode currentNode))
                    {
                        if (!_osmParser.Nodes.TryGetValue(nodeId, out var osmNode))
                            continue;

                        Vector3 pos = _osmParser.LatLonToUnity(osmNode.Lat, osmNode.Lon);
                        GameObject go = new GameObject();
                        go.transform.parent = _graphNodesHolder.transform;
                        currentNode = go.AddComponent<GraphNode>();
                        currentNode.Initialize(nodeId, pos);
                        _graphNodes[nodeId] = currentNode;
                    }

                    // Connect to previous node in way
                    if (i > 0)
                    {
                        long prevId = nodeRefs[i - 1];
                        if (_graphNodes.TryGetValue(prevId, out GraphNode prevNode))
                        {
                            currentNode.AddConnection(prevNode);
                            prevNode.AddConnection(currentNode);
                        }
                    }
                }
            }

            Debug.Log($"Created {_graphNodes.Count} GraphNode objects.");
        }


        public List<GraphNode> GetPath(long startId, long endId)
        {
            if (!_graphNodes.ContainsKey(startId) || !_graphNodes.ContainsKey(endId))
                return null;

            var start = _graphNodes[startId];
            var end = _graphNodes[endId];

            Queue<GraphNode> queue = new();
            Dictionary<GraphNode, GraphNode> cameFrom = new();
            HashSet<GraphNode> visited = new();

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current == end)
                {
                    List<GraphNode> path = new();
                    while (current != null)
                    {
                        path.Add(current);
                        cameFrom.TryGetValue(current, out current);
                    }
                    path.Reverse();
                    return path;
                }

                foreach (var neighbor in current.Connections)
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        cameFrom[neighbor] = current;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return null;
        }
    }
}
