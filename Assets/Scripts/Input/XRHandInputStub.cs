using UnityEngine;

public class XRHandInputStub : MonoBehaviour, IHandInput
{
    // Всегда false, позже заменится на реальный трекинг
    public bool IsPinching => false;
    public bool IsPointing => false;
    public bool IsOpenPalm => false;
    public bool IsMenuGesture => false;
    public bool ConfirmTeleport => false;
    public Vector3 IndexTipPosition => Vector3.zero;
    public Vector3 PalmPosition => Vector3.zero;
}
