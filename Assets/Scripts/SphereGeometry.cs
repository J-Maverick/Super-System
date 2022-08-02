using System.Collections;
using System.Collections.Generic;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine;

public class SphereGeometry : UdonSharpBehaviour
{
    private Mesh mesh;
    private Vector3[] vertices;
    private Color[] colors;
    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.HasComponent<MeshFilter>())
        {
            mesh = GetComponent<MeshFilter>().mesh;

            vertices = mesh.vertices;
            int nVert = vertices.Length;
            colors = new Color[nVert];

            for (int i = 0; i < nVert; i++)
            {
                //vertices[i] *= Random.Range(0.9f, 1.1f);
                // colors[i] = Random.ColorHSV();
                colors[i] = new Color(vertices[i].x, vertices[i].y, vertices[i].z, 1);
            }
            mesh.vertices = vertices;
            mesh.colors = colors;
            // mesh.RecalculateBounds();
            // mesh.RecalculateTangents();
            // mesh.RecalculateNormals();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
