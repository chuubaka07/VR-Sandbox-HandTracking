using UnityEngine;

public class MouseHandInput : MonoBehaviour, IHandInput
{
    public bool IsPinching => Input.GetKey(KeyCode.F1);
    public bool IsPointing => Input.GetKey(KeyCode.F2);
    public bool IsOpenPalm => Input.GetKey(KeyCode.F3);
    public bool IsMenuGesture => Input.GetKeyDown(KeyCode.F4);
    public bool ConfirmTeleport => IsPointing && IsPinching;

    public Vector3 IndexTipPosition =>
        Camera.main.transform.position + Camera.main.transform.forward * 0.5f;

    public Vector3 PalmPosition => Camera.main.transform.position - Vector3.up * 0.2f;
}
