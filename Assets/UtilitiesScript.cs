using UnityEngine;
using System.Collections;

public class UtilitiesScript : MonoBehaviour {

		
	
	/// <summary>
	/// Gets the dice box.
	/// </summary>
	/// <returns>
	/// The dice box.
	/// </returns>
	GameObject diceBox;
	public Transform getDiceBox()
	{
		if(null == diceBox)
			diceBox = GameObject.Find("DiceBox");
		
		return diceBox.transform;
	}
	
	GameObject diceThrower;
	BattleControllerScript diceThrowerScript;
	public GameObject getDiceThrower()
	{
		if(null == diceThrower)
			diceThrower = GameObject.Find("BattleController");
		
		return diceThrower;
	}
	public BattleControllerScript getDiceThrowerScript()
	{
		if(null == diceThrower)
			getDiceThrower();
		
		if(null == diceThrowerScript)
			diceThrowerScript = diceThrower.GetComponent<BattleControllerScript>();
		
		return diceThrowerScript;
	}
}
