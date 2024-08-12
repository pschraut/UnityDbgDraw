// DbgDraw for Unity. Copyright (c) 2019-2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityDbgDraw
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#pragma warning disable IDE0018 // Variable declaration can be inlined
#pragma warning disable IDE0017 // Object initialization can be simplified
#pragma warning disable IDE1006 // Naming rule vilation

namespace Oddworm.Framework
{
    public partial class DbgDraw
    {
        /// <summary>
        /// Gets and sets whether DbgDraw is enabled.
        /// </summary>
        public static bool isEnabled = true;

        /// <summary>
        /// Gets whether DbgDraw is supported. Returns true in development mode builds and if the development mode setting is ticked editor, false otherwise.
        /// </summary>
        public static bool isSupported
        {
            get
            {
#if UNITY_EDITOR
                if (UnityEditor.EditorUserBuildSettings.development)
                    return true;
                return false;
#elif DEVELOPMENT_BUILD
                return true;
#else
                return false;
#endif
            }
        }

        const float kTau = 2 * Mathf.PI;
        static Color s_XAxisColor = new Color(219f / 255, 62f / 255, 29f / 255, .93f);
        static Color s_YAxisColor = new Color(154f / 255, 243f / 255, 72f / 255, .93f);
        static Color s_ZAxisColor = new Color(58f / 255, 122f / 255, 248f / 255, .93f);

        static bool isEnabledAndPlaying
        {
            get
            {
                if (!isEnabled || !isSupported)
                    return false;

#if UNITY_EDITOR
                if (!Application.isPlaying)
                    return false;

                //if (UnityEditor.EditorApplication.isPaused)
                //    return false;
#endif

                return true;
            }
        }

        static bool TryGetLineBatch(out LineBatchJob batch, bool depthTest, CullMode cullMode)
        {
            if (!isEnabledAndPlaying)
            {
                batch = new LineBatchJob();
                return false;
            }

            batch = instance.GetLineBatch(depthTest, cullMode);
            return true;
        }

        static bool TryAllocPrimitiveJob(out PrimitiveJob job, int primitiveType, float duration, bool depthTest, CullMode cullMode, bool shaded)
        {
            if (!isEnabledAndPlaying)
            {
                job = new PrimitiveJob();
                return false;
            }

            job = instance.AllocPrimitiveJob(primitiveType, duration, depthTest, cullMode, shaded);
            return true;
        }

        static bool TryAllocMeshJob(out MeshJob job, float duration, bool depthTest, CullMode cullMode, bool shaded)
        {
            //shaded = true;
            if (!isEnabledAndPlaying)
            {
                job = new MeshJob();
                return false;
            }

            job = instance.AllocMeshJob(duration, depthTest, cullMode, shaded);
            return true;
        }

        static Mesh CreateMesh(PrimitiveType type)
        {
            var go = GameObject.CreatePrimitive(type);
            var mesh = MonoBehaviour.Instantiate<Mesh>(go.GetComponent<MeshFilter>().sharedMesh);
            MonoBehaviour.Destroy(go);
            return mesh;
        }

        static void ReleaseOnDestroy(Mesh mesh)
        {
            instance.ReleaseOnDestroy(mesh);
        }

        struct LineBatchJob
        {
            public struct Line
            {
                public Vector3 a;
                public Vector3 b;
                public Color32 color;
                public float remainingDuration;
            }

            public List<Line> list;
            public Material material;

            public void AddLine(Vector3 a, Vector3 b, Color32 color, float duration)
            {
                var line = new Line();
                line.a = a;
                line.b = b;
                line.color = color;
                line.remainingDuration = duration;
                list.Add(line);
            }

            public void Submit()
            {
            }
        }

        struct PrimitiveJob
        {
            public struct Vertex
            {
                public Vector3 position;
                public Color32 color;
            }

            public int primitiveType; // GL.LINES, GL.TRIANGLES, and so on
            public List<PrimitiveJob.Vertex> list;
            public float remainingDuration;
            public bool depthTest;
            public Matrix4x4 matrix;
            public bool useMatrix;
            public bool useVertexColor;
            public Material material;

            readonly List<PrimitiveJob> m_Owner;

            public PrimitiveJob(List<PrimitiveJob> owner)
                : this()
            {
                m_Owner = owner;
            }

            public void AddVertex(Vertex vertex)
            {
                list.Add(vertex);
            }

            public void Submit()
            {
                m_Owner.Add(this);
            }
        }

        struct MeshJob
        {
            public float remainingDuration;
            public bool depthTest;
            public Matrix4x4 matrix;
            public Material material;
            public Mesh mesh;
            public Color color;

            readonly List<MeshJob> m_Owner;

