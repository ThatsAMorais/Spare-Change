﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DiceControllerScript : MonoBehaviour {

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
	public List<Material> DiceMats;

	List<GameObject> dice;
	int dieCount = 0;


	/// <summary>
	/// Roll.
	/// </summary>
	public class Roll
	{
		public string dieName {get;set;}
		public int count {get;set;}

		public Roll(string die, int quantity)
		{
			dieName = die;
			count = quantity;
		}
	}

	public void ClearDice()
	{
		if(null != dice)
		{
			foreach(GameObject die in dice)
			{
				GameObject.Destroy(die);
			}
		}

		dice = new List<GameObject>();

		Utilities().ResetCamera();
	}

	/// <summary>
	/// Throws the dice.
	/// </summary>
	/// <param name='roll'>
	/// Roll.
	/// </param>
	/// <param name='diceBox'>
	/// Dice box.
	/// </param>
	public void ThrowDice(Roll roll)
	{
		//TODO: Many a magic number in this function
		
		Transform diceBox = Utilities().getDiceBox().transform;
		
		ClearDice();
		
		// Calculate the start position from the position of the box
		Vector3 startPosition = new Vector3(diceBox.position.x + diceBox.localScale.x*4,
											diceBox.position.y + 50,
											diceBox.position.z + diceBox.localScale.z*4);
		
		for(int d=0; d < roll.count; d++)
		{
			GameObject die = CreateDie(roll.dieName);
			
			// Position
			die.transform.position = startPosition;
			die.rigidbody.velocity = diceBox.right * 15 + diceBox.forward * -30;
			
			// Orientation
			die.transform.rotation = Quaternion.LookRotation(Random.onUnitSphere);
			die.rigidbody.angularVelocity = Vector3.right * -15;

			dice.Add(die);

			// Position subsequent dice adjacently
			startPosition = startPosition + new Vector3(die.transform.localScale.x + 1, 0, 0);
		}

		Utilities().LookAtDice(dice);
	}


	/// <summary>
	/// Creates the die.
	/// </summary>
	/// <returns>
	/// The die.
	/// </returns>
	/// <param name='dieName'>
	/// Die name.
	/// </param>
	GameObject CreateDie(string dieName)
	{
		if(dieName.Equals("d4")) return InstantiateDie(d4);
		else if(dieName.Equals("d6")) return InstantiateDie(d6_s_d);
		else if(dieName.Equals("d8")) return InstantiateDie(d8);
		else if(dieName.Equals("d10")) return InstantiateDie(d10);
		else if(dieName.Equals("d12")) return InstantiateDie(d12);
		else if(dieName.Equals("d20")) return InstantiateDie(d20);
		else if(dieName.Equals("d100")) return InstantiateDie(d100);
		else return null;
	}
	
	/// <summary>
	/// Instantiates the die.
	/// </summary>
	/// <returns>
	/// The die.
	/// </returns>
	/// <param name='diePrefab'>
	/// Die prefab.
	/// </param>
	GameObject InstantiateDie(GameObject diePrefab)
	{
		GameObject newDie = Instantiate(diePrefab) as GameObject;
		
		newDie.name = string.Format("{0} {1}", newDie.name.Replace("Clone", ""), dieCount);
		
		Material mat1 = DiceMats[Random.Range(0, DiceMats.Count)];
		Material mat2 = DiceMats[Random.Range(0, DiceMats.Count)];
		while (mat1 == mat2) // Don't let them be identical
		{
			mat2 = DiceMats[Random.Range(0, DiceMats.Count)];
		}
		
		// Give it a random texture
		newDie.renderer.materials = new Material[2] {mat1, mat2};
		
		dieCount++;
		
		return newDie;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	UtilitiesScript utilitiesScript;
	UtilitiesScript Utilities()
	{
		if(null == utilitiesScript)
			utilitiesScript = GameObject.Find("Utilities").GetComponent<UtilitiesScript>();

		return utilitiesScript;
	}

}