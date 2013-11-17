using UnityEngine;
using System.Collections;

public class DiceCatcherScript : MonoBehaviour {

	void OnCollision(Collision collision)
	{
		GameObject.Destroy(collider.gameObject);
	}
}
