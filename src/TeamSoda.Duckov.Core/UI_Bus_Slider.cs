using Duckov;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Bus_Slider : MonoBehaviour
{
	private AudioManager.Bus busRef;

	[SerializeField]
	private string busName;

	[SerializeField]
	private TextMeshProUGUI volumeNumberText;

	[SerializeField]
	private Slider slider;

	private bool initialized;

	private AudioManager.Bus BusRef
	{
		get
		{
			if (!AudioManager.Initialized)
			{
				return null;
			}
			if (busRef == null)
			{
				busRef = AudioManager.GetBus(busName);
				if (busRef == null)
				{
					Debug.LogError("Bus not found:" + busName);
				}
			}
			return busRef;
		}
	}

	private void Initialize()
	{
		if (BusRef != null)
		{
			slider.SetValueWithoutNotify(BusRef.Volume);
			volumeNumberText.text = (BusRef.Volume * 100f).ToString("0");
			initialized = true;
		}
	}

	private void Awake()
	{
		slider.onValueChanged.AddListener(OnValueChanged);
	}

	private void Start()
	{
		if (!initialized)
		{
			Initialize();
		}
	}

	private void OnEnable()
	{
		Initialize();
	}

	private void OnValueChanged(float value)
	{
		if (BusRef != null)
		{
			BusRef.Volume = value;
			BusRef.Mute = value == 0f;
			volumeNumberText.text = (BusRef.Volume * 100f).ToString("0");
		}
	}
}
