using System.Collections.Generic;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.Events;

public class DuckovItemAgent : ItemAgent
{
	public HandheldSocketTypes handheldSocket = HandheldSocketTypes.normalHandheld;

	public HandheldAnimationType handAnimationType = HandheldAnimationType.normal;

	private CharacterMainControl holder;

	public UnityEvent OnInitializdEvent;

	[SerializeField]
	private List<Transform> socketsList = new List<Transform>();

	public GameObject setActiveIfMainCharacter;

	private Dictionary<string, Transform> _socketsDic;

	private IAgentUsable usableInterface;

	public CharacterMainControl Holder => holder;

	private Dictionary<string, Transform> SocketsDic
	{
		get
		{
			if (_socketsDic == null)
			{
				_socketsDic = new Dictionary<string, Transform>();
				foreach (Transform sockets in socketsList)
				{
					_socketsDic.Add(sockets.name, sockets);
				}
			}
			return _socketsDic;
		}
	}

	public IAgentUsable UsableInterface => usableInterface;

	public Transform GetSocket(string socketName, bool createNew)
	{
		Transform value;
		bool num = SocketsDic.TryGetValue(socketName, out value);
		if (num && value == null)
		{
			SocketsDic.Remove(socketName);
		}
		if (!num && createNew)
		{
			value = new GameObject(socketName).transform;
			value.SetParent(base.transform);
			value.localPosition = Vector3.zero;
			value.localRotation = Quaternion.identity;
			SocketsDic.Add(socketName, value);
		}
		return value;
	}

	public void SetHolder(CharacterMainControl _holder)
	{
		holder = _holder;
		if ((bool)setActiveIfMainCharacter)
		{
			setActiveIfMainCharacter.SetActive(_holder.IsMainCharacter);
		}
	}

	public CharacterMainControl GetHolder()
	{
		return holder;
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		InitInterfaces();
		OnInitializdEvent?.Invoke();
	}

	private void InitInterfaces()
	{
		usableInterface = this as IAgentUsable;
	}
}
