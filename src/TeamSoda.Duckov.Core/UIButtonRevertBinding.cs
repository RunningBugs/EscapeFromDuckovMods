using UnityEngine;
using UnityEngine.UI;

public class UIButtonRevertBinding : MonoBehaviour
{
	[SerializeField]
	private Button button;

	private void Awake()
	{
		if (button == null)
		{
			button = GetComponent<Button>();
		}
		button.onClick.AddListener(OnBtnClick);
	}

	public void OnBtnClick()
	{
		InputRebinder.Clear();
		InputRebinder.Save();
	}
}
