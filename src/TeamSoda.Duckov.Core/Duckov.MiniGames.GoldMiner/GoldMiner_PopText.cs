using Duckov.Utilities;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner;

public class GoldMiner_PopText : MiniGameBehaviour
{
	[SerializeField]
	private GoldMiner_PopTextEntry textTemplate;

	private PrefabPool<GoldMiner_PopTextEntry> _textPool;

	private PrefabPool<GoldMiner_PopTextEntry> TextPool
	{
		get
		{
			if (_textPool == null)
			{
				_textPool = new PrefabPool<GoldMiner_PopTextEntry>(textTemplate);
			}
			return _textPool;
		}
	}

	public void Pop(string content, Vector3 position)
	{
		TextPool.Get().Setup(position, content, ReleaseEntry);
	}

	private void ReleaseEntry(GoldMiner_PopTextEntry entry)
	{
		TextPool.Release(entry);
	}
}
