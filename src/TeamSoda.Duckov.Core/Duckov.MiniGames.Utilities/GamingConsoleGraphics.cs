using Cysharp.Threading.Tasks;
using ItemStatsSystem;
using UnityEngine;

namespace Duckov.MiniGames.Utilities;

public class GamingConsoleGraphics : MonoBehaviour
{
	[SerializeField]
	private GamingConsole master;

	[SerializeField]
	private Transform monitorRoot;

	[SerializeField]
	private Transform consoleRoot;

	[SerializeField]
	private Transform playingControllerPosition;

	private Transform cartridgeRoot;

	private Item _cachedMonitor;

	private Item _cachedConsole;

	private Item _cachedCartridge;

	private ItemGraphicInfo monitorGraphic;

	private ItemGraphicInfo consoleGraphic;

	private ControllerPickupAnimation pickupAnimation;

	private ControllerAnimator controllerAnimator;

	private bool dirty;

	private bool isBeingDestroyed;

	private void Awake()
	{
		master.onContentChanged += OnContentChanged;
		master.OnAfterAnimateIn += OnAfterAnimateIn;
		master.OnBeforeAnimateOut += OnBeforeAnimateOut;
	}

	private void Start()
	{
		dirty = true;
	}

	private void OnContentChanged(GamingConsole console)
	{
		if (console.Monitor != _cachedMonitor)
		{
			OnMonitorChanged();
		}
		if (console.Console != _cachedConsole)
		{
			OnConsoleChanged();
		}
		if (console.Cartridge != _cachedCartridge)
		{
			OnCatridgeChanged();
		}
		dirty = true;
	}

	private void Update()
	{
		if (dirty)
		{
			RefreshDisplays();
			dirty = false;
		}
	}

	private void RefreshDisplays()
	{
		if (isBeingDestroyed)
		{
			return;
		}
		_cachedMonitor = master.Monitor;
		_cachedConsole = master.Console;
		_cachedCartridge = master.Cartridge;
		if ((bool)monitorGraphic)
		{
			Object.Destroy(monitorGraphic.gameObject);
		}
		if ((bool)consoleGraphic)
		{
			Object.Destroy(consoleGraphic.gameObject);
		}
		if ((bool)_cachedMonitor && !_cachedMonitor.IsBeingDestroyed)
		{
			monitorGraphic = ItemGraphicInfo.CreateAGraphic(_cachedMonitor, monitorRoot);
		}
		if ((bool)_cachedConsole && !_cachedConsole.IsBeingDestroyed)
		{
			consoleGraphic = ItemGraphicInfo.CreateAGraphic(_cachedConsole, consoleRoot);
			if (consoleGraphic != null)
			{
				pickupAnimation = consoleGraphic.GetComponent<ControllerPickupAnimation>();
				controllerAnimator = consoleGraphic.GetComponentInChildren<ControllerAnimator>();
			}
			else
			{
				pickupAnimation = null;
				controllerAnimator = null;
			}
			if (controllerAnimator != null)
			{
				controllerAnimator.SetConsole(master);
			}
		}
	}

	private void OnCatridgeChanged()
	{
	}

	private void OnConsoleChanged()
	{
	}

	private void OnMonitorChanged()
	{
	}

	private void OnDestroy()
	{
		isBeingDestroyed = true;
	}

	private void OnBeforeAnimateOut(GamingConsole console)
	{
		if (!(pickupAnimation == null))
		{
			pickupAnimation.PutDown().Forget();
		}
	}

	private void OnAfterAnimateIn(GamingConsole console)
	{
		if (!(pickupAnimation == null))
		{
			pickupAnimation.PickUp(playingControllerPosition).Forget();
		}
	}
}