            public MeshJob(List<MeshJob> owner)
                : this()
            {
                m_Owner = owner;
            }

            public void Submit()
            {
                m_Owner.Add(this);
            }
        }

        [DefaultExecutionOrder(int.MinValue)]
        class PreDbgDrawBehaviour : MonoBehaviour
        {
            [System.NonSerialized]
            public DbgDrawBehaviour debugDrawBehaviour;

            void Update()
            {
                if (debugDrawBehaviour != null)
                {
                    UnityEngine.Profiling.Profiler.BeginSample("DbgDraw.RemoveDeadJobs");
                    debugDrawBehaviour.RemoveDeadJobs();
                    UnityEngine.Profiling.Profiler.EndSample();
                }
            }
        }

        [DefaultExecutionOrder(int.MaxValue)]
        class DbgDrawBehaviour : MonoBehaviour
        {
            List<PrimitiveJob> m_PrimitiveJobs = new List<PrimitiveJob>(64);
            List<List<PrimitiveJob.Vertex>> m_VertexListCache = new List<List<PrimitiveJob.Vertex>>(64);
            List<MeshJob> m_MeshJobs = new List<MeshJob>(64);
            List<Object> m_ReleaseOnDestroy = new List<Object>();

            Material[,] m_ColoredMaterials = null; // index array via Material[ZTest,CullMode]
            Material[,] m_ShadedMaterials = null; // index array via Material[ZTest,CullMode]
            LineBatchJob[,] m_LineBatch = null;

#if UNITY_EDITOR
            int m_EditorPauseFrame = -1;
#endif

            void Awake()
            {
                m_ColoredMaterials = CreateMaterialArray("Hidden/Internal-Colored"); // Added to "Always Included Shaders" in the Graphics settings by Unity
                m_ShadedMaterials = CreateMaterialArray("Hidden/DbgDraw-Shaded"); // Added to "Always Included Shaders" in the Graphics settings by DbgDrawBuildProcessor

                m_LineBatch = new LineBatchJob[2, 3];
                for (var y = 0; y < m_LineBatch.GetLength(0); ++y)
                {
                    for (var x = 0; x < m_LineBatch.GetLength(1); ++x)
                    {
                        m_LineBatch[y, x].list = new List<LineBatchJob.Line>();
                        m_LineBatch[y, x].material = m_ColoredMaterials[y, x];
                    }
                }

                gameObject.AddComponent<PreDbgDrawBehaviour>().debugDrawBehaviour = this;
                RenderPipelineManager.endCameraRendering += OnRenderPipelineManagerEndCameraRendering;
            }

            void OnRenderPipelineManagerEndCameraRendering(ScriptableRenderContext context, Camera camera)
            {
                if (!isActiveAndEnabled)
                    return;

                UnityEngine.Profiling.Profiler.BeginSample("DbgDraw.Render", camera);
                Render(camera);
                UnityEngine.Profiling.Profiler.EndSample();
            }

            Material[,] CreateMaterialArray(string shaderName)
            {
                var materials = new Material[2, 3];

                // depth test off
                materials[0, (int)CullMode.Off] = CreateMaterial(shaderName, CullMode.Off, false);
                materials[0, (int)CullMode.Front] = CreateMaterial(shaderName, CullMode.Front, false);
                materials[0, (int)CullMode.Back] = CreateMaterial(shaderName, CullMode.Back, false);

                // depth test on
                materials[1, (int)CullMode.Off] = CreateMaterial(shaderName, CullMode.Off, true);
                materials[1, (int)CullMode.Front] = CreateMaterial(shaderName, CullMode.Front, true);
                materials[1, (int)CullMode.Back] = CreateMaterial(shaderName, CullMode.Back, true);

                return materials;
            }

            Material CreateMaterial(string shaderName, CullMode cullMode, bool depthTest)
            {
                var shader = Shader.Find(shaderName);
                if (shader == null)
                {
                    // This should not occur, but if it does, we try to find a fallback shader
                    Debug.LogError($"{nameof(DbgDraw)}: Cannot find shader '{shaderName}'. {nameof(DbgDraw)} will not work correctly.");
                    foreach (var fallback in new[] { "Hidden/Internal-Colored", "Unlit/Color" })
                    {
                        shader = Shader.Find(shaderName);
                        if (shader != null)
                            break;
                    }
                }

                var material = new Material(shader);

                material.SetColor("_Color", Color.white);
                material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                material.SetInt("_Cull", (int)cullMode);
                material.SetInt("_ZWrite", 0);
                if (!depthTest)
                    material.SetInt("_ZTest", 0);

                ReleaseOnDestroy(material);
                return material;
            }

