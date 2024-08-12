// DbgDraw for Unity. Copyright (c) 2019-2024 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityDbgDraw
using UnityEngine;
#pragma warning disable IDE0018 // Variable declaration can be inlined
#pragma warning disable IDE0017 // Object initialization can be simplified

namespace Oddworm.Framework
{
    public partial class DbgDraw
    {
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Matrix(Matrix4x4 matrix, float duration = 0, bool depthTest = true)
        {
            PrimitiveJob job;
            if (!TryAllocPrimitiveJob(out job, GL.LINES, duration, depthTest, UnityEngine.Rendering.CullMode.Off, true))
                return;

            job.matrix = matrix;
            job.useMatrix = true;
            job.useVertexColor = true;

            job.AddVertex(new PrimitiveJob.Vertex() { position = Vector3.zero, color = s_XAxisColor });
            job.AddVertex(new PrimitiveJob.Vertex() { position = Vector3.right, color = s_XAxisColor });

            job.AddVertex(new PrimitiveJob.Vertex() { position = Vector3.zero, color = s_YAxisColor });
            job.AddVertex(new PrimitiveJob.Vertex() { position = Vector3.up, color = s_YAxisColor });

            job.AddVertex(new PrimitiveJob.Vertex() { position = Vector3.zero, color = s_ZAxisColor });
            job.AddVertex(new PrimitiveJob.Vertex() { position = Vector3.forward, color = s_ZAxisColor });

            job.Submit();
        }
    }
}
