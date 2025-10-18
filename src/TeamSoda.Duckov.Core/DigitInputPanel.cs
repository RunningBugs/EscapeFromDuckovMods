using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DigitInputPanel : MonoBehaviour
{
	[SerializeField]
	private TMP_InputField inputField;

	[SerializeField]
	private Button clearButton;

	[SerializeField]
	private Button backspaceButton;

	[SerializeField]
	private Button maximumButton;

	[SerializeField]
	private Button[] numKeys;

	public Func<long> maxFunction;

	public long Value
	{
		get
		{
			string text = inputField.text;
			if (string.IsNullOrEmpty(text))
			{
				return 0L;
			}
			if (!long.TryParse(text, out var result))
			{
				return 0L;
			}
			return result;
		}
	}

	public event Action<string> onInputFieldValueChanged;

	private void Awake()
	{
		inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
		for (int i = 0; i < numKeys.Length; i++)
		{
			int v = i;
			numKeys[i].onClick.AddListener(delegate
			{
				OnNumKeyClicked(v);
			});
		}
		clearButton.onClick.AddListener(OnClearButtonClicked);
		backspaceButton.onClick.AddListener(OnBackspaceButtonClicked);
		maximumButton.onClick.AddListener(Max);
	}

	private void OnBackspaceButtonClicked()
	{
		if (!string.IsNullOrEmpty(inputField.text))
		{
			inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
		}
	}

	private void OnClearButtonClicked()
	{
		inputField.text = string.Empty;
	}

	private void OnNumKeyClicked(long v)
	{
		inputField.text = $"{inputField.text}{v}";
	}

	private void OnInputFieldValueChanged(string value)
	{
		if (long.TryParse(value, out var result) && result == 0L)
		{
			inputField.SetTextWithoutNotify(string.Empty);
		}
		this.onInputFieldValueChanged?.Invoke(value);
	}

	public void Setup(long value, Func<long> maxFunc = null)
	{
		maxFunction = maxFunc;
		inputField.text = $"{value}";
	}

	public void Max()
	{
		if (maxFunction != null)
		{
			long num = maxFunction();
			inputField.text = $"{num}";
		}
	}

	internal void Clear()
	{
		inputField.text = string.Empty;
	}
}
