// DbgDraw for Unity. Copyright (c) 2024 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityDbgDraw
#pragma warning disable IDE0018 // Variable declaration can be inlined
#pragma warning disable IDE0017 // Object initialization can be simplified
#pragma warning disable IDE1006 // Naming rule vilation

using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;

namespace Oddworm.EditorFramework
{

    // The DbgDrawBuildProcessor is responsible for adding the required shaders to the GraphicsSettings > Always Included Shaders list.
    // Without the shaders added to this list, they can't be found by the application at runtime, eg a Windows or Android build.
    class DbgDrawBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => -100;

        public void OnPreprocessBuild(BuildReport report)
        {
            var asset = AssetDatabase.LoadMainAssetAtPath("ProjectSettings/GraphicsSettings.asset");
            if (asset == null)
            {
                Debug.LogError($"{nameof(DbgDrawBuildProcessor)}: Cannot load GraphicsSettings.asset");
                return;
            }

            var serObj = new SerializedObject(asset);
            serObj.UpdateIfRequiredOrScript();

            var includedShadersProp = serObj.FindProperty("m_AlwaysIncludedShaders");
            if (includedShadersProp == null)
            {
                Debug.LogError($"{nameof(DbgDrawBuildProcessor)}: Cannot find m_AlwaysIncludedShaders property");
                return;
            }

            TryAddShader(includedShadersProp, "Hidden/DbgDraw-Shaded");

            serObj.ApplyModifiedPropertiesWithoutUndo();
        }

        void TryAddShader(SerializedProperty includedShadersProp, string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogError($"{nameof(DbgDrawBuildProcessor)}: Cannot find shader with name '{shaderName}'");
                return;
            }

            for (int i = 0, count = includedShadersProp.arraySize; i < count; ++i)
            {
                var element = includedShadersProp.GetArrayElementAtIndex(i);
                if (element.objectReferenceValue == shader)
                    return; // Shader added already
            }

            includedShadersProp.arraySize++;
            var shaderProp = includedShadersProp.GetArrayElementAtIndex(includedShadersProp.arraySize - 1);
            shaderProp.objectReferenceValue = shader;
        }
    }
}
