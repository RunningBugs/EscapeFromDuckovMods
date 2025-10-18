using System.Collections.Generic;
using UnityEngine;

public class UVCylinder : MonoBehaviour
{
	public float radius = 1f;

	public float height = 2f;

	public int subdivision = 16;

	private Mesh mesh;

	private void Generate()
	{
		if (mesh == null)
		{
			mesh = new Mesh();
		}
		mesh.Clear();
		new List<Vector3>();
		new List<Vector2>();
		new List<Vector3>();
		new List<int>();
		for (int i = 0; i < subdivision; i++)
		{
		}
	}
}
