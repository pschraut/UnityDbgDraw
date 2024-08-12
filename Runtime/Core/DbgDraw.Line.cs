// DbgDraw for Unity. Copyright (c) 2019-2024 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityDbgDraw
using UnityEngine;
#pragma warning disable IDE0018 // Variable declaration can be inlined
#pragma warning disable IDE0017 // Object initialization can be simplified

namespace Oddworm.Framework
{
    public partial class DbgDraw
    {
        /// <summary>
        /// Draws a line from start to end.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Line(Vector3 start, Vector3 end, Color color, float duration = 0, bool depthTest = true)
        {
            LineBatchJob job;
            if (!TryGetLineBatch(out job, depthTest, UnityEngine.Rendering.CullMode.Off))
                return;

            job.AddLine(start, end, color, duration);
        }
    }
}
