using System.Linq;
using UnityEngine;

public class SimulatedHandVisual : MonoBehaviour
{
    public bool isLeftHand;
    public Transform palm;
    public Transform thumbTip;
    public Transform indexTip;

    private IHandInput handInput;
    private Vector3 thumbOpenLocalPos;
    private Vector3 indexOpenLocalPos;
    private bool initialized;

    private void Start()
    {
        CacheInput();
        CacheDefaultPose();
    }

    private void Update()
    {
        if (handInput == null)
            CacheInput();

        if (!initialized || handInput == null || thumbTip == null || indexTip == null)
            return;

        // F1 closes thumb/index to emulate pinch in editor.
        if (handInput.IsPinching)
        {
            var pinchTarget = (thumbOpenLocalPos + indexOpenLocalPos) * 0.5f;
            thumbTip.localPosition = Vector3.Lerp(thumbTip.localPosition, pinchTarget, Time.deltaTime * 16f);
            indexTip.localPosition = Vector3.Lerp(indexTip.localPosition, pinchTarget, Time.deltaTime * 16f);
        }
        else
        {
            thumbTip.localPosition = Vector3.Lerp(thumbTip.localPosition, thumbOpenLocalPos, Time.deltaTime * 12f);
            indexTip.localPosition = Vector3.Lerp(indexTip.localPosition, indexOpenLocalPos, Time.deltaTime * 12f);
        }
    }

    private void CacheInput()
    {
        var inputMono = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .FirstOrDefault(m => m is IHandInput);
        handInput = inputMono as IHandInput;
    }

    private void CacheDefaultPose()
    {
        if (thumbTip == null || indexTip == null)
            return;

        thumbOpenLocalPos = thumbTip.localPosition;
        indexOpenLocalPos = indexTip.localPosition;
        initialized = true;
    }
}

