using UnityEngine;

public class InteractMarker : MonoBehaviour
{
	private bool markedAsUsed;

	public GameObject showIfUsedObject;

	public GameObject hideIfUsedObject;

	public void MarkAsUsed()
	{
		if (!markedAsUsed)
		{
			markedAsUsed = true;
			if ((bool)hideIfUsedObject)
			{
				hideIfUsedObject.SetActive(value: false);
			}
			if ((bool)showIfUsedObject)
			{
				showIfUsedObject.SetActive(value: true);
			}
		}
	}
}
