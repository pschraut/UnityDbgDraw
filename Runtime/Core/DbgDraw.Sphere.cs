// DbgDraw for Unity. Copyright (c) 2019-2024 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityDbgDraw
using UnityEngine;
#pragma warning disable IDE0018 // Variable declaration can be inlined
#pragma warning disable IDE0017 // Object initialization can be simplified

namespace Oddworm.Framework
{
    public partial class DbgDraw
    {
        static Mesh s_SphereMesh = null;

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Sphere(Vector3 position, Quaternion rotation, Vector3 scale, Color color, float duration = 0, bool depthTest = true)
        {
            MeshJob job;
            if (!TryAllocMeshJob(out job, duration, depthTest, UnityEngine.Rendering.CullMode.Back, true))
                return;

            if (s_SphereMesh == null)
            {
                s_SphereMesh = CreateSphereMesh();
                ReleaseOnDestroy(s_SphereMesh);
            }

            job.mesh = s_SphereMesh;
            job.matrix = Matrix4x4.TRS(position, rotation, scale);
            job.color = color;

            job.Submit();
        }

        static Mesh CreateSphereMesh()
        {
            var mesh = CreateMesh(PrimitiveType.Sphere);
            mesh.name = "DbgDraw-Sphere-Mesh";
            return mesh;
        }
    }
}
