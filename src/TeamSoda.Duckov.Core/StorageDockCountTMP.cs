using ItemStatsSystem;
using TMPro;
using UnityEngine;

public class StorageDockCountTMP : MonoBehaviour
{
	[SerializeField]
	private TextMeshPro tmp;

	[SerializeField]
	private bool setActiveFalseWhenCountIsZero;

	private void Awake()
	{
		PlayerStorage.OnItemAddedToBuffer += OnItemAddedToBuffer;
		PlayerStorage.OnTakeBufferItem += OnTakeBufferItem;
		PlayerStorage.OnLoadingFinished += OnLoadingFinished;
	}

	private void OnDestroy()
	{
		PlayerStorage.OnItemAddedToBuffer -= OnItemAddedToBuffer;
		PlayerStorage.OnTakeBufferItem -= OnTakeBufferItem;
		PlayerStorage.OnLoadingFinished -= OnLoadingFinished;
	}

	private void OnLoadingFinished()
	{
		Refresh();
	}

	private void Start()
	{
		Refresh();
	}

	private void OnTakeBufferItem()
	{
		Refresh();
	}

	private void OnItemAddedToBuffer(Item item)
	{
		Refresh();
	}

	private void Refresh()
	{
		int count = PlayerStorage.IncomingItemBuffer.Count;
		tmp.text = $"{count}";
		if (setActiveFalseWhenCountIsZero)
		{
			base.gameObject.SetActive(count > 0);
		}
	}
}
