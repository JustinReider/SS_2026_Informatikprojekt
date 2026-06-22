using UnityEngine;
using System.Collections.Generic;

public class BirdSplineFlight : MonoBehaviour
{
    [Header("Path")]
    public Transform pathParent;

    [Header("Movement")]
    public float speed = 5f;
    public float rotationSpeed = 6f;
    public float heightWobble = 0.2f;

    [Header("Curve Feel")]
    [Range(0.01f, 0.3f)]
    public float lookAhead = 0.1f;

    [Header("Start Behavior")]
    public float enterSpeed = 3f;

    private List<Transform> points = new List<Transform>();

    private float t = 0f;
    private int index = 0;

    private bool isEntering = true;
    private Vector3 enterTarget;

    void Start()
    {
        foreach (Transform child in pathParent)
        {
            points.Add(child);
        }

        if (points.Count < 4)
        {
            Debug.LogError("Need at least 4 points for smooth spline loop!");
        }

        enterTarget = points[0].position;
    }

    void Update()
    {
        if (points.Count < 4) return;

        // ----------------------------
        // 1. ENTER ROUTE (kein Snap)
        // ----------------------------
        if (isEntering)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                enterTarget,
                enterSpeed * Time.deltaTime
            );

            Vector3 dir = (enterTarget - transform.position).normalized;

            if (dir != Vector3.zero)
            {
                Quaternion targetRot =
                    Quaternion.LookRotation(dir) *
                    Quaternion.Euler(0, 180, 0);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    rotationSpeed * Time.deltaTime
                );
            }

            if (Vector3.Distance(transform.position, enterTarget) < 0.1f)
                isEntering = false;

            return;
        }

        // ----------------------------
        // 2. SPLINE MOVEMENT
        // ----------------------------

        float segmentLength =
            Vector3.Distance(
                points[LoopIndex(index)].position,
                points[LoopIndex(index + 1)].position
            );

        t += (speed / segmentLength) * Time.deltaTime;

        if (t > 1f)
        {
            t -= 1f;
            index = (index + 1) % points.Count;
        }

        Vector3 pos = GetSplinePosition(index, t);
        Vector3 nextPos = GetSplinePosition(index, t + lookAhead);

        // Wobble
        Vector3 upOffset =
            Vector3.up * Mathf.Sin(Time.time * 2f) * heightWobble;

        transform.position = pos + upOffset;

        // Rotation (frühere Kurvenreaktion durch lookAhead)
        Vector3 dirForward = (nextPos - pos).normalized;

        if (dirForward != Vector3.zero)
        {
            Quaternion targetRot =
                Quaternion.LookRotation(dirForward) *
                Quaternion.Euler(0, 180, 0);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    // ----------------------------
    // SPLINE
    // ----------------------------

    Vector3 GetSplinePosition(int i, float t)
    {
        Vector3 p0 = points[LoopIndex(i - 1)].position;
        Vector3 p1 = points[LoopIndex(i)].position;
        Vector3 p2 = points[LoopIndex(i + 1)].position;
        Vector3 p3 = points[LoopIndex(i + 2)].position;

        return CatmullRom(p0, p1, p2, p3, t);
    }

    int LoopIndex(int i)
    {
        int count = points.Count;
        return (i + count) % count;
    }

    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }
}
