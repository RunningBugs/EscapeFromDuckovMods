using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PackedMapData : ScriptableObject, IMiniMapDataProvider
{
	[Serializable]
	public class Entry : IMiniMapEntry
	{
		[SerializeField]
		private Sprite sprite;

		[SerializeField]
		private float pixelSize;

		[SerializeField]
		private Vector2 offset;

		[SerializeField]
		private string sceneID;

		[SerializeField]
		private bool hide;

		[SerializeField]
		private bool noSignal;

		public Sprite Sprite => sprite;

		public float PixelSize => pixelSize;

		public Vector2 Offset => offset;

		public string SceneID => sceneID;

		public bool Hide => hide;

		public bool NoSignal => noSignal;

		public Entry()
		{
		}

		public Entry(Sprite sprite, float pixelSize, Vector2 offset, string sceneID, bool hide, bool noSignal)
		{
			this.sprite = sprite;
			this.pixelSize = pixelSize;
			this.offset = offset;
			this.sceneID = sceneID;
			this.hide = hide;
			this.noSignal = noSignal;
		}
	}

	[SerializeField]
	private Sprite combinedSprite;

	[SerializeField]
	private float pixelSize;

	[SerializeField]
	private Vector3 combinedCenter;

	[SerializeField]
	private List<Entry> maps = new List<Entry>();

	public Sprite CombinedSprite => combinedSprite;

	public float PixelSize => pixelSize;

	public Vector3 CombinedCenter => combinedCenter;

	public List<IMiniMapEntry> Maps => ((IEnumerable<IMiniMapEntry>)maps).ToList();

	internal void Setup(IMiniMapDataProvider origin)
	{
		combinedSprite = origin.CombinedSprite;
		pixelSize = origin.PixelSize;
		combinedCenter = origin.CombinedCenter;
		maps.Clear();
		foreach (IMiniMapEntry map in origin.Maps)
		{
			Entry item = new Entry(map.Sprite, map.PixelSize, map.Offset, map.SceneID, map.Hide, map.NoSignal);
			maps.Add(item);
		}
	}
}
