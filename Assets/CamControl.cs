using UnityEngine;
using System.Collections;

public class CamControl : MonoBehaviour {
	
	public int camHeight = 12;
	public Vector3 camPos;
	public int posMax = 10;
	public int posMin = -10;
	public int htMax = 12;
	public int htMin = 4;
	
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
	    	camPos.x = Mathf.Clamp(camPos.x + Input.GetAxis("Mouse X"), posMin, posMax);
	    	camPos.z = Mathf.Clamp(camPos.z + Input.GetAxis("Mouse Y"), posMin, posMax);
		}
		
		/*
		if(!selectedGUITexture.HitTest(Input.mousePosition, Camera.main))
		{
			DoMousePicking();
		}
		*/
		
		GameObject.Find("Main Camera").transform.position = camPos;
	}
}
