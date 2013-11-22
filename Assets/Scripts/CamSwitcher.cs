using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CamSwitcher : MonoBehaviour {

	// Private monobehavior members
	int currentCameraIndex;
	List<Camera> characterCameras;
	
	void Start()
	{
		characterCameras = new List<Camera>();
		/// Get the cameras from the scene using Find or as a Parameter
		Camera camera1 = GameObject.Find("Camera1").camera;
		characterCameras.Add(camera1);
		Camera camera2 = GameObject.Find("Camera2").camera;
		characterCameras.Add(camera2);
		Camera camera3 = GameObject.Find("Camera3").camera;
		characterCameras.Add(camera3);
		Camera camera4 = GameObject.Find("Camera4").camera;
		characterCameras.Add(camera4);
	}
	
	
	void Update()
	{
		if(Input.GetKeyUp(KeyCode.Z))
		{
			// Increment the camera index to the next camera in the list
			currentCameraIndex++;
			if(characterCameras.Count == currentCameraIndex)
			{
				currentCameraIndex = 0;
			}
			
			// loop over the camera list, disabling all but the chosen index
			foreach(Camera camera in characterCameras)
			{
				if(camera == characterCameras[currentCameraIndex])
				{
					camera.enabled = true;
				}
				else
				{
					camera.enabled = false;
				}
			}
		}
	}
}
