using UnityEngine;
using UnityEngine.Events;

public class OnTriggerEnterEvent : MonoBehaviour
{
	public bool onlyMainCharacter;

	public bool filterByTeam;

	public Teams selfTeam;

	public LayerMask layerMask;

	public bool triggerOnce;

	public UnityEvent DoOnTriggerEnter = new UnityEvent();

	public UnityEvent DoOnTriggerExit = new UnityEvent();

	private bool triggered;

	private bool mainCharacterIn;

	private bool hideLayerMask
	{
		get
		{
			if (!onlyMainCharacter)
			{
				return filterByTeam;
			}
			return true;
		}
	}

	private void Awake()
	{
		Init();
	}

	public void Init()
	{
		Collider component = GetComponent<Collider>();
		if ((bool)component)
		{
			component.isTrigger = true;
		}
		if (filterByTeam)
		{
			layerMask = 1 << LayerMask.NameToLayer("Character");
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		OnEvent(collision.gameObject, enter: true);
	}

	private void OnCollisionExit(Collision collision)
	{
		OnEvent(collision.gameObject, enter: false);
	}

	private void OnTriggerEnter(Collider other)
	{
		OnEvent(other.gameObject, enter: true);
	}

	private void OnTriggerExit(Collider other)
	{
		OnEvent(other.gameObject, enter: false);
	}

	private void OnEvent(GameObject other, bool enter)
	{
		if (triggerOnce && triggered)
		{
			return;
		}
		if (onlyMainCharacter)
		{
			if (CharacterMainControl.Main == null || other != CharacterMainControl.Main.gameObject)
			{
				return;
			}
		}
		else
		{
			if (((1 << other.layer) | (int)layerMask) != (int)layerMask)
			{
				return;
			}
			if (filterByTeam)
			{
				CharacterMainControl component = other.GetComponent<CharacterMainControl>();
				if (!component)
				{
					return;
				}
				Teams team = component.Team;
				if (!Team.IsEnemy(selfTeam, team))
				{
					return;
				}
			}
		}
		triggered = true;
		if (enter)
		{
			DoOnTriggerEnter?.Invoke();
		}
		else
		{
			DoOnTriggerExit?.Invoke();
		}
	}
}
