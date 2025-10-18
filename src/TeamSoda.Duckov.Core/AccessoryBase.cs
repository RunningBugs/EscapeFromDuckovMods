using ItemStatsSystem;
using UnityEngine;

public class AccessoryBase : MonoBehaviour
{
	public string socketName;

	protected Item selfItem;

	protected DuckovItemAgent parentAgent;

	public void Init(DuckovItemAgent _parentAgent, Item _selfItem)
	{
		if (_parentAgent == null || _selfItem == null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		parentAgent = _parentAgent;
		selfItem = _selfItem;
		Transform socket = parentAgent.GetSocket(socketName, createNew: true);
		base.transform.SetParent(socket);
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.identity;
		OnInit();
	}

	protected virtual void OnInit()
	{
	}
}
