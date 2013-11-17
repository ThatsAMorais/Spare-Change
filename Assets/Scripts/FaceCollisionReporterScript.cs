using UnityEngine;
using System.Collections;

public class FaceCollisionReporterScript : MonoBehaviour {

	void OnTriggerEnter(Collider c)
	{
		if(LayerMask.NameToLayer("Mat") == c.gameObject.layer)
		{
			SendMessageUpwards("GotValue", name);
		}
	}
}
