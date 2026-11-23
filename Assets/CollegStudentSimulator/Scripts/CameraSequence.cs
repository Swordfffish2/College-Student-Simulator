using System.Collections;
using UnityEngine;

public class CameraSequence : MonoBehaviour
{
    [Header("Camera anchors")]
    [SerializeField] private Transform bedLying;
    [SerializeField] private Transform bedSitting;
    [SerializeField] private Transform aisleStanding;
    [SerializeField] private Transform deskSitting;
    [SerializeField] private Transform monitorCloseup;

    public bool IsAtDesk { get; private set; }

    private void Awake()
    {
        if (!bedLying || !bedSitting || !aisleStanding || !deskSitting || !monitorCloseup)
        {
            Debug.LogError("CameraSequence: Assign all five camera anchors.", this);
            enabled = false;
            return;
        }

        transform.SetPositionAndRotation(bedLying.position, bedLying.rotation);
    }

    private IEnumerator Start()
    {
        yield return WakeFromBed();
    }

    public IEnumerator WakeFromBed()
    {
        IsAtDesk = false;
        transform.SetPositionAndRotation(bedLying.position, bedLying.rotation);
        yield return new WaitForSeconds(0.8f);
        yield return MoveTo(bedSitting, 1.4f);
        yield return new WaitForSeconds(0.35f);
        yield return MoveTo(aisleStanding, 1.1f);
        yield return MoveTo(deskSitting, 2.2f);
        IsAtDesk = true;
    }

    public IEnumerator EnterMonitor()
    {
        IsAtDesk = false;
        yield return MoveTo(monitorCloseup, 2.2f);
    }

    private IEnumerator MoveTo(Transform target, float duration)
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float blend = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.SetPositionAndRotation(
                Vector3.Lerp(startPosition, target.position, blend),
                Quaternion.Slerp(startRotation, target.rotation, blend));
            yield return null;
        }

        transform.SetPositionAndRotation(target.position, target.rotation);
    }
}
