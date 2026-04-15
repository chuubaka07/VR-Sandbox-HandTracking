using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class GestureTeleportWithInterface : MonoBehaviour
{
    public MonoBehaviour handInputObject;
    private IHandInput handInput;

    public XRRayInteractor rayInteractor;
    public TeleportationProvider teleportProvider;

    private bool wasPinching;

    void Start()
    {
        handInput = (IHandInput)handInputObject;
    }

    void Update()
    {
        bool isPointing = handInput.IsPointing;
        bool isPinching = handInput.IsPinching;

        rayInteractor.gameObject.SetActive(isPointing);

        if (wasPinching && !isPinching && isPointing)
        {
            TryTeleport();
        }

        wasPinching = isPinching;
    }

    void TryTeleport()
    {
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            TeleportRequest request = new TeleportRequest
            {
                destinationPosition = hit.point
            };

            teleportProvider.QueueTeleportRequest(request);
        }
    }
}
