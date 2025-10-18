using System;
using Duckov;
using ItemStatsSystem;
using UnityEngine;

public abstract class SkillBase : MonoBehaviour
{
	public bool hasReleaseSound;

	public string onReleaseSound;

	public Sprite icon;

	public float staminaCost = 10f;

	public float coolDownTime = 1f;

	private float lastReleaseTime = -999f;

	[SerializeField]
	protected SkillContext skillContext;

	protected SkillReleaseContext skillReleaseContext;

	protected CharacterMainControl fromCharacter;

	[HideInInspector]
	public Item fromItem;

	public Action OnSkillReleasedEvent;

	public float LastReleaseTime => lastReleaseTime;

	public SkillContext SkillContext => skillContext;

	public void ReleaseSkill(SkillReleaseContext releaseContext, CharacterMainControl from)
	{
		lastReleaseTime = Time.time;
		skillReleaseContext = releaseContext;
		fromCharacter = from;
		fromCharacter.UseStamina(staminaCost);
		if (hasReleaseSound && fromCharacter != null && onReleaseSound != "")
		{
			AudioManager.Post(onReleaseSound, from.gameObject);
		}
		OnRelease();
		OnSkillReleasedEvent?.Invoke();
	}

	public abstract void OnRelease();
}
