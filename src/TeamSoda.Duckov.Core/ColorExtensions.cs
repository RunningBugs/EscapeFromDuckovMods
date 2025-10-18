using UnityEngine;

public static class ColorExtensions
{
	public static string ToHexString(this Color color)
	{
		return ((byte)(color.r * 255f)).ToString("X2") + ((byte)(color.g * 255f)).ToString("X2") + ((byte)(color.b * 255f)).ToString("X2") + ((byte)(color.a * 255f)).ToString("X2");
	}
}
