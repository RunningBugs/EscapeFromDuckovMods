using Duckov.Quests;
using Duckov.Scenes;

public class RequireInLevelDataBool : Condition
{
	public string keyString = "";

	private int keyHash = -1;

	private bool keyHashInited;

	private bool isEmptyString;

	public override bool Evaluate()
	{
		if (!MultiSceneCore.Instance)
		{
			return false;
		}
		if (!keyHashInited)
		{
			InitKeyHash();
		}
		if (isEmptyString)
		{
			return false;
		}
		if (MultiSceneCore.Instance.inLevelData.TryGetValue(keyHash, out var value) && value is bool)
		{
			return (bool)value;
		}
		return false;
	}

	private void InitKeyHash()
	{
		if (keyString == "")
		{
			isEmptyString = true;
		}
		keyHash = keyString.GetHashCode();
		keyHashInited = true;
	}
}
