using System.Collections.Generic;
using UnityEngine;

namespace Duckov;

public class CursorManager : MonoBehaviour
{
	[SerializeField]
	private CursorData defaultCursor;

	public CursorData currentCursor;

	private static List<ICursorDataProvider> cursorDataStack = new List<ICursorDataProvider>();

	private int frame;

	private float fpsBuffer;

	public static CursorManager Instance { get; private set; }

	public static void Register(ICursorDataProvider dataProvider)
	{
		cursorDataStack.Add(dataProvider);
		ApplyStackData();
	}

	public static bool Unregister(ICursorDataProvider dataProvider)
	{
		if (cursorDataStack.Count < 1)
		{
			return false;
		}
		if (!cursorDataStack.Contains(dataProvider))
		{
			return false;
		}
		bool result = cursorDataStack.Remove(dataProvider);
		ApplyStackData();
		return result;
	}

	private static void ApplyStackData()
	{
		if (Instance == null)
		{
			return;
		}
		if (cursorDataStack.Count <= 0)
		{
			Instance.MSetDefaultCursor();
			return;
		}
		ICursorDataProvider cursorDataProvider = cursorDataStack[cursorDataStack.Count - 1];
		if (cursorDataProvider == null)
		{
			Instance.MSetDefaultCursor();
		}
		Instance.MSetCursor(cursorDataProvider.GetCursorData());
	}

	private void Awake()
	{
		Instance = this;
		MSetCursor(defaultCursor);
	}

	private void Update()
	{
		if (currentCursor != null && currentCursor.textures.Length >= 2)
		{
			fpsBuffer += Time.unscaledDeltaTime * currentCursor.fps;
			if (fpsBuffer > 1f)
			{
				fpsBuffer = 0f;
				frame++;
				RefreshCursor();
			}
		}
	}

	private void RefreshCursor()
	{
		if (currentCursor != null)
		{
			currentCursor.Apply(frame);
		}
	}

	public void MSetDefaultCursor()
	{
		MSetCursor(defaultCursor);
	}

	public void MSetCursor(CursorData data)
	{
		currentCursor = data;
		frame = 12;
		RefreshCursor();
	}

	private void OnDestroy()
	{
		Cursor.SetCursor(null, default(Vector2), CursorMode.Auto);
	}

	internal static void NotifyRefresh()
	{
		ApplyStackData();
	}
}
