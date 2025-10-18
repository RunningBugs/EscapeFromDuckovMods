using UnityEngine;
using UnityEngine.UI;

namespace UI_Spline_Renderer.Example;

public class UISplineRendererExample : MonoBehaviour
{
	public UISplineRenderer target_uvAnimation;

	public UISplineRenderer target_interaction;

	private void Start()
	{
		target_interaction.GetComponent<Button>().onClick.AddListener(delegate
		{
			Debug.Log("Spline Clicked !");
		});
	}

	private void Update()
	{
		UpdateUV();
	}

	private void UpdateUV()
	{
		target_uvAnimation.uvOffset += new Vector2(0f, Time.deltaTime * 2f);
		target_uvAnimation.clipRange = new Vector2(0f, (Mathf.Sin(Time.time) + 1f) * 0.5f);
	}
}
