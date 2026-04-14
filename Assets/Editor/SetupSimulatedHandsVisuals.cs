using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupSimulatedHandsVisuals
{
    [MenuItem("Tools/VR Sandbox/Setup Simulated Hand Visuals")]
    public static void Setup()
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("Open MainScene before setup.");
            return;
        }

        var xrOrigin = GameObject.Find("XR Origin");
        if (xrOrigin == null)
        {
            Debug.LogError("XR Origin not found. Run 'Add XR Hands Simulator To Scene' first.");
            return;
        }

        var leftAnchor = FindAnchor(xrOrigin.transform, "Left");
        var rightAnchor = FindAnchor(xrOrigin.transform, "Right");
        if (leftAnchor == null || rightAnchor == null)
        {
            Debug.LogError("Left/Right hand tracking anchors not found under XR Origin.");
            return;
        }

        RemoveControllerVisuals(leftAnchor);
        RemoveControllerVisuals(rightAnchor);
        CreateOrRefreshHand(leftAnchor, true);
        CreateOrRefreshHand(rightAnchor, false);

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("Simulated hand visuals are ready. Press Play and use F1 for pinch.");
    }

    private static Transform FindAnchor(Transform root, string side)
    {
        var sideLower = side.ToLowerInvariant();
        return root.GetComponentsInChildren<Transform>(true)
            .FirstOrDefault(t =>
            {
                var n = t.name.ToLowerInvariant();
                return n.Contains(sideLower) &&
                       (n.Contains("hand tracking") || n.Contains("hand") || n.Contains("controller"));
            });
    }

    private static void RemoveControllerVisuals(Transform anchor)
    {
        var toDelete = anchor.GetComponentsInChildren<Transform>(true)
            .Where(t =>
            {
                var n = t.name.ToLowerInvariant();
                return n.Contains("controller model") || n.Contains("model") || n.Contains("visual");
            })
            .Where(t => t != anchor)
            .ToList();

        foreach (var t in toDelete)
            Object.DestroyImmediate(t.gameObject);
    }

    private static void CreateOrRefreshHand(Transform anchor, bool isLeft)
    {
        var existing = anchor.Find("SimulatedHandVisual");
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        var handRoot = new GameObject("SimulatedHandVisual");
        handRoot.transform.SetParent(anchor, false);
        handRoot.transform.localPosition = Vector3.zero;
        handRoot.transform.localRotation = Quaternion.identity;
        handRoot.transform.localScale = Vector3.one;

        // Palm
        var palm = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        palm.name = "Palm";
        palm.transform.SetParent(handRoot.transform, false);
        palm.transform.localScale = new Vector3(0.08f, 0.03f, 0.08f);
        palm.transform.localPosition = new Vector3(0f, 0f, 0f);
        DisableCollider(palm);

        // Thumb tip
        var thumb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        thumb.name = "ThumbTip";
        thumb.transform.SetParent(handRoot.transform, false);
        thumb.transform.localScale = Vector3.one * 0.018f;
        thumb.transform.localPosition = isLeft ? new Vector3(-0.03f, 0.01f, 0.04f) : new Vector3(0.03f, 0.01f, 0.04f);
        DisableCollider(thumb);

        // Index tip
        var index = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        index.name = "IndexTip";
        index.transform.SetParent(handRoot.transform, false);
        index.transform.localScale = Vector3.one * 0.02f;
        index.transform.localPosition = new Vector3(0f, 0.015f, 0.07f);
        DisableCollider(index);

        // Color coding by hand
        var palmRenderer = palm.GetComponent<Renderer>();
        if (palmRenderer != null)
            palmRenderer.sharedMaterial.color = isLeft ? new Color(0.4f, 0.8f, 1f) : new Color(1f, 0.7f, 0.4f);

        var visual = handRoot.AddComponent<SimulatedHandVisual>();
        visual.isLeftHand = isLeft;
        visual.palm = palm.transform;
        visual.thumbTip = thumb.transform;
        visual.indexTip = index.transform;
    }

    private static void DisableCollider(GameObject go)
    {
        var collider = go.GetComponent<Collider>();
        if (collider != null)
            Object.DestroyImmediate(collider);
    }
}

