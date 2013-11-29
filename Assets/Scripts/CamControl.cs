using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CamControl : MonoBehaviour {
	
	public int camHeight = 12;
	public Vector3 cameraPosition;
	public Vector3 initialPosition;
	public static int posMax = 10;
	public static int posMin = -10;
	public static int htMax = 12;
	public static int htMin = 4;
	
	bool bHasLookTarget = false;
	float lookWait = 0.0f;
	Dictionary<GameObject,Transform> camerasAndTargets;
	Dictionary<int,List<Rect>> cameraRects;
	
	// Use this for initialization
	void OnEnable () {
		cameraPosition = gameObject.transform.localPosition;
		initialPosition = cameraPosition;
		
		cameraRects = new Dictionary<int, List<Rect>>();
		// One-die rects list
		List<Rect> oneDie = new List<Rect>();
		oneDie.Add(new Rect(0, 0, 1, 1));
		cameraRects.Add(1,oneDie);
		// Two-die rects list
		List<Rect> twoDice = new List<Rect>();
		twoDice.Add(new Rect(   0, 0, 0.5f, 1));
		twoDice.Add(new Rect(0.5f, 0,    1, 1));
		cameraRects.Add(2,twoDice);
		// Three-die rects list
		List<Rect> threeDice = new List<Rect>();
		threeDice.Add(new Rect(   0, 0.5f,    1,    1));
		threeDice.Add(new Rect(   0,    0, 0.5f, 0.5f));
		threeDice.Add(new Rect(0.5f,    0,    1, 0.5f));
		cameraRects.Add(3,threeDice);
		// Four-die rects list
		List<Rect> fourDice = new List<Rect>();
		fourDice.Add(new Rect(   0, 0.5f, 0.5f,    1));
		fourDice.Add(new Rect(0.5f, 0.5f,    1,    1));
		fourDice.Add(new Rect(   0,    0, 0.5f, 0.5f));
		fourDice.Add(new Rect(0.5f,    0,    1, 0.5f));
		cameraRects.Add(4,fourDice);
	}
	
	// Update is called once per frame
	void Update ()
	{
		/*
		// Camera scrolling
		cameraPosition.y = Mathf.Clamp(cameraPosition.y + Input.GetAxis("Mouse ScrollWheel"), htMin, htMax);
		//gameObject.Find("Point light").light.intensity = camPos.y*0.2;
	
		if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			//Camera Movement
	    	cameraPosition.x = Mathf.Clamp(cameraPosition.x + Input.GetAxis("Mouse Y"), posMin, posMax);
	    	cameraPosition.z = Mathf.Clamp(cameraPosition.z + Input.GetAxis("Mouse X"), posMin, posMax);
		}
		
		Camera.main.transform.position = cameraPosition;
		*/
		
		if(true == bHasLookTarget && null != camerasAndTargets)
		{
			if(lookWait < 1.5f)
			{
				lookWait += Time.deltaTime;
			}
			else
			{
				foreach(GameObject cam in camerasAndTargets.Keys)
				{
					cam.transform.position = Vector3.MoveTowards(cam.transform.position,
																 new Vector3(
																		(camerasAndTargets[cam].position.x + 20),
																		(camerasAndTargets[cam].position.y + 30),
																		(camerasAndTargets[cam].position.z)),
																 25*Time.deltaTime);
					
					cam.transform.LookAt(camerasAndTargets[cam].position);
				}
			}
		}
		else
		{
			bHasLookTarget = false;
			Vector3 diceBoxPosition = Utilities().getDiceBox().transform.position;
			transform.position = Vector3.MoveTowards(transform.position,
													 new Vector3(diceBoxPosition.x,
																 diceBoxPosition.y + 500,
																 diceBoxPosition.z),
													 45*Time.deltaTime);
		}


	}
	
	UtilitiesScript utilitiesScript;
	UtilitiesScript Utilities()
	{
		if(null == utilitiesScript)
			utilitiesScript = GameObject.Find("Utilities").GetComponent<UtilitiesScript>();
		
		return utilitiesScript;
	}
	
	public void LookAtDice(List<GameObject> dice)
	{
		int camCount = 0;
		
		ResetCamera();
		
		foreach(GameObject die in dice)
		{
			GameObject newCam;
			
			if(0 == camCount)
			{
				newCam = gameObject;
				newCam.name = string.Format("Main Dice Cam {0}", die.name);
			}
			else
			{
				newCam = new GameObject();
				newCam.name = string.Format("Secondary Dice Cam {0}", die.name);
				newCam.AddComponent<Camera>();
			}
			
			newCam.camera.rect = cameraRects[dice.Count][camCount];
			camerasAndTargets.Add(newCam, die.transform);
			camCount++;
		}
		
		bHasLookTarget = true;
	}
	
	public void ResetCamera()
	{
		Vector3 diceBoxPosition = Utilities().getDiceBox().transform.position;
		
		lookWait = 0;
		bHasLookTarget = false;
		
		if(null != camerasAndTargets)
		{
			foreach(GameObject cam in camerasAndTargets.Keys)
			{
				if(cam.transform.name.Contains("Main"))
				{
					continue;
				}
				GameObject.Destroy(cam);
			}
		}
		
		camera.rect = cameraRects[1][0];
		camera.transform.name = "Main Camera";
		camera.transform.position = new Vector3(diceBoxPosition.x,
												 diceBoxPosition.y + 300,
												 diceBoxPosition.z);
		camerasAndTargets = new Dictionary<GameObject, Transform>();
	}
}
