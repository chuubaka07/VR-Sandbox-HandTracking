using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Best-effort editor-time setup for XR packages.
/// Uses reflection to avoid hard dependencies if packages aren't installed yet.
/// </summary>
[InitializeOnLoad]
public static class AutoXRProjectSetup
{
    private const string HasRunKey = "VR_Sandbox_HandTracking_AutoXRSetup_HasRun";

    static AutoXRProjectSetup()
    {
        EditorApplication.delayCall += TrySetupOnce;
    }

    private static void TrySetupOnce()
    {
        if (SessionState.GetBool(HasRunKey, false))
            return;

        SessionState.SetBool(HasRunKey, true);

        try
        {
            // XR Plug-in Management presence check
            var xrGeneralSettingsType = Type.GetType("UnityEngine.XR.Management.XRGeneralSettings, Unity.XR.Management");
            if (xrGeneralSettingsType == null)
            {
                Debug.Log(
                    "[AutoXRProjectSetup] XR Plug-in Management not found yet. " +
                    "Install packages from Packages/manifest.json and reopen the project.");
                return;
            }

            // Ensure Input System is enabled (optional but recommended for XR samples).
            EnableNewInputSystemIfAvailable();

            // Try enabling OpenXR loader for Standalone + Android build targets.
            EnableOpenXRLoaderForBuildTargetGroup(BuildTargetGroup.Standalone);
            EnableOpenXRLoaderForBuildTargetGroup(BuildTargetGroup.Android);

            // Try enabling Hand Interaction Profile feature in OpenXR settings (best-effort).
            EnableOpenXRHandInteractionProfile(BuildTargetGroup.Standalone);
            EnableOpenXRHandInteractionProfile(BuildTargetGroup.Android);

            Debug.Log("[AutoXRProjectSetup] XR setup completed (best-effort).");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[AutoXRProjectSetup] XR setup failed (non-fatal): {ex}");
        }
    }

    private static void EnableNewInputSystemIfAvailable()
    {
        // Tries to set PlayerSettings.activeInputHandler = InputSystemPackage if API exists.
        var playerSettingsType = typeof(PlayerSettings);
        var prop = playerSettingsType.GetProperty("activeInputHandler", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop == null)
            return;

        try
        {
            // Unity uses an enum internally; set to "InputSystemPackage" if present.
            var enumType = prop.PropertyType;
            var names = Enum.GetNames(enumType);
            var desiredName = names.FirstOrDefault(n => string.Equals(n, "InputSystemPackage", StringComparison.OrdinalIgnoreCase))
                              ?? names.FirstOrDefault(n => n.IndexOf("InputSystem", StringComparison.OrdinalIgnoreCase) >= 0);
            if (desiredName == null)
                return;

            var value = Enum.Parse(enumType, desiredName);
            prop.SetValue(null, value);
        }
        catch
        {
            // ignore
        }
    }

    private static void EnableOpenXRLoaderForBuildTargetGroup(BuildTargetGroup group)
    {
        // XRPackageMetadataStore.AssignLoader(...) etc live in UnityEditor.XR.Management
        var xrPkgStoreType = Type.GetType("UnityEditor.XR.Management.XRPackageMetadataStore, Unity.XR.Management.Editor");
        var xrSettingsType = Type.GetType("UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget, Unity.XR.Management.Editor");
        if (xrPkgStoreType == null || xrSettingsType == null)
            return;

        // Loaders live in OpenXR package
        var openXrLoaderType = Type.GetType("UnityEngine.XR.OpenXR.OpenXRLoader, Unity.XR.OpenXR");
        if (openXrLoaderType == null)
            return;

        // Call: XRPackageMetadataStore.AssignLoader(XRLoader, BuildTargetGroup)
        var assignLoader = xrPkgStoreType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m => m.Name == "AssignLoader" && m.GetParameters().Length == 2);

        if (assignLoader == null)
            return;

        // Need an instance of the loader asset
        var loaderAsset = ScriptableObject.CreateInstance(openXrLoaderType);
        try
        {
            assignLoader.Invoke(null, new object[] { loaderAsset, group });
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(loaderAsset);
        }
    }

    private static void EnableOpenXRHandInteractionProfile(BuildTargetGroup group)
    {
        // OpenXRSettings.GetSettingsForBuildTargetGroup(group)
        var openXrSettingsType = Type.GetType("UnityEngine.XR.OpenXR.OpenXRSettings, Unity.XR.OpenXR");
        if (openXrSettingsType == null)
            return;

        var getSettings = openXrSettingsType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m => m.Name == "GetSettingsForBuildTargetGroup" && m.GetParameters().Length == 1);
        if (getSettings == null)
            return;

        var settingsObj = getSettings.Invoke(null, new object[] { group });
        if (settingsObj == null)
            return;

        // Iterate settings.features and enable the feature whose type name contains "HandInteractionProfile"
        var featuresProp = openXrSettingsType.GetProperty("features", BindingFlags.Public | BindingFlags.Instance);
        if (featuresProp == null)
            return;

        var features = featuresProp.GetValue(settingsObj) as System.Collections.IEnumerable;
        if (features == null)
            return;

        foreach (var feature in features)
        {
            if (feature == null) continue;
            var t = feature.GetType();
            if (t.FullName == null) continue;

            if (t.FullName.IndexOf("HandInteractionProfile", StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            var enabledProp = t.GetProperty("enabled", BindingFlags.Public | BindingFlags.Instance);
            if (enabledProp != null && enabledProp.PropertyType == typeof(bool) && !(bool)enabledProp.GetValue(feature))
            {
                enabledProp.SetValue(feature, true);
                EditorUtility.SetDirty((UnityEngine.Object)settingsObj);
                AssetDatabase.SaveAssets();
            }
        }
    }
}

