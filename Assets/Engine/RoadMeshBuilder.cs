#region

using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;
using Unity.Mathematics;

#endregion

namespace OSMStreetNetwork
{
    [RequireComponent(typeof(SplineContainer))]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class RoadMeshBuilder : MonoBehaviour
    {
        public float RoadWidth { get; set; } = 2f;
        public int SamplesPerSegment { get; set; } = 30;
        public Color RoadColor { get; set; } = Color.black;

        public void BuildRoadMesh()
        {
            var spline = GetComponent<SplineContainer>().Spline;

            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            float halfWidth = RoadWidth * 0.5f;
            int index = 0;

            float totalLength = spline.GetLength();
            int steps = Mathf.CeilToInt(totalLength * SamplesPerSegment);

            Vector3 lastRight = Vector3.right; // fallback right vector for continuity

            for (int i = 0; i < steps; i++)
            {
                float t1 = i / (float)steps;
                float t2 = (i + 1) / (float)steps;

                SplineUtility.Evaluate(spline, t1, out float3 pos1, out float3 tangent1, out _);
                SplineUtility.Evaluate(spline, t2, out float3 pos2, out float3 tangent2, out _);

                Vector3 dir1 = ((Vector3)tangent1).normalized;
                Vector3 dir2 = ((Vector3)tangent2).normalized;

                // Use fixed up vector to prevent twisting
                Vector3 right1 = Vector3.Cross(Vector3.up, dir1).normalized * halfWidth;
                Vector3 right2 = Vector3.Cross(Vector3.up, dir2).normalized * halfWidth;

                // Fallback in case right flips (happens in vertical loops or sharp corners)
                if (right1 == Vector3.zero) right1 = lastRight;
                if (right2 == Vector3.zero) right2 = lastRight;

                Vector3 v1 = (Vector3)pos1 - right1;
                Vector3 v2 = (Vector3)pos1 + right1;
                Vector3 v3 = (Vector3)pos2 - right2;
                Vector3 v4 = (Vector3)pos2 + right2;

                verts.Add(v1);
                verts.Add(v2);
                verts.Add(v3);
                verts.Add(v4);

                tris.Add(index + 0);
                tris.Add(index + 2);
                tris.Add(index + 1);

                tris.Add(index + 2);
                tris.Add(index + 3);
                tris.Add(index + 1);

                uvs.Add(new Vector2(0, t1));
                uvs.Add(new Vector2(1, t1));
                uvs.Add(new Vector2(0, t2));
                uvs.Add(new Vector2(1, t2));

                index += 4;
                lastRight = right2; // persist the last right for stability
            }

            Mesh mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            var mf = GetComponent<MeshFilter>();
            mf.mesh = mesh;

            var mr = GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = RoadColor;
            mr.material = mat;
        }
    }
}
