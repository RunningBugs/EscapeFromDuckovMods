using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Demo : MonoBehaviour
{
	private GameObject[] animals;

	private int animalIndex;

	private List<string> animationList = new List<string>
	{
		"Attack", "Bounce", "Clicked", "Death", "Eat", "Fear", "Fly", "Hit", "Idle_A", "Idle_B",
		"Idle_C", "Jump", "Roll", "Run", "Sit", "Spin/Splash", "Swim", "Walk"
	};

	private List<string> shapekeyList = new List<string>
	{
		"Eyes_Annoyed", "Eyes_Blink", "Eyes_Cry", "Eyes_Dead", "Eyes_Excited", "Eyes_Happy", "Eyes_LookDown", "Eyes_LookIn", "Eyes_LookOut", "Eyes_LookUp",
		"Eyes_Rabid", "Eyes_Sad", "Eyes_Shrink", "Eyes_Sleep", "Eyes_Spin", "Eyes_Squint", "Eyes_Trauma", "Sweat_L", "Sweat_R", "Teardrop_L",
		"Teardrop_R"
	};

	[Space(10f)]
	private Transform animal_parent;

	private Dropdown dropdownAnimal;

	private Dropdown dropdownAnimation;

	private Dropdown dropdownShapekey;

	private void Start()
	{
		animal_parent = GameObject.Find("Animals").transform;
		Transform transform = GameObject.Find("Canvas").transform;
		dropdownAnimal = transform.Find("Animal").Find("Dropdown").GetComponent<Dropdown>();
		dropdownAnimation = transform.Find("Animation").Find("Dropdown").GetComponent<Dropdown>();
		dropdownShapekey = transform.Find("Shapekey").Find("Dropdown").GetComponent<Dropdown>();
		int childCount = animal_parent.childCount;
		animals = new GameObject[childCount];
		List<string> list = new List<string>();
		for (int i = 0; i < childCount; i++)
		{
			animals[i] = animal_parent.GetChild(i).gameObject;
			string item = animal_parent.GetChild(i).name;
			list.Add(item);
			if (i == 0)
			{
				animals[i].SetActive(value: true);
			}
			else
			{
				animals[i].SetActive(value: false);
			}
		}
		dropdownAnimal.AddOptions(list);
		dropdownAnimation.AddOptions(animationList);
		dropdownShapekey.AddOptions(shapekeyList);
		dropdownShapekey.value = 1;
		ChangeShapekey();
	}

	private void Update()
	{
		if (Input.GetKeyDown("up"))
		{
			PrevAnimal();
		}
		else if (Input.GetKeyDown("down"))
		{
			NextAnimal();
		}
		else if (Input.GetKeyDown("right") && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
		{
			NextShapekey();
		}
		else if (Input.GetKeyDown("left") && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
		{
			PrevShapekey();
		}
		else if (Input.GetKeyDown("right"))
		{
			NextAnimation();
		}
		else if (Input.GetKeyDown("left"))
		{
			PrevAnimation();
		}
	}

	public void NextAnimal()
	{
		if (dropdownAnimal.value >= dropdownAnimal.options.Count - 1)
		{
			dropdownAnimal.value = 0;
		}
		else
		{
			dropdownAnimal.value++;
		}
		ChangeAnimal();
	}

	public void PrevAnimal()
	{
		if (dropdownAnimal.value <= 0)
		{
			dropdownAnimal.value = dropdownAnimal.options.Count - 1;
		}
		else
		{
			dropdownAnimal.value--;
		}
		ChangeAnimal();
	}

	public void ChangeAnimal()
	{
		animals[animalIndex].SetActive(value: false);
		animals[dropdownAnimal.value].SetActive(value: true);
		animalIndex = dropdownAnimal.value;
		ChangeAnimation();
		ChangeShapekey();
	}

	public void NextAnimation()
	{
		if (dropdownAnimation.value >= dropdownAnimation.options.Count - 1)
		{
			dropdownAnimation.value = 0;
		}
		else
		{
			dropdownAnimation.value++;
		}
		ChangeAnimation();
	}

	public void PrevAnimation()
	{
		if (dropdownAnimation.value <= 0)
		{
			dropdownAnimation.value = dropdownAnimation.options.Count - 1;
		}
		else
		{
			dropdownAnimation.value--;
		}
		ChangeAnimation();
	}

	public void ChangeAnimation()
	{
		Animator component = animals[dropdownAnimal.value].GetComponent<Animator>();
		if (!(component != null))
		{
			return;
		}
		int value = dropdownAnimation.value;
		if (value == 15)
		{
			if (component.HasState(0, Animator.StringToHash("Spin")))
			{
				component.Play("Spin");
			}
			else if (component.HasState(0, Animator.StringToHash("Splash")))
			{
				component.Play("Splash");
			}
		}
		else
		{
			component.Play(dropdownAnimation.options[value].text);
		}
	}

	public void NextShapekey()
	{
		if (dropdownShapekey.value >= dropdownShapekey.options.Count - 1)
		{
			dropdownShapekey.value = 0;
		}
		else
		{
			dropdownShapekey.value++;
		}
		ChangeShapekey();
	}

	public void PrevShapekey()
	{
		if (dropdownShapekey.value <= 0)
		{
			dropdownShapekey.value = dropdownShapekey.options.Count - 1;
		}
		else
		{
			dropdownShapekey.value--;
		}
		ChangeShapekey();
	}

	public void ChangeShapekey()
	{
		Animator component = animals[dropdownAnimal.value].GetComponent<Animator>();
		if (component != null)
		{
			component.Play(dropdownShapekey.options[dropdownShapekey.value].text);
		}
	}

	public void GoToWebsite(string URL)
	{
		Application.OpenURL(URL);
	}
}
