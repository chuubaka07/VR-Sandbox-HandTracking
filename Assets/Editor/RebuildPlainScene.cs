using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RebuildPlainScene
{
    [MenuItem("Tools/VR Sandbox/Rebuild Main Scene (Plain)")]
    public static void Rebuild()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateLighting();
        CreateGround();
        CreateTable();
        CreateColoredObjects();

        const string scenePath = "Assets/Scenes/MainScene.unity";
        EnsureFolder("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.Refresh();

        Debug.Log("MainScene rebuilt (plain): table + 3 colored objects.");
    }

    private static void CreateLighting()
    {
        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static void CreateGround()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(2f, 1f, 2f);
    }

    private static void CreateTable()
    {
        var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = "Table";
        table.transform.position = new Vector3(0f, 0.5f, 0f);
        table.transform.localScale = new Vector3(2f, 0.2f, 2f);

        var wood = GetOrCreateMaterial("Wood", new Color(0.55f, 0.35f, 0.2f, 1f));
        ApplyMaterial(table, wood);
    }

    private static void CreateColoredObjects()
    {
        var red = GetOrCreateMaterial("Red", new Color(0.85f, 0.2f, 0.2f, 1f));
        var green = GetOrCreateMaterial("Green", new Color(0.2f, 0.85f, 0.3f, 1f));
        var blue = GetOrCreateMaterial("Blue", new Color(0.2f, 0.45f, 0.95f, 1f));

        CreateObject(PrimitiveType.Cube, "GrabableCube", new Vector3(-0.4f, 0.8f, 0f), new Vector3(0.2f, 0.2f, 0.2f), red);
        CreateObject(PrimitiveType.Sphere, "GrabableSphere", new Vector3(0f, 0.8f, 0f), new Vector3(0.2f, 0.2f, 0.2f), green);
        CreateObject(PrimitiveType.Cylinder, "GrabableCylinder", new Vector3(0.4f, 0.8f, 0f), new Vector3(0.2f, 0.2f, 0.2f), blue);
    }

    private static void CreateObject(PrimitiveType type, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.AddComponent<Rigidbody>();
        ApplyMaterial(go, mat);
    }

    private static void ApplyMaterial(GameObject go, Material mat)
    {
        var rend = go.GetComponent<Renderer>();
        if (rend != null && mat != null)
            rend.sharedMaterial = mat;
    }

    private static Material GetOrCreateMaterial(string name, Color color)
    {
        EnsureFolder("Assets/Materials");
        var path = $"Assets/Materials/{name}.mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
            return existing;

        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = new Material(shader) { color = color };
        AssetDatabase.CreateAsset(mat, path);
        AssetDatabase.SaveAssets();
        return mat;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        var folder = Path.GetFileName(path);
        if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(folder))
            return;

        if (!AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(parent, folder);
    }
}

