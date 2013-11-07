using UnityEngine;
using System.Collections;

public class FaceCollisionReporterScript : MonoBehaviour {

	void OnTriggerEnter(Collider c)
	{
		if(LayerMask.NameToLayer("Mat") == c.gameObject.layer)
		{
			Debug.Log(string.Format("Face Collision - {0}, {1}", name, c.gameObject.name));
			SendMessageUpwards("GotValueCollision", name);
		}
		
	}

	
}
