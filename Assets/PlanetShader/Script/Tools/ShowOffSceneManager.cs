using UnityEngine;
using System.Collections;

public class ShowOffSceneManager : MonoBehaviour {
	
	public float		planetRotSpeed = 1.0f;
	public float		sunRotSpeed = 1.0f;
	public Transform	sun;
	public GameObject[]	planets = new GameObject[9];

	private int			index = 0;
	private GameObject 	planet;

	void Start ()
	{
		// spawn the first planet
		planet = GameObject.Instantiate(planets[0], Vector3.zero, planets[0].transform.rotation) as GameObject;
	}

	void Update ()
	{
		planet.transform.Rotate(new Vector3(0, planetRotSpeed * Time.deltaTime, 0));
		sun.Rotate(new Vector3(0,sunRotSpeed * Time.deltaTime, 0));
	}

	public void NextButton()
	{
		index += 1;
		if (index >= planets.Length)
			index = 0;
		Destroy(planet);
		planet = GameObject.Instantiate(planets[index], Vector3.zero, planets[index].transform.rotation) as GameObject;
		planet.transform.localScale = new Vector3(1,1,1);

	}

	public void PreviousButton()
	{
		index -= 1;
		if (index < 0)
			index = planets.Length - 1;
		Destroy(planet);
		planet = GameObject.Instantiate(planets[index], Vector3.zero, planets[index].transform.rotation) as GameObject;
		planet.transform.localScale = new Vector3(1,1,1);
	}
}
