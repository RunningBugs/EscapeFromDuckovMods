using System;
using UnityEngine;

namespace Duckov.MiniGames;

public class MiniGameBehaviour : MonoBehaviour
{
	[SerializeField]
	private MiniGame game;

	public MiniGame Game => game;

	public void SetGame(MiniGame game = null)
	{
		if (game == null)
		{
			this.game = GetComponentInParent<MiniGame>();
		}
		else
		{
			this.game = game;
		}
	}

	private void OnUpdateLogic(MiniGame game, float deltaTime)
	{
		if (!(this == null) && base.enabled && !(game == null) && !(game != this.game))
		{
			OnUpdate(deltaTime);
		}
	}

	protected virtual void OnEnable()
	{
		MiniGame.onUpdateLogic = (Action<MiniGame, float>)Delegate.Combine(MiniGame.onUpdateLogic, new Action<MiniGame, float>(OnUpdateLogic));
	}

	protected virtual void OnDisable()
	{
		MiniGame.onUpdateLogic = (Action<MiniGame, float>)Delegate.Remove(MiniGame.onUpdateLogic, new Action<MiniGame, float>(OnUpdateLogic));
	}

	private void OnDestroy()
	{
		MiniGame.onUpdateLogic = (Action<MiniGame, float>)Delegate.Remove(MiniGame.onUpdateLogic, new Action<MiniGame, float>(OnUpdateLogic));
	}

	protected virtual void Start()
	{
		if (game == null)
		{
			SetGame();
		}
	}

	protected virtual void OnUpdate(float deltaTime)
	{
	}
}
