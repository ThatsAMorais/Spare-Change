using UnityEngine;
using System.Collections;

public class BattleGUITextScript : MonoBehaviour {
	
	public float scroll = 0.05f;  // scrolling velocity
	public float duration = 1.5f; // time to die
	public float alpha = 1;
	public int direction = 1;
	
	// Use this for initialization
	void Awake ()
	{
		//
		alpha = 1;
	}
	
	// Update is called once per frame
	void Update ()
	{
	    if(alpha>0)
		{
			transform.position = new Vector3(transform.position.x + scroll*Time.deltaTime*direction*0.5f,
											transform.position.y + scroll*Time.deltaTime,
											transform.position.z + scroll*Time.deltaTime*0.5f);
			alpha -= Time.deltaTime/duration;
			guiText.material.color = new Color(guiText.material.color.r,
												guiText.material.color.g,
												guiText.material.color.b,
												alpha);
	    }
		else
		{
	       GameObject.Destroy(gameObject); // text vanished - destroy itself
	    }
	}
	
	public void SetColor(Color newColor)
	{
		direction = (1 == Random.Range(0,2) ? -1 : 1);
		guiText.material.color = newColor; // set text color
	}
}
