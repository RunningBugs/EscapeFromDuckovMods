using Duckov;
using Duckov.Scenes;
using Duckov.Weathers;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.Serialization;

public class WeatherFxControl : MonoBehaviour
{
	public ParticleSystem[] rainyFxParticles;

	[HideInInspector]
	public float[] rainingParticleRate;

	public Weather targetWeather;

	private float targetParticleRate;

	private float lerpValue;

	public float lerpTime = 5f;

	public float deactiveDelay = 10f;

	private float overTimer;

	private bool fxActive;

	private bool inited;

	private EventInstance? weatherSoundInstace;

	public string rainSoundKey = "Amb/amb_rain";

	private bool audioPlaying;

	[FormerlySerializedAs("onlyInDoor")]
	public bool onlyOutDoor = true;

	private void Start()
	{
	}

	private void Init()
	{
		inited = true;
		rainingParticleRate = new float[rainyFxParticles.Length];
		for (int i = 0; i < rainyFxParticles.Length; i++)
		{
			ParticleSystem.EmissionModule emission = rainyFxParticles[i].emission;
			rainingParticleRate[i] = emission.rateOverTime.constant;
		}
		SetFxActive(active: false);
	}

	private void OnSubSceneChanged()
	{
	}

	private void Update()
	{
		if (!inited)
		{
			if ((bool)LevelManager.Instance && LevelManager.LevelInited)
			{
				Init();
				SetFxActive(active: false);
			}
		}
		else
		{
			if (!TimeOfDayController.Instance || !MultiSceneCore.Instance)
			{
				return;
			}
			bool flag = TimeOfDayController.Instance.CurrentWeather == targetWeather;
			SubSceneEntry subSceneInfo = MultiSceneCore.Instance.GetSubSceneInfo();
			if (onlyOutDoor && subSceneInfo.IsInDoor)
			{
				flag = false;
				lerpValue = 0f;
			}
			if (flag)
			{
				overTimer = deactiveDelay;
				if (!fxActive)
				{
					SetFxActive(active: true);
				}
			}
			else if (lerpValue <= 0.01f)
			{
				overTimer -= Time.deltaTime;
				if (overTimer <= 0f)
				{
					SetFxActive(active: false);
				}
			}
			if (!fxActive)
			{
				return;
			}
			lerpValue = Mathf.MoveTowards(lerpValue, flag ? 1f : 0f, Time.deltaTime / lerpTime);
			for (int i = 0; i < rainyFxParticles.Length; i++)
			{
				ParticleSystem.EmissionModule emission = rainyFxParticles[i].emission;
				float b = rainingParticleRate[i];
				emission.rateOverTime = Mathf.Lerp(0f, b, lerpValue);
			}
			if (flag != audioPlaying)
			{
				audioPlaying = flag;
				if (flag)
				{
					weatherSoundInstace = AudioManager.Post(rainSoundKey, base.gameObject);
				}
				else if (weatherSoundInstace.HasValue)
				{
					weatherSoundInstace.Value.stop(STOP_MODE.ALLOWFADEOUT);
				}
			}
		}
	}

	private void SetFxActive(bool active)
	{
		ParticleSystem[] array = rainyFxParticles;
		foreach (ParticleSystem particleSystem in array)
		{
			if (!(particleSystem == null))
			{
				particleSystem.gameObject.SetActive(active);
			}
		}
		fxActive = active;
	}

	private void OnDestroy()
	{
		if (weatherSoundInstace.HasValue)
		{
			weatherSoundInstace.Value.stop(STOP_MODE.ALLOWFADEOUT);
		}
	}
}
