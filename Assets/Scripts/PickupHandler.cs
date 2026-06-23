using UnityEngine;
using System.Collections.Generic;

public class PickupHandler : MonoBehaviour
{
    public Transform rightHand;
    public Transform leftHand;
    private Dictionary<string, Vector3> originalWorldScales = new();

    public void PickUpRight(string objectName) => Pickup(objectName, rightHand);
    public void PickUpLeft(string objectName)  => Pickup(objectName, leftHand);
    public void PutDown(string objectName)     => Drop(objectName);

    void Pickup(string objectName, Transform hand)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj == null) return;

        // World-Scale VOR dem Parenting speichern
        originalWorldScales[objectName] = obj.transform.lossyScale;

        obj.transform.SetParent(hand, true);

        // World-Scale nach dem Parenting wiederherstellen
        ApplyWorldScale(obj.transform, originalWorldScales[objectName]);
    }

    void Drop(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj == null) return;

        Vector3 worldPos = obj.transform.position;
        Quaternion worldRot = obj.transform.rotation;
        Vector3 worldScale = originalWorldScales.ContainsKey(objectName)
            ? originalWorldScales[objectName]
            : obj.transform.lossyScale;

        obj.transform.SetParent(null);
        obj.transform.position = worldPos;
        obj.transform.rotation = worldRot;
        ApplyWorldScale(obj.transform, worldScale);

        originalWorldScales.Remove(objectName);
    }

    void ApplyWorldScale(Transform t, Vector3 targetWorldScale)
    {
        // localScale berechnen der die gewünschte World-Scale ergibt
        if (t.parent == null)
        {
            t.localScale = targetWorldScale;
            return;
        }

        Vector3 parentScale = t.parent.lossyScale;
        t.localScale = new Vector3(
            parentScale.x != 0 ? targetWorldScale.x / parentScale.x : targetWorldScale.x,
            parentScale.y != 0 ? targetWorldScale.y / parentScale.y : targetWorldScale.y,
            parentScale.z != 0 ? targetWorldScale.z / parentScale.z : targetWorldScale.z
        );
    }
}
