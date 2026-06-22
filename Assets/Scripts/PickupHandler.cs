using UnityEngine;
using System.Collections.Generic;

public class PickupHandler : MonoBehaviour
{
    public Transform rightHand;
    public Transform leftHand;

    private Dictionary<string, Vector3> originalScales = new();

    public void PickUpRight(string objectName) => Pickup(objectName, rightHand);
    public void PickUpLeft(string objectName)  => Pickup(objectName, leftHand);
    public void PutDown(string objectName)     => Drop(objectName);

    void Pickup(string objectName, Transform hand)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj == null) return;

        originalScales[objectName] = obj.transform.localScale;

        obj.transform.SetParent(hand, true); // worldPositionStays = true
        obj.transform.localScale = originalScales[objectName];
    }

    void Drop(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj == null) return;

        Vector3 currentWorldPos = obj.transform.position;
        Quaternion currentWorldRot = obj.transform.rotation;
        Vector3 scale = originalScales.ContainsKey(objectName) 
            ? originalScales[objectName] 
            : obj.transform.localScale;

        obj.transform.SetParent(null);
        obj.transform.position   = currentWorldPos;
        obj.transform.rotation   = currentWorldRot;
        obj.transform.localScale = scale;

        originalScales.Remove(objectName);
    }
}
