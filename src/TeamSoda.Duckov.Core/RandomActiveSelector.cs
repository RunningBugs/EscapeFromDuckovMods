using System.Collections.Generic;
using Duckov.Scenes;
using UnityEngine;

public class RandomActiveSelector : MonoBehaviour
{
	[Range(0f, 1f)]
	public float activeChance = 1f;

	private int activeIndex;

	private int guid;

	private bool setted;

	public List<GameObject> selections;

	private void Awake()
	{
		foreach (GameObject selection in selections)
		{
			if (!(selection == null))
			{
				selection.SetActive(value: false);
			}
		}
	}

	private void Update()
	{
		if (!setted && LevelManager.LevelInited)
		{
			Set();
		}
	}

	private void Set()
	{
		if (MultiSceneCore.Instance == null)
		{
			return;
		}
		if (MultiSceneCore.Instance.inLevelData.TryGetValue(guid, out var value))
		{
			activeIndex = (int)value;
		}
		else
		{
			if (Random.Range(0f, 1f) > activeChance)
			{
				activeIndex = -1;
			}
			else
			{
				activeIndex = Random.Range(0, selections.Count);
			}
			MultiSceneCore.Instance.inLevelData.Add(guid, activeIndex);
		}
		if (activeIndex >= 0)
		{
			GameObject gameObject = selections[activeIndex];
			if ((bool)gameObject)
			{
				gameObject.SetActive(value: true);
			}
		}
		setted = true;
		base.enabled = false;
	}
}
