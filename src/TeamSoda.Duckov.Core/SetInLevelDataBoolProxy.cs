using Duckov.Scenes;
using UnityEngine;

public class SetInLevelDataBoolProxy : MonoBehaviour
{
	public bool targetValue = true;

	public string keyString = "";

	private int keyHash;

	private bool keyInited;

	public void SetToTarget()
	{
		SetTo(targetValue);
	}

	public void SetTo(bool target)
	{
		if (!(keyString == ""))
		{
			if (!keyInited)
			{
				InitKey();
			}
			if ((bool)MultiSceneCore.Instance)
			{
				MultiSceneCore.Instance.inLevelData[keyHash] = target;
			}
		}
	}

	private void InitKey()
	{
		keyHash = keyString.GetHashCode();
		keyInited = true;
	}
}
