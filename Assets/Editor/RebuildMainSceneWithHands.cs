using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RebuildMainSceneWithHands
{
    [MenuItem("Tools/VR Sandbox/Rebuild Main Scene (Two Hands)")]
    public static void Rebuild()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateLighting();
        CreateGroundAndTable();
        CreateGrabbableSet();
        EnsureXRCoreRig();
        EnsureXRDeviceSimulator();
        EnsureXRInteractionManager();
        EnsureInputActionManager();

        const string scenePath = "Assets/Scenes/MainScene.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.Refresh();
        Debug.Log("MainScene rebuilt: two-hand XR simulator setup + grabbable objects.");
    }

    private static void CreateLighting()
    {
        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static void CreateGroundAndTable()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(2f, 1f, 2f);

        var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = "Table";
        table.transform.position = new Vector3(0f, 0.5f, 0f);
        table.transform.localScale = new Vector3(2f, 0.2f, 2f);
    }

    private static void CreateGrabbableSet()
    {
        CreateGrabbablePrimitive("GrabableCube", PrimitiveType.Cube, new Vector3(-0.4f, 0.8f, 0f), new Vector3(0.2f, 0.2f, 0.2f));
        CreateGrabbablePrimitive("GrabableSphere", PrimitiveType.Sphere, new Vector3(0f, 0.8f, 0f), new Vector3(0.2f, 0.2f, 0.2f));
        CreateGrabbablePrimitive("GrabableCylinder", PrimitiveType.Cylinder, new Vector3(0.4f, 0.8f, 0f), new Vector3(0.2f, 0.2f, 0.2f));
    }

    private static void CreateGrabbablePrimitive(string objectName, PrimitiveType type, Vector3 position, Vector3 scale)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = objectName;
        go.transform.position = position;
        go.transform.localScale = scale;

        var rb = go.AddComponent<Rigidbody>();
        rb.mass = 1f;

        // Add XR grab interactable if XRI is installed.
        var grabType = Type.GetType(
            "UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable, Unity.XR.Interaction.Toolkit");
        if (grabType != null && go.GetComponent(grabType) == null)
        {
            go.AddComponent(grabType);
        }
    }

    private static void EnsureXRCoreRig()
    {
        // Prefer XRI Starter Assets prefab if available (already has left/right setup).
        var xrOriginPrefab = FindPrefabByName("XR Origin (XR Rig)")
                            ?? FindPrefabByName("XR Origin");

        GameObject xrOriginInstance = null;
        if (xrOriginPrefab != null)
        {
            xrOriginInstance = PrefabUtility.InstantiatePrefab(xrOriginPrefab) as GameObject;
        }

        if (xrOriginInstance == null)
        {
            xrOriginInstance = new GameObject("XR Origin");
            var cameraOffset = new GameObject("Camera Offset");
            cameraOffset.transform.SetParent(xrOriginInstance.transform, false);

            var cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            cam.transform.SetParent(cameraOffset.transform, false);
            cam.transform.localPosition = new Vector3(0f, 1.6f, -2.5f);
            cam.transform.localRotation = Quaternion.identity;
            cam.AddComponent<Camera>();
            cam.AddComponent<AudioListener>();

            // Create explicit left/right hand anchors for simulator visuals/scripts.
            var left = new GameObject("Left Hand Tracking");
            left.transform.SetParent(cameraOffset.transform, false);
            left.transform.localPosition = new Vector3(-0.2f, 1.4f, 0.5f);

            var right = new GameObject("Right Hand Tracking");
            right.transform.SetParent(cameraOffset.transform, false);
            right.transform.localPosition = new Vector3(0.2f, 1.4f, 0.5f);
        }
    }

    private static void EnsureXRDeviceSimulator()
    {
        var simulatorPrefab = FindPrefabByName("XR Device Simulator");
        if (simulatorPrefab != null)
        {
            PrefabUtility.InstantiatePrefab(simulatorPrefab);
            return;
        }

        // Fallback: create simulator component directly.
        var simulatorType = Type.GetType(
            "UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRDeviceSimulator, Unity.XR.Interaction.Toolkit");
        if (simulatorType != null)
        {
            var sim = new GameObject("XR Device Simulator");
            sim.AddComponent(simulatorType);
        }
    }

    private static void EnsureXRInteractionManager()
    {
        var managerType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.XRInteractionManager, Unity.XR.Interaction.Toolkit");
        if (managerType == null)
            return;

        if (UnityEngine.Object.FindObjectsByType(managerType, FindObjectsSortMode.None).Length == 0)
        {
            var go = new GameObject("XR Interaction Manager");
            go.AddComponent(managerType);
        }
    }

    private static void EnsureInputActionManager()
    {
        var managerType = Type.GetType(
            "UnityEngine.XR.Interaction.Toolkit.Inputs.InputActionManager, Unity.XR.Interaction.Toolkit");
        if (managerType == null)
            return;

        if (UnityEngine.Object.FindObjectsByType(managerType, FindObjectsSortMode.None).Length == 0)
        {
            var go = new GameObject("Input Action Manager");
            go.AddComponent(managerType);
        }
    }

    private static GameObject FindPrefabByName(string prefabName)
    {
        var guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");
        if (guids == null || guids.Length == 0)
            return null;

        // Prefer samples/starter assets if multiple candidates exist.
        var path = guids
            .Select(AssetDatabase.GUIDToAssetPath)
            .OrderByDescending(p => p.IndexOf("Samples", StringComparison.OrdinalIgnoreCase) >= 0)
            .FirstOrDefault();

        return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }
}

