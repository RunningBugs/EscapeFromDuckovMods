using ItemStatsSystem;
using UnityEngine;
using UnityEngine.Serialization;

public class KunEvents : MonoBehaviour
{
	[SerializeField]
	private int hairID = 6;

	[ItemTypeID]
	[SerializeField]
	private int armorID;

	public DialogueBubbleProxy dialogueBubbleProxy;

	[LocalizationKey("Dialogues")]
	public string notRight;

	[LocalizationKey("Dialogues")]
	public string onlyRightFace;

	[LocalizationKey("Dialogues")]
	public string onlyRightCloth;

	[LocalizationKey("Dialogues")]
	public string allRight;

	[FormerlySerializedAs("SetActiveObject")]
	public GameObject setActiveObject;

	private void Awake()
	{
		setActiveObject.SetActive(value: false);
		if (!dialogueBubbleProxy)
		{
			dialogueBubbleProxy.GetComponent<DialogueBubbleProxy>();
		}
	}

	public void Check()
	{
		bool flag = false;
		bool flag2 = false;
		if (CharacterMainControl.Main == null)
		{
			return;
		}
		CharacterMainControl main = CharacterMainControl.Main;
		CharacterModel characterModel = main.characterModel;
		if (!characterModel)
		{
			return;
		}
		CustomFaceInstance customFace = characterModel.CustomFace;
		if ((bool)customFace)
		{
			flag = customFace.ConvertToSaveData().hairID == hairID;
			Item armorItem = main.GetArmorItem();
			if (armorItem != null && armorItem.TypeID == armorID)
			{
				flag2 = true;
			}
			if (!flag && !flag2)
			{
				dialogueBubbleProxy.textKey = notRight;
			}
			else if (flag && !flag2)
			{
				dialogueBubbleProxy.textKey = onlyRightFace;
			}
			else if (!flag && flag2)
			{
				dialogueBubbleProxy.textKey = onlyRightCloth;
			}
			else
			{
				dialogueBubbleProxy.textKey = allRight;
				setActiveObject.SetActive(value: true);
			}
			dialogueBubbleProxy.Pop();
		}
	}
}
