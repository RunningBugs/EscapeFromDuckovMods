using UnityEngine;
using UnityEngine.UIElements;

public class UIToolkitTest : MonoBehaviour
{
	[SerializeField]
	private UIDocument doc;

	private void Awake()
	{
		VisualElement visualElement = doc.rootVisualElement.Q("Button");
		VisualElement visualElement2 = doc.rootVisualElement.Q("Button2");
		visualElement.RegisterCallback<ClickEvent>(OnButtonClicked);
		visualElement2.RegisterCallback<ClickEvent>(OnButton2Clicked);
	}

	private void OnButton2Clicked(ClickEvent evt)
	{
		Debug.Log("Button 2 Clicked");
	}

	private void OnButtonClicked(ClickEvent evt)
	{
		Debug.Log("Button Clicked");
	}
}
