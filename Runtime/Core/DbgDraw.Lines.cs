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
        /// Draws a list of line segments.
        /// </summary>
        /// <param name="lineSegments">A list of pairs of points that represent the start and end of line segments.</param>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Lines(List<Vector3> lineSegments, Color color, float duration = 0, bool depthTest = true)
        {
            if (lineSegments == null || lineSegments.Count == 0)
                return;

            LineBatchJob job;
            if (!TryGetLineBatch(out job, depthTest, UnityEngine.Rendering.CullMode.Off))
                return;

            for (var n = 1; n < lineSegments.Count; ++n)
                job.AddLine(lineSegments[n - 1], lineSegments[n], color, duration);
        }

        /// <summary>
        /// Draws a list of line segments.
        /// </summary>
        /// <param name="lineSegments">A list of pairs of points that represent the start and end of line segments.</param>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Lines(Vector3[] lineSegments, Color color, float duration = 0, bool depthTest = true)
        {
            if (lineSegments == null || lineSegments.Length == 0)
                return;

            LineBatchJob job;
            if (!TryGetLineBatch(out job, depthTest, UnityEngine.Rendering.CullMode.Off))
                return;

            for (var n = 1; n < lineSegments.Length; ++n)
                job.AddLine(lineSegments[n - 1], lineSegments[n], color, duration);
        }

#if UNITY_2018_1_OR_NEWER
        /// <summary>
        /// Draws a list of line segments.
        /// </summary>
        /// <param name="lineSegments">A list of pairs of points that represent the start and end of line segments.</param>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Lines(Unity.Collections.NativeSlice<Vector3> lineSegments, Color color, float duration = 0, bool depthTest = true)
        {
            if (lineSegments == null || lineSegments.Length == 0)
                return;

            LineBatchJob job;
            if (!TryGetLineBatch(out job, depthTest, UnityEngine.Rendering.CullMode.Off))
                return;

            for (var n = 1; n < lineSegments.Length; ++n)
                job.AddLine(lineSegments[n - 1], lineSegments[n], color, duration);
        }
#endif
    }
}
