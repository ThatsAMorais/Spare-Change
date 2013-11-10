using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UI_mgr : MonoBehaviour {
	
	static private List<string> diceNames = new List<string>();
	
	public Transform createSpot;
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
	public List<Material> DiceMats;
	
	private int dieCount = 0;
	//private bool throw_a_dice = false;	
	
	private Vector3 clickStartPosition;
	
	public class Throw
	{
		public string description {get;set;}
		public List<string> dice {get;set;}
		
		public Throw()
		{
			dice = new List<string>();
		}
		
		public void Add(string newDice)
		{
			dice.Add(newDice);
		}
	}
	
	private class Throws
	{
		List<Throw> throwQueue = new List<Throw>();
		
		// This function pops the top element, removing it
		public Throw GetNext()
		{
			Throw retrievedThrow = null;
				
			if(throwQueue.Count > 0)
			{
				retrievedThrow = throwQueue[0];
				throwQueue.Remove(retrievedThrow);
			}
			return retrievedThrow;
		}
		
		public void Add(Throw newThrow)
		{
			throwQueue.Add(newThrow);
		}
	}
	
	Throws throws = new Throws();
	
	private bool bReadyToThrow = false;
	private bool bStartTargetSet = false;
	private Throw lastThrow;
	
	// Use this for initialization
	void Awake () {
		diceNames.Add("d4");
		diceNames.Add("d6_s_p");
		diceNames.Add("d6_s_d");
		/*These are impossible to work with due to their poly count*/
		/*diceStr.Add("d6_r_p");
		diceStr.Add("d6_r_d");*/
		diceNames.Add("d8");
		diceNames.Add("d10");
		diceNames.Add("d12");
		diceNames.Add("d20");
		diceNames.Add("d100");
	}
	
	// Update is called once per frame
	void Update () {
	
		/*
		// Detect left-mouse-click down
		// Determine its screen position
		if(bReadyToThrow && Input.GetMouseButtonDown(0))
		{
			Debug.Log(string.Format("Setting initial position"));
			bReadyToThrow = false;
			clickStartPosition = Input.mousePosition;
			bStartTargetSet = true;
		}
		else if(bStartTargetSet && Input.GetMouseButtonUp(0))
		{
			Debug.Log(string.Format("Throwing the dice"));
			//ThrowDice(throws.GetNext(), Camera.main.transform.position, GameObject.Find("Mat").transform.position);
			bStartTargetSet = false;
		}
		*/
	}
	
	void QueueDieThrow(string diceType, int quantity, string description)
	{
		Debug.Log(string.Format("Queueing a dice throw"));
		Throw newThrow = new Throw();
		newThrow.description = description;
		for(int i=0; i<quantity; i++)
		{
			newThrow.Add(diceType);
		}
		throws.Add(newThrow);
		bReadyToThrow = true;
	}
	
	void ThrowDice(Throw dieThrow, Vector3 startPosition, Vector3 endPosition)
	{
		if(null == dieThrow)
			if(null == lastThrow) return;
			else dieThrow = lastThrow;
		else lastThrow = dieThrow;
		
		foreach(string dieType in dieThrow.dice) 
		{
			GameObject die = CreateDie(dieType);

//			rigidbody.angularVelocity = Random.insideUnitSphere * randomSpin;
//			rigidbody.velocity = Random.insideUnitSphere * randomVelocity;
			transform.rotation = Quaternion.LookRotation(Random.onUnitSphere);
			die.transform.position = startPosition;
			die.transform.LookAt(endPosition);
			die.rigidbody.velocity = Camera.main.transform.forward * -10 + Camera.main.transform.right * 5;
			die.rigidbody.angularVelocity = Vector3.right * 10;
			
			startPosition = startPosition + new Vector3(die.transform.localScale.x + 1, 0, 0);
			
			//Camera.main.GetComponent<CamControl>().LookAtDice(die.transform);
		}
	}
	
	GameObject CreateDie(string dieName) {
		
		GameObject newDie;
		
		if(dieName.Equals("d4")){
			newDie = Instantiate(d4) as GameObject;
		}
		else if(dieName.Equals("d6_s_p")) {
			newDie = Instantiate(d6_s_p) as GameObject;
		}
		else if(dieName.Equals("d6_s_d")) {
			newDie = Instantiate(d6_s_d) as GameObject;
		}
		else if(dieName.Equals("d6_r_p")) {
			newDie = Instantiate(d6_r_p) as GameObject;
		}
		else if(dieName.Equals("d6_r_d")) {
			newDie = Instantiate(d6_r_d) as GameObject;
		}
		else if(dieName.Equals("d8")) {
			newDie = Instantiate(d8) as GameObject;
		}
		else if(dieName.Equals("d10")) {
			newDie = Instantiate(d10) as GameObject;
		}
		else if(dieName.Equals("d12")) {
			newDie = Instantiate(d12) as GameObject;
		}
		else if(dieName.Equals("d20")) {
			newDie = Instantiate(d20) as GameObject;
		}
		else if(dieName.Equals("d100")) {
			newDie = Instantiate(d100) as GameObject;
		}
		else {
			newDie = Instantiate(cube) as GameObject;
		}
		
		newDie.name = string.Format("{0}_{1}", newDie.name, dieCount);
		
		// Give them a random texture
		newDie.renderer.materials = new Material[2] {DiceMats[Random.Range(0, DiceMats.Count)], DiceMats[Random.Range(0, DiceMats.Count)]};
		
		dieCount++;
		
		return newDie;
	}
	
	Rect GUIButtonRect() {
		return new Rect(Screen.width*0.01f, Screen.height*0.01f, Screen.width*0.98f, Screen.height*0.4f);
	}
	
	private int diceQuantity = 1;
	private static int DICE_QUANTITY_MAX = 10;
	private static int DICE_QUANTITY_MIN = 1;
	
	
	void OnGUI() {
		GUILayout.BeginArea(GUIButtonRect());
		
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal();
		foreach(string die in diceNames)
		{
			if(GUILayout.Button(die))
			{
				QueueDieThrow(die, diceQuantity, "Test");
			}
		}
		GUILayout.EndHorizontal();
		
		/*
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("+"))
		{
			diceQuantity = Mathf.Min(DICE_QUANTITY_MAX, ++diceQuantity);
		}
		GUILayout.BeginVertical();
		GUILayout.TextField(string.Format("{0}", diceQuantity));
		GUILayout.TextField("Quantity");
		GUILayout.EndVertical();
		if(GUILayout.Button("-"))
		{
			diceQuantity = Mathf.Max(DICE_QUANTITY_MIN, --diceQuantity);
		}
		GUILayout.EndHorizontal();
		*/
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.TextField(string.Format("Dice Created: {0}", dieCount));
		if(GUILayout.Button("Throw Dice"))
		{
			if(null != throws)
				ThrowDice(throws.GetNext(), Camera.main.transform.position, GameObject.Find("DiceBox").transform.position);
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
}
