using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CreateMainSceneEditor
{
    [MenuItem("Tools/VR Sandbox/Create Main Scene")]
    public static void CreateMainScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Directional light
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Main Camera (needed so Game view can render)
        var cameraGO = new GameObject("Main Camera");
        cameraGO.tag = "MainCamera";
        var camera = cameraGO.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Skybox;
        camera.fieldOfView = 60f;
        cameraGO.transform.position = new Vector3(0f, 1.6f, -2.5f);
        cameraGO.transform.rotation = Quaternion.Euler(10f, 0f, 0f);
        cameraGO.AddComponent<AudioListener>();

        // Ground
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(1f, 1f, 1f);

        // Table
        var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = "Table";
        table.transform.position = new Vector3(0f, 0.5f, 0f);
        table.transform.localScale = new Vector3(2f, 0.2f, 2f);

        // Grabable cube
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "GrabableCube";
        cube.tag = "Untagged";
        cube.transform.position = new Vector3(-0.4f, 0.8f, 0f);
        cube.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        cube.AddComponent<Rigidbody>();

        // Grabable sphere
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "GrabableSphere";
        sphere.tag = "Untagged";
        sphere.transform.position = new Vector3(0f, 0.8f, 0f);
        sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        sphere.AddComponent<Rigidbody>();

        // Grabable cylinder
        var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = "GrabableCylinder";
        cylinder.tag = "Untagged";
        cylinder.transform.position = new Vector3(0.4f, 0.8f, 0f);
        cylinder.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        cylinder.AddComponent<Rigidbody>();

        const string scenePath = "Assets/Scenes/MainScene.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.Refresh();

        Debug.Log($"Main scene created and saved to: {scenePath}");
    }
}

