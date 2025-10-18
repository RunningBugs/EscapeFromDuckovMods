using System.Collections.Generic;
using System.Linq;
using Duckov.PerkTrees;
using UnityEngine;

public class PerkTreeManager : MonoBehaviour
{
	private static PerkTreeManager instance;

	public List<PerkTree> perkTrees;

	public static PerkTreeManager Instance => instance;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Debug.LogError("检测到多个PerkTreeManager");
		}
	}

	public static PerkTree GetPerkTree(string id)
	{
		if (instance == null)
		{
			return null;
		}
		PerkTree perkTree = instance.perkTrees.FirstOrDefault((PerkTree e) => e != null && e.ID == id);
		if (perkTree == null)
		{
			Debug.LogError("未找到PerkTree id:" + id);
		}
		return perkTree;
	}
}
