using Duckov.Scenes;
using UnityEngine;

public class SetActiveByChance : MonoBehaviour
{
	public bool saveInLevel;

	private int keyCached;

	[Range(0f, 1f)]
	public float activeChange = 0.5f;

	private void Awake()
	{
		bool flag = Random.Range(0f, 1f) < activeChange;
		if (saveInLevel && (bool)MultiSceneCore.Instance)
		{
			if (MultiSceneCore.Instance.inLevelData.TryGetValue(keyCached, out var value) && value is bool flag2)
			{
				Debug.Log($"存在门存档信息：{flag2}");
				flag = flag2;
			}
			MultiSceneCore.Instance.inLevelData[keyCached] = flag;
		}
		base.gameObject.SetActive(flag);
	}

	private int GetKey()
	{
		Vector3 vector = base.transform.position * 10f;
		int x = Mathf.RoundToInt(vector.x);
		int y = Mathf.RoundToInt(vector.y);
		int z = Mathf.RoundToInt(vector.z);
		Vector3Int vector3Int = new Vector3Int(x, y, z);
		return $"Door_{vector3Int}".GetHashCode();
	}
}
