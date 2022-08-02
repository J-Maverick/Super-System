using System.Collections;
using System.Collections.Generic;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine;

public class CameraController : UdonSharpBehaviour
{
    private Vector3 startPosition;
    private float startScale;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.localPosition;
        startScale = transform.parent.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = startPosition * startScale / transform.parent.localScale.x;
    }
}
