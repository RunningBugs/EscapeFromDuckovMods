using Cysharp.Threading.Tasks;
using ItemStatsSystem.Data;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI;

public class StorageDockEntry : MonoBehaviour
{
	[SerializeField]
	private ItemMetaDisplay itemDisplay;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private GameObject countDisplay;

	[SerializeField]
	private TextMeshProUGUI countText;

	[SerializeField]
	private Image bgImage;

	[SerializeField]
	private Button button;

	[SerializeField]
	private GameObject loadingIndicator;

	[SerializeField]
	private Color colorNormal;

	[SerializeField]
	private Color colorFull;

	[SerializeField]
	[LocalizationKey("Default")]
	private string textKeyNormal;

	[SerializeField]
	[LocalizationKey("Default")]
	private string textKeyInventoryFull;

	private int index;

	private ItemTreeData item;

	private void Awake()
	{
		button.onClick.AddListener(OnButtonClick);
	}

	private void OnButtonClick()
	{
		if (PlayerStorage.IsAccessableAndNotFull())
		{
			TakeTask().Forget();
		}
	}

	private async UniTask TakeTask()
	{
		if (!PlayerStorage.TakingItem)
		{
			loadingIndicator.SetActive(value: true);
			text.gameObject.SetActive(value: false);
			await PlayerStorage.TakeBufferItem(index);
		}
	}

	public void Setup(int index, ItemTreeData item)
	{
		this.index = index;
		this.item = item;
		ItemTreeData.DataEntry rootData = item.RootData;
		itemDisplay.Setup(rootData.typeID);
		int stackCount = rootData.StackCount;
		if (stackCount > 1)
		{
			countText.text = stackCount.ToString();
			countDisplay.SetActive(value: true);
		}
		else
		{
			countDisplay.SetActive(value: false);
		}
		if (PlayerStorage.IsAccessableAndNotFull())
		{
			bgImage.color = colorNormal;
			text.text = textKeyNormal.ToPlainText();
		}
		else
		{
			bgImage.color = colorFull;
			text.text = textKeyInventoryFull.ToPlainText();
		}
		text.gameObject.SetActive(value: true);
		loadingIndicator.SetActive(value: false);
	}
}
