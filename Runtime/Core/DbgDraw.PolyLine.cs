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
        /// <summary>
        /// Draws a line going through the list of points.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void PolyLine(List<Vector3> points, Color color, float duration = 0, bool depthTest = true)
        {
            if (points == null || points.Count == 0)
                return;

            LineBatchJob job;
            if (!TryGetLineBatch(out job, depthTest, UnityEngine.Rendering.CullMode.Off))
                return;

            for (var n = 1; n < points.Count; ++n)
                job.AddLine(points[n - 1], points[n], color, duration);
        }

        /// <summary>
        /// Draws a line going through the list of points.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void PolyLine(Vector3[] points, Color color, float duration = 0, bool depthTest = true)
        {
            if (points == null || points.Length == 0)
                return;

            LineBatchJob job;
            if (!TryGetLineBatch(out job, depthTest, UnityEngine.Rendering.CullMode.Off))
                return;

            for (var n = 1; n < points.Length; ++n)
                job.AddLine(points[n - 1], points[n], color, duration);
        }

#if UNITY_2018_1_OR_NEWER
        /// <summary>
        /// Draws a line going through the list of points.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void PolyLine(Unity.Collections.NativeSlice<Vector3> points, Color color, float duration = 0, bool depthTest = true)
        {
            if (points == null || points.Length == 0)
                return;

            LineBatchJob job;
            if (!TryGetLineBatch(out job, depthTest, UnityEngine.Rendering.CullMode.Off))
                return;

            for (var n = 1; n < points.Length; ++n)
                job.AddLine(points[n - 1], points[n], color, duration);
        }
#endif
    }
}
