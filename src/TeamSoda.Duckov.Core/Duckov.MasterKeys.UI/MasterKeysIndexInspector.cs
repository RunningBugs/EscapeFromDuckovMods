using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.MasterKeys.UI;

public class MasterKeysIndexInspector : MonoBehaviour
{
	[SerializeField]
	private int targetItemID;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private GameObject content;

	[SerializeField]
	private GameObject placeHolder;

	internal void Setup(MasterKeysIndexEntry target)
	{
		if (target == null)
		{
			SetupEmpty();
		}
		else
		{
			SetupNormal(target);
		}
	}

	private void SetupNormal(MasterKeysIndexEntry target)
	{
		targetItemID = target.ItemID;
		placeHolder.SetActive(value: false);
		content.SetActive(value: true);
		nameText.text = target.DisplayName;
		descriptionText.text = target.Description;
		icon.sprite = target.Icon;
	}

	private void SetupEmpty()
	{
		content.gameObject.SetActive(value: false);
		placeHolder.SetActive(value: true);
	}
}
