using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace IrsikSoftware.LogSmith.Editor
{
    /// <summary>
    /// Generates sample scenes for render pipeline demos.
    /// Run via Unity batch mode: -executeMethod IrsikSoftware.LogSmith.Editor.SampleSceneGenerator.GenerateAllScenes
    /// </summary>
    public static class SampleSceneGenerator
    {
        public static void GenerateAllScenes()
        {
            try
            {
                Debug.Log("[SampleSceneGenerator] Starting scene generation...");

                GenerateScene("Built-in", "BuiltInDemo");
                GenerateScene("URP", "URPDemo");
                GenerateScene("HDRP", "HDRPDemo");

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("[SampleSceneGenerator] All sample scenes generated successfully!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SampleSceneGenerator] Failed to generate scenes: {ex.Message}\n{ex.StackTrace}");
                EditorApplication.Exit(1);
            }
        }

        private static void GenerateScene(string pipelineName, string sceneName)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = sceneName;

            // Configure Main Camera
            var camera = Camera.main;
            if (camera != null)
            {
                camera.transform.position = new Vector3(0, 2, -5);
                camera.transform.rotation = Quaternion.Euler(10, 0, 0);
                camera.clearFlags = CameraClearFlags.Skybox;
            }

            // Add directional light with better settings
            var light = GameObject.Find("Directional Light");
            if (light == null)
            {
                light = new GameObject("Directional Light");
                var lightComp = light.AddComponent<Light>();
                lightComp.type = LightType.Directional;
                lightComp.intensity = 1f;
                lightComp.shadows = LightShadows.Soft;
                light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            // Add a ground plane with material
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "Ground";
            plane.transform.position = Vector3.zero;
            plane.transform.localScale = new Vector3(5f, 1f, 5f);

            // Add a cube as a reference object
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Reference Cube";
            cube.transform.position = new Vector3(0, 0.5f, 0);

            // Add an empty GameObject for demo script (user will add the component manually)
            var demoObject = new GameObject($"{pipelineName}DemoScript");
            demoObject.transform.position = Vector3.zero;

            // Add a TextAsset note explaining setup
            var noteObject = new GameObject("README");
            noteObject.SetActive(false); // Hide it in hierarchy by default
            noteObject.transform.position = Vector3.zero;

            // Save scene
            string scenePath = $"Packages/com.irsiksoftware.logsmith/Samples~/{pipelineName}/{sceneName}.unity";
            string dirPath = System.IO.Path.GetDirectoryName(scenePath);

            if (!System.IO.Directory.Exists(dirPath))
            {
                System.IO.Directory.CreateDirectory(dirPath);
            }

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[SampleSceneGenerator] {pipelineName} scene created: {scenePath}");
        }
    }
}
