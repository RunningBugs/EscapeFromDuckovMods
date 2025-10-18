namespace Duckov.Tasks;

public interface ITaskBehaviour
{
	void Begin();

	bool IsPending();

	bool IsComplete();

	void Skip()
	{
	}
}
