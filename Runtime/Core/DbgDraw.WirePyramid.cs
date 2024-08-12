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
        static Mesh s_WirePyramidMesh = null;

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void WirePyramid(Vector3 position, Quaternion rotation, Vector3 scale, Color color, float duration = 0, bool depthTest = true)
        {
            MeshJob job;
            if (!TryAllocMeshJob(out job, duration, depthTest, UnityEngine.Rendering.CullMode.Off, true))
                return;

            if (s_WirePyramidMesh == null)
            {
                s_WirePyramidMesh = CreateWirePyramidMesh();
                ReleaseOnDestroy(s_WirePyramidMesh);
            }

            job.mesh = s_WirePyramidMesh;
            job.matrix = Matrix4x4.TRS(position, rotation, scale);
            job.color = color;

            job.Submit();
        }


        static Mesh CreateWirePyramidMesh()
        {
            var mesh = new Mesh();
            mesh.name = "DbgDraw-WirePyramid-Mesh";

            var vertices = new List<Vector3>(16);
            var s = 0.5f;

            vertices.Add(new Vector3(-s, -s, -s)); // bottom near left
            vertices.Add(new Vector3(-s, -s, +s)); // bottom far left
            vertices.Add(new Vector3(-s, -s, +s)); // bottom far left
            vertices.Add(new Vector3(+s, -s, +s)); // bottom far right
            vertices.Add(new Vector3(+s, -s, +s)); // bottom far right
            vertices.Add(new Vector3(+s, -s, -s)); // bottom near right
            vertices.Add(new Vector3(+s, -s, -s)); // bottom near right
            vertices.Add(new Vector3(-s, -s, -s)); // bottom near left

            vertices.Add(new Vector3(+0, +s, +0)); // top center
            vertices.Add(new Vector3(-s, -s, -s)); // bottom near left
            vertices.Add(new Vector3(+0, +s, +0)); // top center
            vertices.Add(new Vector3(-s, -s, +s)); // bottom far left
            vertices.Add(new Vector3(+0, +s, +0)); // top center
            vertices.Add(new Vector3(+s, -s, -s)); // bottom near right
            vertices.Add(new Vector3(+0, +s, +0)); // top center
            vertices.Add(new Vector3(+s, -s, +s)); // bottom far right

            mesh.SetVertices(vertices);

            var indices = new int[vertices.Count];
            for (var n = 0; n < indices.Length; ++n)
                indices[n] = n;
            mesh.SetIndices(indices, MeshTopology.Lines, 0);

            return mesh;
        }
    }
}
