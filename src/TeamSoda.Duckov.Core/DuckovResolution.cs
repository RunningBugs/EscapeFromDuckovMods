using System;
using UnityEngine;

[Serializable]
public struct DuckovResolution
{
	public int width;

	public int height;

	public override bool Equals(object obj)
	{
		if (obj is DuckovResolution duckovResolution && duckovResolution.height == height && duckovResolution.width == width)
		{
			return true;
		}
		return false;
	}

	public override string ToString()
	{
		return $"{width} x {height}";
	}

	public DuckovResolution(Resolution res)
	{
		height = res.height;
		width = res.width;
	}

	public DuckovResolution(int _width, int _height)
	{
		height = _height;
		width = _width;
	}

	public bool CheckRotioFit(DuckovResolution newRes, DuckovResolution defaultRes)
	{
		float num = (float)newRes.height / (float)newRes.width;
		if (Mathf.Abs((float)defaultRes.height - num * (float)defaultRes.width) <= 2f)
		{
			return true;
		}
		return false;
	}
}
