using System.Collections.Generic;
using System.Linq;
using Duckov.Options;
using Sirenix.Utilities;
using SodaCraft.Localizations;
using UnityEngine;

public class ResolutionSetter : MonoBehaviour
{
	public enum screenModes
	{
		Borderless,
		Window
	}

	public static string Key_Resolution = "Resolution";

	public static string Key_ScreenMode = "ScreenMode";

	public static bool currentFullScreen = false;

	private static float fullScreenChangeCheckCoolTimer = 1f;

	private static float fullScreenChangeCheckCoolTime = 1f;

	public Vector2Int debugDisplayRes = new Vector2Int(0, 0);

	public Vector2Int debugScreenRes = new Vector2Int(0, 0);

	public Vector2Int debugmMaxRes = new Vector2Int(0, 0);

	public DuckovResolution[] testRes;

	public static DuckovResolution MaxResolution
	{
		get
		{
			Resolution[] resolutions = Screen.resolutions;
			resolutions.Sort(delegate(Resolution A, Resolution B)
			{
				if (A.height > B.height)
				{
					return -1;
				}
				if (A.height < B.height)
				{
					return 1;
				}
				if (A.width > B.width)
				{
					return -1;
				}
				return (A.width < B.width) ? 1 : 0;
			});
			Resolution res = new Resolution
			{
				width = Screen.currentResolution.width,
				height = Screen.currentResolution.height
			};
			Resolution res2 = Screen.resolutions[resolutions.Length - 1];
			DuckovResolution result = ((res.width <= res2.width) ? new DuckovResolution(res2) : new DuckovResolution(res));
			if ((float)result.width / (float)result.height < 1.4f)
			{
				result.width = Mathf.RoundToInt(result.height * 16 / 9);
			}
			return result;
		}
	}

	private void Test()
	{
		debugDisplayRes = new Vector2Int(Display.main.systemWidth, Display.main.systemHeight);
		debugmMaxRes = new Vector2Int(MaxResolution.width, MaxResolution.height);
		debugScreenRes = new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);
		testRes = GetResolutions();
	}

	public static Resolution GetResByHeight(int height, DuckovResolution maxRes)
	{
		return new Resolution
		{
			height = height,
			width = (int)((float)maxRes.width * (float)height / (float)maxRes.height)
		};
	}

	public static DuckovResolution[] GetResolutions()
	{
		DuckovResolution maxResolution = MaxResolution;
		List<Resolution> list = Screen.resolutions.ToList();
		list.Add(GetResByHeight(1080, maxResolution));
		list.Add(GetResByHeight(900, maxResolution));
		list.Add(GetResByHeight(720, maxResolution));
		list.Add(GetResByHeight(540, maxResolution));
		List<DuckovResolution> list2 = new List<DuckovResolution>();
		bool flag = OptionsManager.Load(Key_ScreenMode, screenModes.Window) != screenModes.Window;
		foreach (Resolution item in list)
		{
			DuckovResolution duckovResolution = new DuckovResolution(item);
			if (!list2.Contains(duckovResolution) && !((float)duckovResolution.width / (float)duckovResolution.height < 1.4f) && (!flag || duckovResolution.CheckRotioFit(duckovResolution, maxResolution)))
			{
				list2.Add(duckovResolution);
			}
		}
		list2.Sort(delegate(DuckovResolution A, DuckovResolution B)
		{
			if (A.height > B.height)
			{
				return -1;
			}
			if (A.height < B.height)
			{
				return 1;
			}
			if (A.width > B.width)
			{
				return -1;
			}
			return (A.width < B.width) ? 1 : 0;
		});
		return list2.ToArray();
	}

	private void Update()
	{
		UpdateFullScreenCheck();
	}

	private void UpdateFullScreenCheck()
	{
		fullScreenChangeCheckCoolTimer -= Time.unscaledDeltaTime;
		if (!(fullScreenChangeCheckCoolTimer > 0f) && currentFullScreen != (Screen.fullScreenMode == FullScreenMode.FullScreenWindow || Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen))
		{
			currentFullScreen = !currentFullScreen;
			OptionsManager.Save(Key_ScreenMode, (!currentFullScreen) ? screenModes.Window : screenModes.Borderless);
			fullScreenChangeCheckCoolTimer = fullScreenChangeCheckCoolTime;
		}
	}

	public static void UpdateResolutionAndScreenMode()
	{
		fullScreenChangeCheckCoolTimer = fullScreenChangeCheckCoolTime;
		DuckovResolution duckovResolution = OptionsManager.Load(Key_Resolution, new DuckovResolution(Screen.resolutions[Screen.resolutions.Length - 1]));
		if ((float)duckovResolution.width / (float)duckovResolution.height < 1.3666667f)
		{
			duckovResolution.width = Mathf.RoundToInt(duckovResolution.height * 16 / 9);
		}
		screenModes screenModes = OptionsManager.Load(Key_ScreenMode, screenModes.Borderless);
		currentFullScreen = screenModes == screenModes.Borderless;
		Screen.SetResolution(duckovResolution.width, duckovResolution.height, ScreenModeToFullScreenMode(screenModes));
	}

	private static FullScreenMode ScreenModeToFullScreenMode(screenModes screenMode)
	{
		return screenMode switch
		{
			screenModes.Borderless => FullScreenMode.FullScreenWindow, 
			screenModes.Window => FullScreenMode.Windowed, 
			_ => FullScreenMode.ExclusiveFullScreen, 
		};
	}

	public static string[] GetScreenModes()
	{
		return new string[2]
		{
			("Option_ScreenMode_" + screenModes.Borderless).ToPlainText(),
			("Option_ScreenMode_" + screenModes.Window).ToPlainText()
		};
	}

	public static string ScreenModeToName(screenModes mode)
	{
		return ("Option_ScreenMode_" + mode).ToPlainText();
	}

	private void Awake()
	{
		UpdateResolutionAndScreenMode();
		OptionsManager.OnOptionsChanged += OnOptionsChanged;
	}

	private void OnDestroy()
	{
		OptionsManager.OnOptionsChanged -= OnOptionsChanged;
	}

	private void OnOptionsChanged(string key)
	{
		if (key == Key_Resolution || key == Key_ScreenMode)
		{
			UpdateResolutionAndScreenMode();
		}
	}
}
