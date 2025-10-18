using System;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner;

public class GoldMinerEntity : MiniGameBehaviour
{
	public enum Size
	{
		XS = -2,
		S,
		M,
		L,
		XL
	}

	public enum Tag
	{
		None,
		Rock,
		Gold,
		Diamond,
		Mine,
		Chest,
		Pig,
		Cable
	}

	[SerializeField]
	private string typeID;

	[SerializeField]
	public Size size;

	[SerializeField]
	public Tag[] tags;

	[SerializeField]
	private int value;

	[SerializeField]
	private float speed = 1f;

	[SerializeField]
	private ParticleSystem contactFX;

	[SerializeField]
	private ParticleSystem beginMoveFX;

	[SerializeField]
	private ParticleSystem resolveFX;

	[SerializeField]
	private ParticleSystem explodeFX;

	public Action<GoldMinerEntity, Hook> OnAttached;

	public Action<GoldMinerEntity, GoldMiner> OnResolved;

	public GoldMiner master { get; private set; }

	public string TypeID => typeID;

	public float Speed => speed;

	public int Value
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
		}
	}

	public void SetMaster(GoldMiner master)
	{
		this.master = master;
	}

	public void NotifyAttached(Hook hook)
	{
		OnAttached?.Invoke(this, hook);
		FXPool.Play(contactFX, base.transform.position, base.transform.rotation);
	}

	public void NotifyBeginRetrieving()
	{
		FXPool.Play(beginMoveFX, base.transform.position, base.transform.rotation);
	}

	internal void Explode(Vector3 origin)
	{
		UnityEngine.Object.Destroy(base.gameObject);
		FXPool.Play(explodeFX, base.transform.position, base.transform.rotation);
	}

	internal void NotifyResolved(GoldMiner game)
	{
		OnResolved?.Invoke(this, game);
		FXPool.Play(resolveFX, base.transform.position, base.transform.rotation);
	}
}
