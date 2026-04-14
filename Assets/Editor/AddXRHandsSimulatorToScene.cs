using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AddXRHandsSimulatorToScene
{
    [MenuItem("Tools/VR Sandbox/Add XR Hands Simulator To Scene")]
    public static void AddSetupToCurrentScene()
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("Open a scene before running XR setup.");
            return;
        }

        // Create or reuse XR Origin
        var xrOriginGo = EnsureXROrigin();
        if (xrOriginGo == null)
        {
            Debug.LogError("XROrigin type not found. Install XR Interaction Toolkit + XR Core Utils.");
            return;
        }

        EnsureXRInteractionManager();
        EnsureXRDeviceSimulator();
        RemoveStandaloneMainCamera();

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("XR Hands simulator setup added to scene.");
    }

    private static GameObject EnsureXROrigin()
    {
        var xrOriginType = Type.GetType("Unity.XR.CoreUtils.XROrigin, Unity.XR.CoreUtils");
        if (xrOriginType == null)
            return null;

        var existing = UnityEngine.Object.FindObjectsByType(xrOriginType, FindObjectsSortMode.None).FirstOrDefault() as Component;
        if (existing != null)
            return existing.gameObject;

        var root = new GameObject("XR Origin");
        var xrOrigin = root.AddComponent(xrOriginType);

        var cameraOffset = new GameObject("Camera Offset");
        cameraOffset.transform.SetParent(root.transform, false);
        cameraOffset.transform.localPosition = Vector3.zero;

        var cameraGo = new GameObject("Main Camera");
        cameraGo.tag = "MainCamera";
        cameraGo.transform.SetParent(cameraOffset.transform, false);
        cameraGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        cameraGo.AddComponent<Camera>();
        cameraGo.AddComponent<AudioListener>();

        // Try to add TrackedPoseDriver (new Input System) if present
        var trackedPoseType = Type.GetType("UnityEngine.InputSystem.XR.TrackedPoseDriver, Unity.InputSystem");
        if (trackedPoseType != null)
            cameraGo.AddComponent(trackedPoseType);

        // Wire fields/properties if available
        TryAssignXROriginReference(xrOrigin, "CameraFloorOffsetObject", cameraOffset.transform);
        TryAssignXROriginReference(xrOrigin, "Camera", cameraGo.GetComponent<Camera>());
        TryAssignXROriginReference(xrOrigin, "Origin", root.transform);

        return root;
    }

    private static void TryAssignXROriginReference(Component xrOrigin, string memberName, object value)
    {
        var type = xrOrigin.GetType();
        var prop = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop != null && prop.CanWrite && prop.PropertyType.IsInstanceOfType(value))
        {
            prop.SetValue(xrOrigin, value);
            return;
        }

        var field = type.GetField("m_" + memberName, BindingFlags.Instance | BindingFlags.NonPublic)
                   ?? type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null && field.FieldType.IsInstanceOfType(value))
            field.SetValue(xrOrigin, value);
    }

    private static void EnsureXRInteractionManager()
    {
        var managerType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.XRInteractionManager, Unity.XR.Interaction.Toolkit");
        if (managerType == null)
            return;

        var existing = UnityEngine.Object.FindObjectsByType(managerType, FindObjectsSortMode.None).FirstOrDefault() as Component;
        if (existing != null)
            return;

        var go = new GameObject("XR Interaction Manager");
        go.AddComponent(managerType);
    }

    private static void EnsureXRDeviceSimulator()
    {
        var simulatorType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRDeviceSimulator, Unity.XR.Interaction.Toolkit");
        if (simulatorType == null)
        {
            Debug.LogWarning("XRDeviceSimulator type not found. Install XR Interaction Toolkit.");
            return;
        }

        var existing = UnityEngine.Object.FindObjectsByType(simulatorType, FindObjectsSortMode.None).FirstOrDefault() as Component;
        if (existing != null)
            return;

        // Try to instantiate imported sample prefab first
        var sampleGuids = AssetDatabase.FindAssets("XR Device Simulator t:Prefab");
        if (sampleGuids != null && sampleGuids.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(sampleGuids[0]);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                PrefabUtility.InstantiatePrefab(prefab);
                return;
            }
        }

        // Fallback: create plain simulator object
        var simulatorGo = new GameObject("XR Device Simulator");
        simulatorGo.AddComponent(simulatorType);
    }

    private static void RemoveStandaloneMainCamera()
    {
        var allCameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (var cam in allCameras)
        {
            if (cam == null)
                continue;

            // Keep only cameras inside XR Origin hierarchy, remove all others.
            var root = cam.transform.root;
            var isUnderXrOrigin = root != null && root.name == "XR Origin";
            if (!isUnderXrOrigin)
            {
                UnityEngine.Object.DestroyImmediate(cam.gameObject);
            }
        }
    }
}

