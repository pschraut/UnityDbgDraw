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
        /// Draws a circular arc in 3D space.
        /// </summary>
        /// <param name="position">The center of the circle.</param>
        /// <param name="rotation">The rotation of the circle.</param>
        /// <param name="from">The direction of the point on the circle circumference, relative to the center, where the arc begins. This is often just transform.forward for example.</param>
        /// <param name="fromAngle">The starting angle of the arc, relative to 'from', in degrees.</param>
        /// <param name="toAngle">The ending angle of the arc, relative to 'from', in degrees.</param>
        /// <param name="innerRadius">The inner radius of the circle.</param>
        /// <param name="outerRadius">The outer radius of the circle.</param>
        /// <param name="color">The color.</param>
        /// <param name="duration">How long the arc should be visible, in seconds.</param>
        /// <param name="depthTest">Whether the arc be obscured by objects closer to the camera.</param>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Arc(Vector3 position, Quaternion rotation, Vector3 from, float angle, float radius, Color color, float duration = 0, bool depthTest = true)
        {
            Arc(position, rotation, from, 0, angle, radius, radius, color, duration, depthTest);
        }

        /// <summary>
        /// Draws a circular arc in 3D space.
        /// </summary>
        /// <param name="position">The center of the circle.</param>
        /// <param name="rotation">The rotation of the circle.</param>
        /// <param name="from">The direction of the point on the circle circumference, relative to the center, where the arc begins. This is often just transform.forward for example.</param>
        /// <param name="fromAngle">The starting angle of the arc, relative to 'from', in degrees.</param>
        /// <param name="toAngle">The ending angle of the arc, relative to 'from', in degrees.</param>
        /// <param name="innerRadius">The inner radius of the circle.</param>
        /// <param name="outerRadius">The outer radius of the circle.</param>
        /// <param name="color">The color.</param>
        /// <param name="duration">How long the arc should be visible, in seconds.</param>
        /// <param name="depthTest">Whether the arc be obscured by objects closer to the camera.</param>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Arc(Vector3 position, Quaternion rotation, Vector3 from, float fromAngle, float toAngle, float innerRadius, float outerRadius, Color color, float duration = 0, bool depthTest = true)
        {
            PrimitiveJob job;
            if (!TryAllocPrimitiveJob(out job, GL.TRIANGLE_STRIP, duration, depthTest, UnityEngine.Rendering.CullMode.Off, true))
                return;

            job.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
            job.useMatrix = true;
            job.useVertexColor = true;

            if (fromAngle != 0)
                fromAngle %= 360;

            if (toAngle != 0)
                toAngle %= 360;

            if (fromAngle > toAngle)
            {
                var temp = fromAngle;
                fromAngle = toAngle;
                toAngle = temp;
            }

            innerRadius = Mathf.Max(0, innerRadius);
            outerRadius = Mathf.Max(0, outerRadius);
            if (innerRadius > outerRadius)
            {
                var temp = innerRadius;
                innerRadius = outerRadius;
                outerRadius = temp;
            }

            const int circleSegments = 24;
            var rotationRightVector = job.matrix.GetColumn(0);
            var rotationUpVector = job.matrix.GetColumn(1);
            var startingTheta = Vector3.SignedAngle(rotationRightVector, from, rotationUpVector) * Mathf.Deg2Rad;
            var theta = startingTheta + fromAngle * Mathf.Deg2Rad; // start at this angle
            var thetaTarget = startingTheta + toAngle * Mathf.Deg2Rad; // end at this angle
            var thetaStep = (thetaTarget - theta) / circleSegments; // make a step of this size

            for (var n = 0; n <= circleSegments; ++n)
            {
                var v = new Vector3(Mathf.Cos(theta), 0, -Mathf.Sin(theta));
                job.AddVertex(new PrimitiveJob.Vertex() { position = v * innerRadius, color = color });
                job.AddVertex(new PrimitiveJob.Vertex() { position = v * outerRadius, color = color });
                theta += thetaStep;
            }

            job.Submit();
        }
    }
}
