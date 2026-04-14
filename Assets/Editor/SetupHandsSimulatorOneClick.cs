using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupHandsSimulatorOneClick
{
    [MenuItem("Tools/VR Sandbox/Setup XR Hands Simulator (One Click)")]
    public static void Setup()
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("Open MainScene before running setup.");
            return;
        }

        var xrOrigin = EnsureXrOriginFromMenuOrPrefab();
        if (xrOrigin == null)
        {
            Debug.LogError("Failed to create/find XR Origin. Ensure XR Interaction Toolkit is installed.");
            return;
        }

        var mainCamera = FindMainCameraUnder(xrOrigin.transform);
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera under XR Origin is missing.");
            return;
        }

        EnsureInteractionManager();
        EnsureInputActionManagerWithDefaults();
        var simulator = EnsureDeviceSimulatorPrefab();
        WireSimulatorCamera(simulator, mainCamera.transform);

        EnsureGrabbable("GrabableCube");
        EnsureGrabbable("GrabableSphere");
        EnsureGrabbable("GrabableCylinder");

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("XR simulator setup complete. Press Play and click Game view.");
    }

    private static GameObject EnsureXrOriginFromMenuOrPrefab()
    {
        var existing = GameObject.FindObjectsByType<Transform>(FindObjectsSortMode.None)
            .Select(t => t.gameObject)
            .FirstOrDefault(go => go.name.Contains("XR Origin", StringComparison.OrdinalIgnoreCase));
        if (existing != null)
            return existing;

        var xrPrefab = FindPrefab("XR Origin (XR Rig)") ?? FindPrefab("XR Origin");
        if (xrPrefab != null)
            return PrefabUtility.InstantiatePrefab(xrPrefab) as GameObject;

        return null;
    }

    private static Camera FindMainCameraUnder(Transform root)
    {
        return root.GetComponentsInChildren<Camera>(true)
            .FirstOrDefault(c => c.CompareTag("MainCamera") || c.name.Contains("Main Camera", StringComparison.OrdinalIgnoreCase));
    }

    private static void EnsureInteractionManager()
    {
        var type = Type.GetType("UnityEngine.XR.Interaction.Toolkit.XRInteractionManager, Unity.XR.Interaction.Toolkit");
        if (type == null) return;
        if (GameObject.FindObjectsByType(type, FindObjectsSortMode.None).Length > 0) return;
        new GameObject("XR Interaction Manager").AddComponent(type);
    }

    private static void EnsureInputActionManagerWithDefaults()
    {
        var managerType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.Inputs.InputActionManager, Unity.XR.Interaction.Toolkit");
        if (managerType == null) return;

        var manager = GameObject.FindObjectsByType(managerType, FindObjectsSortMode.None).FirstOrDefault() as Component;
        if (manager == null)
            manager = new GameObject("Input Action Manager").AddComponent(managerType);

        var assetsProp = managerType.GetField("m_ActionAssets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                       ?? managerType.GetField("actionAssets", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (assetsProp == null) return;

        var inputActionAssetType = Type.GetType("UnityEngine.InputSystem.InputActionAsset, Unity.InputSystem");
        if (inputActionAssetType == null) return;

        var current = assetsProp.GetValue(manager) as System.Collections.IList;
        if (current == null) return;

        var defaultAsset = FindAssetByName("XRI Default Input Actions", "t:InputActionAsset");
        if (defaultAsset != null && !current.Contains(defaultAsset))
            current.Add(defaultAsset);
    }

    private static Component EnsureDeviceSimulatorPrefab()
    {
        var simType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRDeviceSimulator, Unity.XR.Interaction.Toolkit");
        if (simType == null) return null;

        var existing = GameObject.FindObjectsByType(simType, FindObjectsSortMode.None).FirstOrDefault() as Component;
        if (existing != null) return existing;

        var prefab = FindPrefab("XR Device Simulator");
        if (prefab != null)
            return (PrefabUtility.InstantiatePrefab(prefab) as GameObject)?.GetComponent(simType);

        return new GameObject("XR Device Simulator").AddComponent(simType);
    }

    private static void WireSimulatorCamera(Component simulator, Transform cameraTransform)
    {
        if (simulator == null || cameraTransform == null) return;
        var t = simulator.GetType();
        var field = t.GetField("m_CameraTransform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                 ?? t.GetField("cameraTransform", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (field != null && field.FieldType == typeof(Transform))
            field.SetValue(simulator, cameraTransform);
    }

    private static void EnsureGrabbable(string objectName)
    {
        var go = GameObject.Find(objectName);
        if (go == null) return;

        var grabType = Type.GetType(
            "UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable, Unity.XR.Interaction.Toolkit");
        if (grabType != null && go.GetComponent(grabType) == null)
            go.AddComponent(grabType);
    }

    private static GameObject FindPrefab(string name)
    {
        var guid = AssetDatabase.FindAssets($"{name} t:Prefab").FirstOrDefault();
        if (string.IsNullOrEmpty(guid)) return null;
        var path = AssetDatabase.GUIDToAssetPath(guid);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    private static UnityEngine.Object FindAssetByName(string name, string filter)
    {
        var guid = AssetDatabase.FindAssets($"{name} {filter}").FirstOrDefault();
        if (string.IsNullOrEmpty(guid)) return null;
        return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(guid));
    }
}

