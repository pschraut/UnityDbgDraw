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
        public static void WireArrow(Vector3 position, Quaternion rotation, Vector3 scale, Color color, float duration = 0, bool depthTest = true)
        {
            PrimitiveJob job;
            if (!TryAllocPrimitiveJob(out job, GL.LINE_STRIP, duration, depthTest, UnityEngine.Rendering.CullMode.Off, true))
                return;

            job.matrix = Matrix4x4.TRS(position, rotation, scale);
            job.useMatrix = true;

            var s = 1.0f * 0.5f;

            // forward aligned arrow
            job.AddVertex(new PrimitiveJob.Vertex() { position = new Vector3(0, 0, 1), color = color }); // tip
            job.AddVertex(new PrimitiveJob.Vertex() { position = new Vector3(+s, 0, 0), color = color });
            job.AddVertex(new PrimitiveJob.Vertex() { position = new Vector3(-s, 0, 0), color = color });
            job.AddVertex(new PrimitiveJob.Vertex() { position = new Vector3(0, 0, 1), color = color }); // tip
            job.AddVertex(new PrimitiveJob.Vertex() { position = new Vector3(0, +s, 0), color = color });
            job.AddVertex(new PrimitiveJob.Vertex() { position = new Vector3(0, -s, 0), color = color });
            job.AddVertex(new PrimitiveJob.Vertex() { position = new Vector3(0, 0, 1), color = color }); // tip
                                                                                                         //job.AddVertex(new PrimitiveJob.Vertex() { position = new Vector3(0, 0, 0), color = color }); // middle line

            job.Submit();
        }
    }
}
