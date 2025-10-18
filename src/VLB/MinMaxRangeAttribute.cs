using System;

namespace VLB;

public class MinMaxRangeAttribute : Attribute
{
	public float minValue { get; private set; }

	public float maxValue { get; private set; }

	public MinMaxRangeAttribute(float min, float max)
	{
		minValue = min;
		maxValue = max;
	}
}
