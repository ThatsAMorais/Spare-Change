using UnityEngine;
using System.Collections;

public class CamControl : MonoBehaviour {
	
	public int camHeight = 12;
	public Vector3 camPos;
	public int posMax = 10;
	public int posMin = -10;
	public int htMax = 12;
	public int htMin = 4;
	
	Transform lookTarget;
	float lookWait = 0.0f;
	
	// Use this for initialization
	void Start () {
		camPos = gameObject.transform.localPosition;
	}
	
	// Update is called once per frame
	void Update () {
		// Camera scrolling
		camPos.y = Mathf.Clamp(camPos.y + Input.GetAxis("Mouse ScrollWheel"), htMin, htMax);
		//gameObject.Find("Point light").light.intensity = camPos.y*0.2;
		
		if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			/* Camera movement */
	    	camPos.x = Mathf.Clamp(camPos.x + Input.GetAxis("Mouse Y"), posMin, posMax);
	    	camPos.z = Mathf.Clamp(camPos.z + Input.GetAxis("Mouse X"), posMin, posMax);
		}
		
		/*
		if(!selectedGUITexture.HitTest(Input.mousePosition, Camera.main))
		{
			DoMousePicking();
		}
		*/
		
		GameObject.Find("Main Camera").transform.position = camPos;
		
		
		if(null != lookTarget)
		{
			lookWait += Time.deltaTime;
			if(lookWait > 3.0f)
			{
				
				
		        /*camera.transform.position = new Vector3((lookTarget.position.x + 100),
														(lookTarget.position.y + 100),
														(lookTarget.position.z));*/
				
				transform.LookAt(lookTarget.position);
			}
		}
	}
	
	public void LookAtDice(Transform die)
	{
		lookTarget = die;
	}
}
