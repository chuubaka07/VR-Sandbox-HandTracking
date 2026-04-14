using UnityEngine;

public interface IHandInput
{
    bool IsPinching { get; }      // F1
    bool IsPointing { get; }      // F2
    bool IsOpenPalm { get; }      // F3
    bool IsMenuGesture { get; }   // F4 (однократно)
    bool ConfirmTeleport { get; } // IsPointing && IsPinching
    Vector3 IndexTipPosition { get; }
    Vector3 PalmPosition { get; }
}
