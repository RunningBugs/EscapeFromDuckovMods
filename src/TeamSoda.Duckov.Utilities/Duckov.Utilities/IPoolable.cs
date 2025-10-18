namespace Duckov.Utilities;

public interface IPoolable
{
	void NotifyPooled();

	void NotifyReleased();
}
