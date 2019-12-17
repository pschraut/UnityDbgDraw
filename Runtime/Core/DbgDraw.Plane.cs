// DbgDraw for Unity. Copyright (c) 2019 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityDbgDraw
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable IDE0018 // Variable declaration can be inlined
#pragma warning disable IDE0017 // Object initialization can be simplified

namespace Oddworm.Framework
{
    public partial class DbgDraw
    {
        static Mesh s_PlaneMesh = null;

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Plane(UnityEngine.Plane plane, Vector3 position, Vector3 scale, Color color, float duration = 0, bool depthTest = true)
        {
            MeshJob job;
            if (!TryAllocMeshJob(out job, duration, depthTest, UnityEngine.Rendering.CullMode.Off, true))
                return;

            if (s_PlaneMesh == null)
            {
                s_PlaneMesh = CreatePlaneMesh();
                ReleaseOnDestroy(s_PlaneMesh);
            }

            job.mesh = s_PlaneMesh;
            job.matrix = Matrix4x4.TRS(position, Quaternion.LookRotation(plane.normal, Vector3.forward), scale);
            job.color = color;

            job.Submit();
        }

        static Mesh CreatePlaneMesh()
        {
            var mesh = new Mesh();
            mesh.name = "DbgDraw-Plane-Mesh";

            var vertices = new List<Vector3>(64 * 3);
            var s = 0.5f;

            // quad
            vertices.Add(new Vector3(-s, 0, -s)); // near left
            vertices.Add(new Vector3(-s, 0, +s)); // far left
            vertices.Add(new Vector3(+s, 0, +s)); // far right

            vertices.Add(new Vector3(+s, 0, +s)); // far right
            vertices.Add(new Vector3(+s, 0, -s)); // near right
            vertices.Add(new Vector3(-s, 0, -s)); // near left

            // "arrrow"
            s = 0.01f;
            vertices.Add(new Vector3(0, 0, -s));
            vertices.Add(new Vector3(0, 0.25f, 0));
            vertices.Add(new Vector3(0, 0, +s));

            vertices.Add(new Vector3(-s, 0, 0));
            vertices.Add(new Vector3(0, 0.25f, 0));
            vertices.Add(new Vector3(+s, 0, 0));


            var indices = new int[vertices.Count];
            for (var n = 0; n < indices.Length; ++n)
                indices[n] = n;

            mesh.SetVertices(vertices);
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            mesh.RecalculateNormals();

            return mesh;
        }
    }
}
