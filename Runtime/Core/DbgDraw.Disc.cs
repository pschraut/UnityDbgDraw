// DbgDraw for Unity. Copyright (c) 2019-2024 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityDbgDraw
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable IDE0018 // Variable declaration can be inlined
#pragma warning disable IDE0017 // Object initialization can be simplified

namespace Oddworm.Framework
{
    public partial class DbgDraw
    {
        static Mesh s_DiscMesh = null;

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Disc(Vector3 position, Quaternion rotation, float radius, Color color, float duration = 0, bool depthTest = true)
        {
            MeshJob job;
            if (!TryAllocMeshJob(out job, duration, depthTest, UnityEngine.Rendering.CullMode.Off, true))
                return;

            if (s_DiscMesh == null)
            {
                s_DiscMesh = CreateDiscMesh();
                ReleaseOnDestroy(s_DiscMesh);
            }

            job.mesh = s_DiscMesh;
            job.matrix = Matrix4x4.TRS(position, rotation, Vector3.one * radius);
            job.color = color;

            job.Submit();
        }

        static Mesh CreateDiscMesh()
        {
            var mesh = new Mesh();
            mesh.name = "DbgDraw-Disc-Mesh";

            var vertices = new List<Vector3>(64 * 3);
            var step = kTau / 64;

            for (var theta = step; theta < kTau; theta += step)
            {
                var cos0 = Mathf.Cos(theta - step);
                var cos1 = Mathf.Cos(theta);
                var sin0 = Mathf.Sin(theta - step);
                var sin1 = Mathf.Sin(theta);

                vertices.Add(Vector3.zero);
                vertices.Add(new Vector3(cos0, 0, -sin0));
                vertices.Add(new Vector3(cos1, 0, -sin1));
            }

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
