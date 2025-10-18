using Duckov.Quests;
using ItemStatsSystem;
using UnityEngine;

public class Condition_XiaoHeiZi : Condition
{
	[SerializeField]
	private int hairID = 6;

	[ItemTypeID]
	[SerializeField]
	private int armorID = 379;

	public override string DisplayText => "看看你是不是小黑子";

	public override bool Evaluate()
	{
		if (CharacterMainControl.Main == null)
		{
			return false;
		}
		CharacterMainControl main = CharacterMainControl.Main;
		CharacterModel characterModel = main.characterModel;
		if (!characterModel)
		{
			return false;
		}
		CustomFaceInstance customFace = characterModel.CustomFace;
		if (!customFace)
		{
			return false;
		}
		if (customFace.ConvertToSaveData().hairID != hairID)
		{
			return false;
		}
		Item armorItem = main.GetArmorItem();
		if (armorItem == null)
		{
			return false;
		}
		if (armorItem.TypeID != armorID)
		{
			return false;
		}
		return true;
	}
}