            public void ReleaseOnDestroy(UnityEngine.Object obj)
            {
                m_ReleaseOnDestroy.Add(obj);
            }

            void OnDestroy()
            {
                for (var n = 0; n < m_ReleaseOnDestroy.Count; ++n)
                {
                    if (m_ReleaseOnDestroy[n] != null)
                        Destroy(m_ReleaseOnDestroy[n]);
                    m_ReleaseOnDestroy[n] = null;
                }
                m_ReleaseOnDestroy.Clear();

                m_ReleaseOnDestroy = null;
                m_PrimitiveJobs = null;
                m_VertexListCache = null;
                m_MeshJobs = null;
                m_ShadedMaterials = null;
                m_ColoredMaterials = null;
                m_LineBatch = null;
                RenderPipelineManager.endCameraRendering -= OnRenderPipelineManagerEndCameraRendering;
            }

            void OnRenderObject()
            {
                var camera = Camera.current;
                UnityEngine.Profiling.Profiler.BeginSample("DbgDraw.Render", camera);
                Render(camera);
                UnityEngine.Profiling.Profiler.EndSample();
            }

            void Render(Camera camera)
            {
                if (!isEnabled || camera == null || !isSupported)
                    return;
                //if (!Application.isPlaying)
                //    return;

                // Render only in game or scene view. If we don't to this, we also render
                // stuff in the frame debugger mesh preview window for example.
                var validCamera = false;
                if (camera.cameraType == CameraType.Game && camera.CompareTag("MainCamera")) validCamera = true;
                if (camera.cameraType == CameraType.SceneView) validCamera = true;
                if (!validCamera)
                    return;

                // If you pause and then unpause the Unity editor, Time.unscaledDeltaTime gets advanced
                // the amount of time you paused the editor. In this case, any jobs with a 'duration' often just vanish.
                // Therefore we detect when the editor gets unpaused and for this frame, we ignore the deltatime!
                var deltaTime = Time.unscaledDeltaTime;

#if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPaused)
                    m_EditorPauseFrame = Time.frameCount;

                if (!UnityEditor.EditorApplication.isPaused && m_EditorPauseFrame == Time.frameCount - 1)
                    deltaTime = 0;
#endif

                UnityEngine.Profiling.Profiler.BeginSample("DbgDraw.DrawMeshJobs");
                DrawMeshJobs(deltaTime);
                UnityEngine.Profiling.Profiler.EndSample();

                UnityEngine.Profiling.Profiler.BeginSample("DbgDraw.DrawPrimitiveJobs");
                DrawPrimitiveJobs(deltaTime);
                UnityEngine.Profiling.Profiler.EndSample();

                UnityEngine.Profiling.Profiler.BeginSample("DbgDraw.DrawLines");
                DrawLines(deltaTime);
                UnityEngine.Profiling.Profiler.EndSample();
            }

            void ResetMaterialColors()
            {
                ResetMaterialColors(m_ColoredMaterials);
                ResetMaterialColors(m_ShadedMaterials);
            }

            void ResetMaterialColors(Material[,] materials)
            {
                for (var y = 0; y < materials.GetLength(0); y++)
                {
                    for (var x = 0; x < materials.GetLength(1); x++)
                    {
                        if (materials[y, x] != null)
                            materials[y, x].color = Color.white;
                    }
                }
            }

            void DrawPrimitiveJobs(float deltaTime)
            {
                ResetMaterialColors();
                Material material = null;

                GL.PushMatrix();

                for (var k = 0; k < m_PrimitiveJobs.Count; ++k)
                {
                    var job = m_PrimitiveJobs[k];

                    if (job.useMatrix)
                    {
                        GL.PushMatrix();
                        GL.MultMatrix(job.matrix);
                    }

                    if (job.material != material)
                    {
                        material = job.material;
                        material.color = Color.white;
                        material.SetPass(0);
                    }

                    GL.Begin(job.primitiveType);

                    for (var n = 0; n < job.list.Count; ++n)
                    {
                        if (job.useVertexColor || n == 0)
                            GL.Color(job.list[n].color);

                        GL.Vertex(job.list[n].position);
                    }

                    GL.End();

                    if (job.useMatrix)
                        GL.PopMatrix();

                    job.remainingDuration -= deltaTime;
                    m_PrimitiveJobs[k] = job;
                }

                GL.PopMatrix();
            }

            void DrawMeshJobs(float deltaTime)
            {
                ResetMaterialColors();
                Material material = null;

                for (var k = 0; k < m_MeshJobs.Count; ++k)
                {
                    var job = m_MeshJobs[k];

                    if (job.material != material || material.color != job.color)
                    {
                        material = job.material;
                        material.color = job.color;
                        material.SetPass(0);
                    }

                    Graphics.DrawMeshNow(job.mesh, job.matrix);

                    job.remainingDuration -= deltaTime;
                    m_MeshJobs[k] = job;
                }
            }

