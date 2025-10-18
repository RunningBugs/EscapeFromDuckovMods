using UnityEngine;

namespace VLB;

public static class GlobalMeshHD
{
	private static Mesh ms_Mesh;

	public static Mesh Get()
	{
		if (ms_Mesh == null)
		{
			Destroy();
			ms_Mesh = MeshGenerator.GenerateConeZ_Radii_DoubleCaps(1f, 1f, 1f, Config.Instance.sharedMeshSides, inverted: true);
			ms_Mesh.hideFlags = Consts.Internal.ProceduralObjectsHideFlags;
		}
		return ms_Mesh;
	}

	public static void Destroy()
	{
		if (ms_Mesh != null)
		{
			Object.DestroyImmediate(ms_Mesh);
			ms_Mesh = null;
		}
	}
}
