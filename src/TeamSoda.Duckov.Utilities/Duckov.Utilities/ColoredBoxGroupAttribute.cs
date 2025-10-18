using System;
using Sirenix.OdinInspector;

namespace Duckov.Utilities;

public class ColoredBoxGroupAttribute : PropertyGroupAttribute
{
	public float R;

	public float G;

	public float B;

	public float A;

	public ColoredBoxGroupAttribute(string path)
		: base(path)
	{
	}

	public ColoredBoxGroupAttribute(string path, float r, float g, float b, float a = 1f)
		: base(path)
	{
		R = r;
		G = g;
		B = b;
		A = a;
	}

	protected override void CombineValuesWith(PropertyGroupAttribute other)
	{
		ColoredBoxGroupAttribute coloredBoxGroupAttribute = (ColoredBoxGroupAttribute)other;
		R = Math.Max(coloredBoxGroupAttribute.R, R);
		G = Math.Max(coloredBoxGroupAttribute.G, G);
		B = Math.Max(coloredBoxGroupAttribute.B, B);
		A = Math.Max(coloredBoxGroupAttribute.A, A);
	}
}
