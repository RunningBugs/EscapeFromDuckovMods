using System;
using UnityEngine;

namespace VLB;

[Serializable]
public struct MinMaxRangeFloat : IEquatable<MinMaxRangeFloat>
{
	[SerializeField]
	private float m_MinValue;

	[SerializeField]
	private float m_MaxValue;

	public float minValue => m_MinValue;

	public float maxValue => m_MaxValue;

	public float randomValue => UnityEngine.Random.Range(minValue, maxValue);

	public Vector2 asVector2 => new Vector2(minValue, maxValue);

	public float GetLerpedValue(float lerp01)
	{
		return Mathf.Lerp(minValue, maxValue, lerp01);
	}

	public MinMaxRangeFloat(float min, float max)
	{
		m_MinValue = min;
		m_MaxValue = max;
	}

	public override bool Equals(object obj)
	{
		if (obj is MinMaxRangeFloat other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(MinMaxRangeFloat other)
	{
		if (m_MinValue == other.m_MinValue)
		{
			return m_MaxValue == other.m_MaxValue;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (m_MinValue, m_MaxValue).GetHashCode();
	}

	public static bool operator ==(MinMaxRangeFloat lhs, MinMaxRangeFloat rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(MinMaxRangeFloat lhs, MinMaxRangeFloat rhs)
	{
		return !(lhs == rhs);
	}
}
