using Duckov;
using Duckov.Quests;
using Duckov.UI;
using Saves;
using SodaCraft.Localizations;
using UnityEngine;

public class SpaceShipInstaller : MonoBehaviour
{
	[SerializeField]
	private string saveDataKey;

	[SerializeField]
	private int questID;

	[SerializeField]
	private InteractableBase interactable;

	[SerializeField]
	[LocalizationKey("Default")]
	private string notificationKey;

	[SerializeField]
	[LocalizationKey("Default")]
	private string interactKey;

	private bool inited;

	public GameObject builtGraphic;

	public GameObject unbuiltGraphic;

	public GameObject buildFx;

	private bool Installed
	{
		get
		{
			return SavesSystem.Load<bool>(saveDataKey);
		}
		set
		{
			SavesSystem.Save(saveDataKey, value);
		}
	}

	private void Awake()
	{
		if ((bool)buildFx)
		{
			buildFx.SetActive(value: false);
		}
		interactable.overrideInteractName = true;
		interactable._overrideInteractNameKey = interactKey;
	}

	public void Install()
	{
		if ((bool)buildFx)
		{
			buildFx.SetActive(value: true);
		}
		AudioManager.Post("Archived/Building/Default/Constructed", base.gameObject);
		Installed = true;
		SyncGraphic(_installed: true);
		interactable.gameObject.SetActive(value: false);
		NotificationText.Push(notificationKey.ToPlainText());
	}

	private void SyncGraphic(bool _installed)
	{
		if ((bool)builtGraphic)
		{
			builtGraphic.SetActive(_installed);
		}
		if ((bool)unbuiltGraphic)
		{
			unbuiltGraphic.SetActive(!_installed);
		}
	}

	private void Update()
	{
		if (!LevelManager.LevelInited)
		{
			return;
		}
		bool flag = false;
		if (!inited)
		{
			flag = Installed;
			if (flag)
			{
				TaskEvent.EmitTaskEvent(saveDataKey);
			}
			else if (QuestManager.IsQuestFinished(questID))
			{
				flag = true;
				Installed = true;
			}
			interactable.gameObject.SetActive(!flag && QuestManager.IsQuestActive(questID));
			SyncGraphic(flag);
			inited = true;
		}
		if (!Installed && !interactable.gameObject.activeSelf && QuestManager.IsQuestActive(questID))
		{
			interactable.gameObject.SetActive(value: true);
		}
	}
}
