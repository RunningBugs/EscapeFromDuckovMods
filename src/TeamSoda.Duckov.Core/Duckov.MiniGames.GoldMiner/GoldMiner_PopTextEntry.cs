using System;
using TMPro;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner;

public class GoldMiner_PopTextEntry : MonoBehaviour
{
	public TextMeshProUGUI tmp;

	public float lifeTime;

	public float moveSpeed = 1f;

	private bool initialized;

	private float life;

	private Action<GoldMiner_PopTextEntry> releaseAction;

	public void Setup(Vector3 pos, string text, Action<GoldMiner_PopTextEntry> releaseAction)
	{
		initialized = true;
		tmp.text = text;
		life = 0f;
		base.transform.position = pos;
		this.releaseAction = releaseAction;
	}

	private void Update()
	{
		if (initialized)
		{
			life += Time.deltaTime;
			base.transform.position += Vector3.up * moveSpeed * Time.deltaTime;
			if (life >= lifeTime)
			{
				Release();
			}
		}
	}

	private void Release()
	{
		if (releaseAction != null)
		{
			releaseAction(this);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
