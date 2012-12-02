using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UI_mgr : MonoBehaviour {
	
	public GameObject d4;
	public GameObject d6_s_p;
	public GameObject d6_s_d;
	public GameObject d6_r_p;
	public GameObject d6_r_d;
	public GameObject d8;
	public GameObject d10;
	public GameObject d12;
	public GameObject d20;
	public GameObject d100;
	public GameObject cube;
	public float throwForce;
	
	private int dieCount = 0;
	//private bool throw_a_dice = false;
	
	static private IList<string> diceStr = new List<string>();
	
	// Use this for initialization
	void Start () {
		diceStr.Add("d4");
		diceStr.Add("d6_s_p");
		diceStr.Add("d6_s_d");
		diceStr.Add("d6_r_p");
		diceStr.Add("d6_r_d");
		diceStr.Add("d8");
		diceStr.Add("d10");
		diceStr.Add("d12");
		diceStr.Add("d20");
		diceStr.Add("d100");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void ThrowDie(GameObject die)
	{
		float force_x = throwForce; //throwForce * Input.GetAxis("Vertical");
		float force_z = throwForce; //throwForce * Input.GetAxis("Horizontal");
		Debug.Log(string.Format("Force_x({0}) Force_y({1})", force_x, force_z));
		
		die.rigidbody.AddForce(force_x, 0.0f, -1 * force_z);
		die.rigidbody.AddTorque(2.0f, 4.0f, 2.0f);
	}
	
	void CreateDie(string die) {
		
		GameObject newDie;
		//Vector3 camPos = Camera.main.transform.position;
		
		if(die.Equals("d4")){
			newDie = Instantiate(d4) as GameObject;
		}
		else if(die.Equals("d6_s_p")) {
			newDie = Instantiate(d6_s_p) as GameObject;
		}
		else if(die.Equals("d6_s_d")) {
			newDie = Instantiate(d6_s_d) as GameObject;
		}
		else if(die.Equals("d6_r_p")) {
			newDie = Instantiate(d6_r_p) as GameObject;
		}
		else if(die.Equals("d6_r_d")) {
			newDie = Instantiate(d6_r_d) as GameObject;
		}
		else if(die.Equals("d8")) {
			newDie = Instantiate(d8) as GameObject;
		}
		else if(die.Equals("d10")) {
			newDie = Instantiate(d10) as GameObject;
		}
		else if(die.Equals("d12")) {
			newDie = Instantiate(d12) as GameObject;
		}
		else if(die.Equals("d20")) {
			newDie = Instantiate(d20) as GameObject;
		}
		else if(die.Equals("d100")) {
			newDie = Instantiate(d100) as GameObject;
		}
		else {
			newDie = Instantiate(cube) as GameObject;
		}
		
		newDie.name = string.Format("{0}_{1}", newDie.name, dieCount);
		newDie.transform.position = new Vector3(0, 12.0f, 0);
		
		dieCount++;
		
		Debug.Log(newDie.name);		
		ThrowDie(newDie);
	}
	
	Rect GUIButtonRect() {
		return new Rect(Screen.width*0.01f, Screen.height*0.01f, Screen.width*0.98f, Screen.height*0.2f);
	}
	
	void OnGUI() {
		GUILayout.BeginArea(GUIButtonRect());
		GUILayout.BeginHorizontal();
		
		foreach(string die in diceStr)
		{
			if(GUILayout.Button(die))
			{
				CreateDie(die);
			}
		}
		
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
}
