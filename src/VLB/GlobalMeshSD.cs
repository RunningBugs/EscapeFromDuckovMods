using UnityEngine;

namespace VLB;

public static class GlobalMeshSD
{
	private static Mesh ms_Mesh;

	private static bool ms_DoubleSided;

	public static Mesh Get()
	{
		bool sD_requiresDoubleSidedMesh = Config.Instance.SD_requiresDoubleSidedMesh;
		if (ms_Mesh == null || ms_DoubleSided != sD_requiresDoubleSidedMesh)
		{
			Destroy();
			ms_Mesh = MeshGenerator.GenerateConeZ_Radii(1f, 1f, 1f, Config.Instance.sharedMeshSides, Config.Instance.sharedMeshSegments, cap: true, sD_requiresDoubleSidedMesh);
			ms_Mesh.hideFlags = Consts.Internal.ProceduralObjectsHideFlags;
			ms_DoubleSided = sD_requiresDoubleSidedMesh;
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
