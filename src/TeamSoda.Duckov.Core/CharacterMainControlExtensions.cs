public static class CharacterMainControlExtensions
{
	public static bool IsMainCharacter(this CharacterMainControl character)
	{
		if (character == null)
		{
			return false;
		}
		return LevelManager.Instance?.MainCharacter == character;
	}
}
