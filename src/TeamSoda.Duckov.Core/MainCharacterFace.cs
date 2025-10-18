using UnityEngine;

public class MainCharacterFace : MonoBehaviour
{
	public CustomFaceManager customFaceManager;

	public CustomFaceInstance customFace;

	private void Start()
	{
		CustomFaceSettingData saveData = customFaceManager.LoadMainCharacterSetting();
		customFace.LoadFromData(saveData);
	}

	private void Update()
	{
	}
}
