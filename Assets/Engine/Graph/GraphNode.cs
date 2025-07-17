#region

using System.Collections.Generic;
using UnityEngine;

#endregion


namespace OSMStreetNetwork.Graph
{
    public class GraphNode : MonoBehaviour
    {
        public long NodeId;
        public Vector3 WorldPosition;
        public List<GraphNode> Connections = new List<GraphNode>();

        public void Initialize(long id, Vector3 position)
        {
            NodeId = id;
            WorldPosition = position;
            transform.position = position;

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(this.transform);
            sphere.transform.localPosition = Vector3.zero;
            sphere.transform.localScale = Vector3.one * 0.5f;
            sphere.GetComponent<Renderer>().material.color = Color.red;

            // Optional: name for clarity in Hierarchy
            gameObject.name = $"GraphNode_{NodeId}";
        }

        public void AddConnection(GraphNode other)
        {
            if (!Connections.Contains(other))
            {
                Connections.Add(other);
            }
        }
    }
}
