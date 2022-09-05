
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ParentHandler : UdonSharpBehaviour
{
    public Transform[] children;

    void Start()
    {
        children = new Transform[transform.childCount];
        int i = 0;
        //Debug.LogFormat("Checking {0} children", transform.childCount);
        foreach (Transform child in transform)
        {
            children[i] = child;
            i++;
        }

    }

    void ResetParents()
    {
        foreach (Transform child in children)
        {
            child.SetParent(transform);
        }
    }

    public void SetParent(Transform targetTransform)
    {
        ResetParents();
        foreach (Transform child in children)
        {
            if (child != targetTransform) child.SetParent(targetTransform);
        }
    }
}
