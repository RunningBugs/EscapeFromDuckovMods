using Cysharp.Threading.Tasks;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIKeybindingEntry : MonoBehaviour
{
	[SerializeField]
	private InputActionReference actionRef;

	[SerializeField]
	private int index;

	[SerializeField]
	private string overrideDisplayNameKey;

	private string[] excludes = new string[6] { "<Mouse>/leftButton", "<Mouse>/rightButton", "<Pointer>/position", "<Pointer>/delta", "<Pointer>/press", "<Mouse>/scroll" };

	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private Button rebindButton;

	[SerializeField]
	private InputIndicator indicator;

	[LocalizationKey("UIText")]
	private string displayNameKey
	{
		get
		{
			if (!string.IsNullOrEmpty(overrideDisplayNameKey))
			{
				return overrideDisplayNameKey;
			}
			if (actionRef == null)
			{
				return "?";
			}
			return "Input_" + actionRef.action.name;
		}
		set
		{
		}
	}

	private void Awake()
	{
		rebindButton.onClick.AddListener(OnButtonClick);
		Setup();
		LocalizationManager.OnSetLanguage += OnLanguageChanged;
	}

	private void OnDestroy()
	{
		LocalizationManager.OnSetLanguage -= OnLanguageChanged;
	}

	private void OnLanguageChanged(SystemLanguage language)
	{
		label.text = displayNameKey.ToPlainText();
	}

	private void OnButtonClick()
	{
		InputRebinder.RebindAsync(actionRef.action.name, index, excludes, save: true).Forget();
	}

	private void OnValidate()
	{
		Setup();
	}

	private void Setup()
	{
		indicator.Setup(actionRef, index);
		label.text = displayNameKey.ToPlainText();
	}
}
