using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMesh : MonoBehaviour
{

	public float meshWidth = 1f;
    public float meshHeight = 10f;
    public int phaseCount = 20;
    public float endWidth = 0.1f;

    void Awake()
    {
        float phaseHeight = meshHeight / (phaseCount - 1);
        float decreaseWidth = (meshWidth - endWidth) / (phaseCount - 1);

        // 顶点
        Vector3[] vertices = new Vector3[phaseCount * 6];
        float bottomY = -meshHeight * 0.5f;
        for(int i = 0; i < phaseCount; i++)
        {
        	float curWidth = meshWidth - decreaseWidth * i;
            vertices[i * 6 + 0] = new Vector3 (bottomY + i * phaseHeight, (curWidth / 2), 0);
            vertices[i * 6 + 1] = new Vector3 (bottomY + i * phaseHeight, (curWidth / 4), -Mathf.Sqrt(3) * curWidth / 4);
            vertices[i * 6 + 2] = new Vector3 (bottomY + i * phaseHeight, -(curWidth / 4), -Mathf.Sqrt(3) * curWidth / 4);
            vertices[i * 6 + 3] = new Vector3 (bottomY + i * phaseHeight, -(curWidth / 2), 0);
            vertices[i * 6 + 4] = new Vector3 (bottomY + i * phaseHeight, -(curWidth / 4), Mathf.Sqrt(3) * curWidth / 4);
            vertices[i * 6 + 5] = new Vector3 (bottomY + i * phaseHeight, (curWidth / 4), Mathf.Sqrt(3) * curWidth / 4);
        }

        //法线
        Vector3[] normals = new Vector3[phaseCount * 6];
        for(int i = 0; i < phaseCount; i++)
        {
            float curWidth = meshWidth - decreaseWidth * i;
            normals[i * 6 + 0] = new Vector3 (0, (curWidth / 2), 0).normalized;
            normals[i * 6 + 1] = new Vector3 (0, (curWidth / 4), -Mathf.Sqrt(3) * curWidth / 4).normalized;
            normals[i * 6 + 2] = new Vector3 (0, -(curWidth / 4), -Mathf.Sqrt(3) * curWidth / 4).normalized;
            normals[i * 6 + 3] = new Vector3 (0, -(curWidth / 2), 0).normalized;
            normals[i * 6 + 4] = new Vector3 (0, -(curWidth / 4), Mathf.Sqrt(3) * curWidth / 4).normalized;
            normals[i * 6 + 5] = new Vector3 (0, (curWidth / 4), Mathf.Sqrt(3) * curWidth / 4).normalized;
        }

        //三角形
        int[] indices = new int[(phaseCount - 1) * 6 * 6];
        for( int i = 1; i < phaseCount; i++)
        {
            for(int j = 0; j < 6; j++)
            {
                int nextIndex = (j + 1) % 6;
                indices[( i - 1) * 6 * 6 + j * 6 + 0] = (i - 1) * 6 + j;
                indices[( i - 1) * 6 * 6 + j * 6 + 1] = i * 6 + nextIndex;
                indices[( i - 1) * 6 * 6 + j * 6 + 2] = (i - 1) * 6 + nextIndex;

                indices[( i - 1) * 6 * 6 + j * 6 + 3] = (i - 1) * 6 + j;
                indices[( i - 1) * 6 * 6 + j * 6 + 4] = i * 6 + j;
                indices[( i - 1) * 6 * 6 + j * 6 + 5] = i * 6 + nextIndex;
            }
        }


        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = indices;
    }
}
