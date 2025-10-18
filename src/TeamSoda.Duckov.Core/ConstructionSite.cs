using Duckov.Economy;
using Duckov.Scenes;
using Saves;
using UnityEngine;
using UnityEngine.Events;

public class ConstructionSite : MonoBehaviour
{
	[SerializeField]
	private string _key;

	[SerializeField]
	private bool dontSave;

	private bool saveInMultiSceneCore;

	[SerializeField]
	private Cost cost;

	[SerializeField]
	private CostTaker costTaker;

	[SerializeField]
	private GameObject[] notBuiltGameObjects;

	[SerializeField]
	private GameObject[] builtGameObjects;

	[SerializeField]
	private GameObject[] setActiveOnBuilt;

	[SerializeField]
	private UnityEvent<ConstructionSite> onBuilt;

	[SerializeField]
	private UnityEvent<ConstructionSite> onActivate;

	[SerializeField]
	private UnityEvent<ConstructionSite> onDeactivate;

	private bool wasBuilt;

	private Color KeyFieldColor
	{
		get
		{
			if (string.IsNullOrWhiteSpace(_key))
			{
				return Color.red;
			}
			return Color.white;
		}
	}

	private string SaveKey => "ConstructionSite_" + _key;

	private void Awake()
	{
		costTaker.onPayed += OnBuilt;
		Load();
		SavesSystem.OnCollectSaveData += Save;
		costTaker.SetCost(cost);
		RefreshGameObjects();
	}

	private void OnDestroy()
	{
		SavesSystem.OnCollectSaveData -= Save;
	}

	private void Save()
	{
		if (dontSave)
		{
			int inLevelDataKey = GetInLevelDataKey();
			if (MultiSceneCore.Instance.inLevelData.ContainsKey(inLevelDataKey))
			{
				MultiSceneCore.Instance.inLevelData[inLevelDataKey] = wasBuilt;
			}
			else
			{
				MultiSceneCore.Instance.inLevelData.Add(inLevelDataKey, wasBuilt);
			}
		}
		else if (string.IsNullOrWhiteSpace(_key))
		{
			Debug.LogError($"Construction Site {base.gameObject} 没有配置保存用的key");
		}
		else
		{
			SavesSystem.Save(SaveKey, wasBuilt);
		}
	}

	private int GetInLevelDataKey()
	{
		Vector3 vector = base.transform.position * 10f;
		int x = Mathf.RoundToInt(vector.x);
		int y = Mathf.RoundToInt(vector.y);
		int z = Mathf.RoundToInt(vector.z);
		return ("ConstSite" + new Vector3Int(x, y, z).ToString()).GetHashCode();
	}

	private void Load()
	{
		if (!dontSave)
		{
			if (string.IsNullOrWhiteSpace(_key))
			{
				Debug.LogError($"Construction Site {base.gameObject} 没有配置保存用的key");
			}
			wasBuilt = SavesSystem.Load<bool>(SaveKey);
		}
		else
		{
			int inLevelDataKey = GetInLevelDataKey();
			MultiSceneCore.Instance.inLevelData.TryGetValue(inLevelDataKey, out var value);
			if (value != null)
			{
				wasBuilt = (bool)value;
			}
		}
		if (wasBuilt)
		{
			OnActivate();
		}
		else
		{
			OnDeactivate();
		}
	}

	private void Start()
	{
	}

	private void OnBuilt(CostTaker taker)
	{
		wasBuilt = true;
		onBuilt?.Invoke(this);
		RefreshGameObjects();
		GameObject[] array = setActiveOnBuilt;
		foreach (GameObject gameObject in array)
		{
			if ((bool)gameObject)
			{
				gameObject.SetActive(value: true);
			}
		}
		Save();
	}

	private void OnActivate()
	{
		onActivate?.Invoke(this);
		RefreshGameObjects();
	}

	private void OnDeactivate()
	{
		onDeactivate?.Invoke(this);
		RefreshGameObjects();
	}

	public void RefreshGameObjects()
	{
		costTaker.gameObject.SetActive(!wasBuilt);
		GameObject[] array = notBuiltGameObjects;
		foreach (GameObject gameObject in array)
		{
			if ((bool)gameObject)
			{
				gameObject.SetActive(!wasBuilt);
			}
		}
		array = builtGameObjects;
		foreach (GameObject gameObject2 in array)
		{
			if ((bool)gameObject2)
			{
				gameObject2.SetActive(wasBuilt);
			}
		}
	}
}
