﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RollValueDeciderScript : MonoBehaviour {
		
	public int currentValue; //This is public for viewing from the editor

	Dictionary<string,int> faceValues;
	GameObject diceThrower;
	bool bReported;

	/// <summary>
	/// Gots the value collision.
	/// </summary>
	/// <param name='faceName'>
	/// Face name.
	/// </param>
	public void GotValue(string faceName)
	{
		faceValues.TryGetValue(faceName, out currentValue);
	}
	
	void Update ()
	{
		if(false == bReported)
		{
			if(rigidbody.velocity.magnitude < 0.001 && rigidbody.angularVelocity.magnitude < 0.001)
			{
				bReported = true;

				Utilities().DiceRolled(gameObject, currentValue);
			}
		}
	}

	void Awake()
	{
		bReported = false;
	}

	void OnCollisionEnter(Collision c)
	{
		if(LayerMask.NameToLayer("Mat") == c.gameObject.layer && !audio.isPlaying)
		{
			int randomSound = Random.Range(0,3);
			AudioSource audioSource = GetComponents<AudioSource>()[randomSound];
			audioSource.PlayOneShot(audioSource.clip);
		}
	}

	void OnEnable ()
	{
		bReported = false;
		currentValue = 0;
		
		// Do other initializations above this stuff
		faceValues = new Dictionary<string, int>();
		
		if(name.Contains("d100"))
		{
			faceValues.Add("10", 10);
			faceValues.Add("20", 20);
			faceValues.Add("30", 30);
			faceValues.Add("40", 40);
			faceValues.Add("50", 50);
			faceValues.Add("60", 60);
			faceValues.Add("70", 70);
			faceValues.Add("80", 80);
			faceValues.Add("90", 90);
			faceValues.Add("00", 100);
			return;
		}
		
		// Start updward count of faces
		faceValues.Add("1", 1);
		faceValues.Add("2", 2);
		faceValues.Add("3", 3);
		faceValues.Add("4", 4);
		
		// Break here for the d4
		if(name.Contains("d4")) return;
		
		faceValues.Add("5", 5);
		faceValues.Add("6", 6);
		
		// Break here for the d6
		if(name.Contains("d6")) return;
			
		faceValues.Add("7", 7);
		faceValues.Add("8", 8);
		
		// Break here for the d8
		if(name.Contains("d8")) return;
		
		faceValues.Add("9", 9);
		faceValues.Add("10", 10);
		
		// Break here for the d10
		if(name.Contains("d10")) return;
		
		faceValues.Add("11", 11);
		faceValues.Add("12", 12);
		
		// Break here for the d12
		if(name.Contains("d12")) return;
		
		// The rest is for d20
		faceValues.Add("13", 13);
		faceValues.Add("14", 14);
		faceValues.Add("15", 15);
		faceValues.Add("16", 16);
		faceValues.Add("17", 17);
		faceValues.Add("18", 18);
		faceValues.Add("19", 19);
		faceValues.Add("20", 20);
		
		return;
	}
	
	UtilitiesScript utilitiesScript;
	UtilitiesScript Utilities()
	{
		if(null == utilitiesScript)
			utilitiesScript = GameObject.Find("Utilities").GetComponent<UtilitiesScript>();
		
		return utilitiesScript;
	}
}
