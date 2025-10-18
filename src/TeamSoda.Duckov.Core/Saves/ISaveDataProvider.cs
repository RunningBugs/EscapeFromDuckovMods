namespace Saves;

public interface ISaveDataProvider
{
	object GenerateSaveData();

	void SetupSaveData(object data);
}
