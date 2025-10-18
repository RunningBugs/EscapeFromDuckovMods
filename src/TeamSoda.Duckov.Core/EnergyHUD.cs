using ItemStatsSystem;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;

public class EnergyHUD : MonoBehaviour
{
	private CharacterMainControl characterMainControl;

	private float percent = -1f;

	public ProceduralImage fillImage;

	public ProceduralImage backgroundImage;

	public Color backgroundColor;

	public Color emptyBackgroundColor;

	private Item item => characterMainControl.CharacterItem;

	private void Update()
	{
		if (!characterMainControl)
		{
			characterMainControl = LevelManager.Instance.MainCharacter;
			if (!characterMainControl)
			{
				return;
			}
		}
		float a = characterMainControl.CurrentEnergy / characterMainControl.MaxEnergy;
		if (!Mathf.Approximately(a, percent))
		{
			percent = a;
			fillImage.fillAmount = percent;
			if (percent <= 0f)
			{
				backgroundImage.color = emptyBackgroundColor;
			}
			else
			{
				backgroundImage.color = backgroundColor;
			}
		}
	}
}