            void DrawLines(float deltaTime)
            {
                ResetMaterialColors();
                GL.PushMatrix();

                for (var y = 0; y < m_LineBatch.GetLength(0); ++y)
                {
                    for (var x = 0; x < m_LineBatch.GetLength(1); ++x)
                    {
                        var list = m_LineBatch[y, x].list;
                        if (list.Count == 0)
                            continue;

                        var material = m_LineBatch[y, x].material;
                        if (material == null)
                            continue;

                        material.color = Color.white;
                        material.SetPass(0);

                        GL.Begin(GL.LINES);

                        for (var n = 0; n < list.Count; ++n)
                        {
                            var line = list[n];

                            GL.Color(line.color);
                            GL.Vertex(line.a);
                            GL.Vertex(line.b);

                            line.remainingDuration -= deltaTime;
                            list[n] = line;
                        }

                        GL.End();
                    }
                }

                GL.PopMatrix();
            }

            List<PrimitiveJob.Vertex> GetCachedVertexList()
            {
                if (m_VertexListCache.Count == 0)
                    return new List<PrimitiveJob.Vertex>(32);

                var list = m_VertexListCache[m_VertexListCache.Count - 1];
                m_VertexListCache.RemoveAt(m_VertexListCache.Count - 1);
                return list;
            }

            public void RemoveDeadJobs()
            {
                for (var n = m_PrimitiveJobs.Count - 1; n >= 0; --n)
                {
                    var job = m_PrimitiveJobs[n];
                    if (job.remainingDuration <= 0)
                    {
                        if (job.list != null)
                        {
                            job.list.Clear();
                            m_VertexListCache.Add(job.list);
                        }

                        m_PrimitiveJobs.RemoveAt(n);
                    }
                }

                for (var y = 0; y < m_LineBatch.GetLength(0); ++y)
                {
                    for (var x = 0; x < m_LineBatch.GetLength(1); ++x)
                    {
                        var list = m_LineBatch[y, x].list;
                        for (var n = list.Count - 1; n >= 0; --n)
                        {
                            var line = list[n];
                            if (line.remainingDuration <= 0)
                                list.RemoveAt(n);
                        }
                    }
                }

                for (var n = m_MeshJobs.Count - 1; n >= 0; --n)
                {
                    var job = m_MeshJobs[n];
                    if (job.remainingDuration <= 0)
                        m_MeshJobs.RemoveAt(n);
                }
            }

            public LineBatchJob GetLineBatch(bool depthTest, CullMode cullMode)
            {
                var batch = m_LineBatch[depthTest ? 1 : 0, (int)cullMode];
                return batch;
            }

            public PrimitiveJob AllocPrimitiveJob(int primitiveType, float duration, bool depthTest, CullMode cullMode, bool shaded)
            {
                var job = new PrimitiveJob(m_PrimitiveJobs);
                job.primitiveType = primitiveType;
                job.remainingDuration = duration;
                job.depthTest = depthTest;

                if (shaded)
                    job.material = m_ShadedMaterials[depthTest ? 1 : 0, (int)cullMode];
                else
                    job.material = m_ColoredMaterials[depthTest ? 1 : 0, (int)cullMode];

                job.list = GetCachedVertexList();
                return job;
            }

            public MeshJob AllocMeshJob(float duration, bool depthTest, CullMode cullMode, bool shaded)
            {
                var job = new MeshJob(m_MeshJobs);
                job.remainingDuration = duration;
                job.depthTest = depthTest;
                job.color = Color.white;

                if (shaded)
                    job.material = m_ShadedMaterials[depthTest ? 1 : 0, (int)cullMode];
                else
                    job.material = m_ColoredMaterials[depthTest ? 1 : 0, (int)cullMode];

                return job;
            }
        }

        static DbgDrawBehaviour s_Instance;
        static DbgDrawBehaviour instance
        {
            get
            {
                if (s_Instance != null)
                    return s_Instance;

#if UNITY_2023_1_OR_NEWER
                s_Instance = DbgDrawBehaviour.FindFirstObjectByType<DbgDrawBehaviour>();
#else
                s_Instance = DbgDrawBehaviour.FindObjectOfType<DbgDrawBehaviour>();
#endif
                if (s_Instance == null)
                {
                    s_Instance = new GameObject("DbgDraw").AddComponent<DbgDrawBehaviour>();
                    s_Instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                }

                return s_Instance;
            }
        }
    }
}
