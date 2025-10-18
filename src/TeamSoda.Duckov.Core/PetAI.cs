using ItemStatsSystem;
using UnityEngine;

public class PetAI : MonoBehaviour
{
	[HideInInspector]
	public CharacterMainControl master;

	public AICharacterController selfAiCharacterController;

	public bool setMainCharacterAsMaster;

	public bool standBy;

	public Vector3 standByPos = Vector3.zero;

	private bool statInited;

	private Stat walkSpeedStat;

	private Stat runSpeedStat;

	private CharacterMainControl selfCharacter;

	private float distanceToMaster;

	private void Start()
	{
		if (setMainCharacterAsMaster)
		{
			SetMaster(CharacterMainControl.Main);
		}
	}

	public void SetMaster(CharacterMainControl _master)
	{
		if ((bool)_master)
		{
			master = _master;
			_master.OnSetPositionEvent -= OnMainCharacterSetPosition;
			_master.OnSetPositionEvent += OnMainCharacterSetPosition;
		}
	}

	private void SyncSpeed()
	{
		if ((bool)master)
		{
			if (distanceToMaster > 10f)
			{
				walkSpeedStat.BaseValue = master.CharacterWalkSpeed + 2f;
				runSpeedStat.BaseValue = master.CharacterRunSpeed + 2f;
			}
			else if (distanceToMaster < 8f)
			{
				walkSpeedStat.BaseValue = master.CharacterWalkSpeed;
				runSpeedStat.BaseValue = master.CharacterRunSpeed;
			}
		}
	}

	private void InitStats()
	{
		if ((bool)selfCharacter)
		{
			Item characterItem = selfCharacter.CharacterItem;
			if ((bool)characterItem)
			{
				statInited = true;
				walkSpeedStat = characterItem.Stats["WalkSpeed"];
				runSpeedStat = characterItem.Stats["RunSpeed"];
			}
		}
	}

	private void OnDestroy()
	{
		if (master != null)
		{
			master.OnSetPositionEvent -= OnMainCharacterSetPosition;
		}
	}

	private void Awake()
	{
	}

	private void Update()
	{
		if (!selfCharacter && (bool)selfAiCharacterController)
		{
			selfCharacter = selfAiCharacterController.CharacterMainControl;
		}
		if (master != null)
		{
			if (!statInited)
			{
				InitStats();
			}
			distanceToMaster = Vector3.Distance(base.transform.position, master.transform.position);
			SyncSpeed();
			if (!standBy && distanceToMaster > 40f)
			{
				SetPosition(master.transform.position + Vector3.forward * 0.4f + Vector3.up * 0.5f);
			}
		}
	}

	private void OnMainCharacterSetPosition(CharacterMainControl character, Vector3 targetPos)
	{
		if ((bool)master && master.IsMainCharacter && !LevelManager.Instance.IsBaseLevel)
		{
			SetPosition(targetPos + Vector3.forward * 0.4f + Vector3.up * 0.5f);
		}
	}

	private void SetPosition(Vector3 targetPos)
	{
		selfAiCharacterController.CharacterMainControl.SetPosition(targetPos);
	}

	public void SetStandBy(bool _standBy, Vector3 pos)
	{
		standBy = _standBy;
		standByPos = pos;
	}
}
