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
        public static void WireCapsule(Vector3 position, Quaternion rotation, float radius, float height, Color color, float duration = 0, bool depthTest = true)
        {
            if (!isEnabledAndPlaying)
                return;

            radius = Mathf.Max(0, radius);
            radius *= 2;

            height -= radius; // subtract the hemisphere on both sides
            height = Mathf.Max(0, height);


            WireHemisphere(position + rotation * new Vector3(0, height * 0.5f, 0), rotation, Vector3.one * radius, color, duration, depthTest);
            WireTube(position, rotation, new Vector3(radius, height, radius), color, duration, depthTest);
            WireHemisphere(position - rotation * new Vector3(0, height * 0.5f, 0), rotation * Quaternion.AngleAxis(180, Vector3.right), Vector3.one * radius, color, duration, depthTest);
        }
    }
}
