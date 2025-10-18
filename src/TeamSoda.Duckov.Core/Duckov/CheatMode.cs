using System;
using System.IO;
using Saves;

namespace Duckov;

public class CheatMode
{
	private static bool _acitive;

	public static bool Active
	{
		get
		{
			return _acitive;
		}
		private set
		{
			_acitive = value;
			CheatMode.OnCheatModeStatusChanged?.Invoke(value);
		}
	}

	private bool Cheated => SavesSystem.Load<bool>("Cheated");

	public static event Action<bool> OnCheatModeStatusChanged;

	public static void Activate()
	{
		if (CheatFileExists())
		{
			Active = true;
			SavesSystem.Save("Cheated", value: true);
		}
	}

	public static void Deactivate()
	{
		Active = false;
	}

	private static bool CheatFileExists()
	{
		return File.Exists(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WWSSADADBA"));
	}
}
