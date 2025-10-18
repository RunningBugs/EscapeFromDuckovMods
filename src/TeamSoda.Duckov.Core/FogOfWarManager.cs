using FOW;
using UnityEngine;
using UnityEngine.Serialization;

public class FogOfWarManager : MonoBehaviour
{
	[FormerlySerializedAs("mianVis")]
	public FogOfWarRevealer3D mainVis;

	public float mianVisYOffset = 1f;

	private CharacterMainControl character;

	public FogOfWarWorld fogOfWar;

	private float viewAgnel = -1f;

	private float senseRange = -1f;

	private float viewDistance = -1f;

	private TimeOfDayController timeOfDayController;

	private bool allVision;

	private bool inited;

	private void Start()
	{
		LevelManager.OnMainCharacterDead += OnCharacterDie;
	}

	private void OnDestroy()
	{
		LevelManager.OnMainCharacterDead -= OnCharacterDie;
	}

	private void Init()
	{
		inited = true;
		if (!LevelManager.Instance.IsRaidMap || !LevelManager.Rule.FogOfWar)
		{
			allVision = true;
		}
	}

	private void Update()
	{
		if (!LevelManager.LevelInited)
		{
			return;
		}
		if (!character)
		{
			character = CharacterMainControl.Main;
			if (!character)
			{
				return;
			}
		}
		if (!inited)
		{
			Init();
		}
		if (!timeOfDayController)
		{
			timeOfDayController = LevelManager.Instance.TimeOfDayController;
			if (!timeOfDayController)
			{
				return;
			}
		}
		Vector3 position = character.transform.position + Vector3.up * mianVisYOffset;
		mainVis.transform.position = position;
		position = new Vector3(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), Mathf.RoundToInt(position.z));
		fogOfWar.UpdateWorldBounds(position, new Vector3(128f, 1f, 128f));
		Vector3 forward = character.GetCurrentAimPoint() - character.transform.position;
		Debug.DrawLine(character.GetCurrentAimPoint(), character.GetCurrentAimPoint() + Vector3.up * 2f, Color.green, 0.2f);
		forward.y = 0f;
		forward.Normalize();
		float t = Mathf.Clamp01(character.NightVisionAbility + (character.FlashLight ? 0.3f : 0f));
		float viewAngle = character.ViewAngle;
		float num = character.SenseRange;
		float num2 = character.ViewDistance;
		viewAngle *= Mathf.Lerp(TimeOfDayController.NightViewAngleFactor, 1f, t);
		num *= Mathf.Lerp(TimeOfDayController.NightSenseRangeFactor, 1f, t);
		num2 *= Mathf.Lerp(TimeOfDayController.NightViewDistanceFactor, 1f, t);
		if (num2 < num - 2.5f)
		{
			num2 = num - 2.5f;
		}
		if (allVision)
		{
			viewAngle = 360f;
			num = 50f;
			num2 = 50f;
		}
		if (viewAngle != viewAgnel)
		{
			if (viewAgnel < 0f)
			{
				viewAgnel = viewAngle;
			}
			viewAgnel = Mathf.MoveTowards(viewAgnel, viewAngle, 120f * Time.deltaTime);
			mainVis.ViewAngle = viewAgnel;
		}
		if (num != senseRange)
		{
			if (senseRange < 0f)
			{
				senseRange = num;
			}
			senseRange = Mathf.MoveTowards(senseRange, num, 2f * Time.deltaTime);
			mainVis.UnobscuredRadius = senseRange;
		}
		if (num2 != viewDistance)
		{
			if (viewDistance < 0f)
			{
				viewDistance = num2;
			}
			viewDistance = Mathf.MoveTowards(viewDistance, num2, 30f * Time.deltaTime);
			mainVis.ViewRadius = viewDistance;
		}
		mainVis.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
	}

	private void OnCharacterDie(DamageInfo dmgInfo)
	{
		LevelManager.OnMainCharacterDead -= OnCharacterDie;
		Object.Destroy(base.gameObject);
	}
}
