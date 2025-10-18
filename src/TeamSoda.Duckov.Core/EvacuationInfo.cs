using System;
using UnityEngine;

[Serializable]
public struct EvacuationInfo
{
	public string subsceneID;

	public Vector3 position;

	public EvacuationInfo(string subsceneID, Vector3 position)
	{
		this.subsceneID = subsceneID;
		this.position = position;
	}
}
