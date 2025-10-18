using System.Collections.Generic;
using UnityEngine;

public class SetActiveByEnding : MonoBehaviour
{
	public GameObject target;

	public List<int> endingIndexs;

	private void Start()
	{
		target.SetActive(endingIndexs.Contains(Ending.endingIndex));
	}
}
