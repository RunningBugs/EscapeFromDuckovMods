using UnityEngine;

namespace VLB;

public static class TransformUtils
{
	public struct Packed
	{
		public Vector3 position;

		public Quaternion rotation;

		public Vector3 lossyScale;

		public bool IsSame(Transform transf)
		{
			if (transf.position == position && transf.rotation == rotation)
			{
				return transf.lossyScale == lossyScale;
			}
			return false;
		}
	}

	public static Packed GetWorldPacked(this Transform self)
	{
		return new Packed
		{
			position = self.position,
			rotation = self.rotation,
			lossyScale = self.lossyScale
		};
	}
}
