using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class BattleControllerScript : MonoBehaviour {
	
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
	
	/// <summary>
	/// Weapon.
	/// </summary>
	public class Weapon
	{
		public enum Type
		{
			Melee,
			Ranged,
			Magical,
		}
		
		public string name {get;set;}
		public Roll damage {get;set;}
		public Type type {get;set;}
		public Dictionary<string,Attack> attacks {get;set;}
		
		public Weapon(string theName, Type theType, Roll theDamage)
		{
			name = theName;
			type = theType;
			damage = theDamage;
			attacks = new Dictionary<string, Attack>();
		}
		
		public void AddAttack(Attack attack)
		{
			attacks.Add(attack.name,attack);
		}
	}
	
	/// <summary>
	/// Action.
	/// </summary>
	public class BattleAction
	{
		public enum Type 
		{
			Attack,
			AttackAll,
			All,
			Self,
			Nothing,
		}
		
		public string name {get;set;}
		public Roll roll {get;set;}
		public Type type {get;set;}
		
		public BattleAction(Type theType, string theName, Roll theRoll)
		{
			type = theType;
			name = theName;
			roll = theRoll;
		}
	}
	
	/// <summary>
	/// Attack.
	/// </summary>
	public class Attack : BattleAction
	{			
		public Weapon weapon {get;set;}
		public int damageModifier {get;set;}
		public int hitModifier {get;set;}
		
		public Attack(string theName, Weapon theWeapon, int dmgMod=0, int hitMod=0, BattleAction.Type actionType=BattleAction.Type.Attack)
			: base((BattleAction.Type.AttackAll == actionType ? actionType : BattleAction.Type.Attack), theName, theWeapon.damage)
		{
			weapon = theWeapon;
			damageModifier = dmgMod;
			hitModifier = hitMod;
		}
	}
	
	/// <summary>
	/// Battle actor.
	/// </summary>
	public class BattleActor
	{
		public enum Type 
		{
			Enemy,
			Character,
		}
		
		public string name {get;set;}
		public int health {get;set;}
		public int speed {get;set;}
		public int defense {get;set;}
		public Weapon weapon {get;set;}
		public Dictionary<string,BattleAction> actions
		{
			get // Return all actions, including the weapon actions
			{
				Dictionary<string,BattleAction> allActions = new Dictionary<string,BattleAction>();
				
				foreach(BattleAction action in actionList)
				{
					allActions.Add(action.name, action);
				}
				foreach(Attack attack in weapon.attacks.Values)
				{
					allActions.Add(attack.name, attack);
				}
				
				return allActions;
			}
		}
		private List<BattleAction> actionList;
		
		public BattleActor(string theName, int theHealth, int theSpeed, int theDefense, Weapon theWeapon)
		{
			name = theName;
			health = theHealth;
			defense = theDefense;
			weapon = theWeapon;
			
			actionList = new List<BattleAction>();
		}
		
		public void AddAction(BattleAction action)
		{
			actionList.Add(action);
		}
	}
	
	/// <summary>
	/// Character.
	/// </summary>
	public class Character : BattleActor
	{
		public enum Type 
		{
			Shooter,
			Fighter,
			Conjurer,
			//TODO: More classes for variety
		}
		
		/// <summary>
		/// Inventory.
		/// </summary>
		public class Inventory
		{
			public double change {get;set;}
			public double xp {get;set;}
			public List<Weapon> weapons {get;set;}
			
			public Inventory()
			{
				weapons = new List<Weapon>();
				change = 0;
				xp = 0;
			}
			
			public void AddWeapon(Weapon newWeapon)
			{
				weapons.Add(newWeapon);
			}
			
			public void RemoveWeapon(Weapon weaponToRemove)
			{
				if(false == weapons.Remove(weaponToRemove))
				{
					Debug.Log(string.Format("Weapon, {0}, was not removed", weaponToRemove.name));
				}
			}
		}
		
		public Inventory inventory;
		
		public Character(string theName, int theHealth, int theSpeed, int theDefense, Weapon theWeapon)
		: base(theName, theHealth, theSpeed, theDefense, theWeapon)
		{
			inventory = new Inventory();
			inventory.AddWeapon(theWeapon);
		}
		
		public void AddChange(int amountOfChange)
		{
			inventory.change += amountOfChange;
		}
		
		public void AddExperience(int amountOfExp)
		{
			inventory.xp += amountOfExp;
		}
		
		public void AddWeapon(Weapon theWeapon)
		{
			// Set this as the current weapon
			weapon = theWeapon;
			
			// Store the weapon with the others
			inventory.AddWeapon(theWeapon);
		}
		
		public void DropWeapon(Weapon theWeapon)
		{
			inventory.RemoveWeapon(theWeapon);
		}
	}
	
	/// <summary>
	/// Game state.
	/// </summary>
	public enum GameState 
	{
		Title,
		CharacterSelection,
		BattleStart,
		BattleMode,
		BattleOver
	}
	
	/// <summary>
	/// Battle round.
	/// </summary>
	class BattleRound
	{
		public enum State
		{
			SelectAction,
			SelectTarget,
			ToHitRolling,
			DamageRolling,
			Finished
		}
		
		public State state {get;set;}
		public BattleActor actor {get;set;}
		
		public BattleRound(BattleActor turnActor)
		{
			actor = turnActor;
			state = State.SelectAction;
		}
	}
	
	// Public
	public GUISkin guiSkin;
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
	
	// Static
	private static int GUI_SCREEN_BORDER = 15;
	
	// Private
	List<BattleActor> enemies;
	Character playerCharacter;
	Dictionary<string,Weapon> weapons;
	GameState gameState;
	Queue<BattleRound> queue;
	BattleRound currentTurn;
	int roundCount;
	Dictionary<string,int> battleActorTurnCounts;
	int dieCount = 0;
	
	BattleAction selectedAction;
	BattleActor selectedActor;
	
	void Awake ()
	{
		gameState = GameState.Title;  // TODO: This is where the game should eventually start
		
		// pre-create some weapons
		weapons = new Dictionary<string, Weapon>();
		
		Weapon bat = new Weapon("bat", Weapon.Type.Melee, new Roll("d6",1));
		bat.AddAttack(new Attack("Swing", bat));
		weapons.Add(bat.name, bat);
		
		Weapon knife = new Weapon("knife", Weapon.Type.Melee, new Roll("d4",1));
		knife.AddAttack(new Attack("Swing", knife));
		knife.AddAttack(new Attack("Stab", knife));
		weapons.Add(knife.name, knife);
				
		Weapon nunchucks = new Weapon("nunchucks", Weapon.Type.Melee, new Roll("d4", 2));
		nunchucks.AddAttack(new Attack("Swing", nunchucks, 0, 1));
		nunchucks.AddAttack(new Attack("Strike", nunchucks, 1, 0));
		weapons.Add(nunchucks.name, nunchucks);
		
		Weapon pistol = new Weapon("pistol", Weapon.Type.Ranged, new Roll("d8",1));
		pistol.AddAttack(new Attack("Fire", pistol));
		pistol.AddAttack(new Attack("Sideways Cocked", pistol, 1, -1));
		weapons.Add(pistol.name, pistol);
		/////////////
	}
	
	void Update()
	{

	}
	
	void OnGUI()
	{
		GUI.skin = guiSkin;
		GUILayout.BeginArea(new Rect(0 + GUI_SCREEN_BORDER,
									 0 + GUI_SCREEN_BORDER,
									 Screen.width - GUI_SCREEN_BORDER,
									 Screen.height - GUI_SCREEN_BORDER));
		switch(gameState)
		{
		case GameState.Title:
			
			GUILayout.BeginVertical();
			GUILayout.Box("Spare Change"); 		//TODO: Need a custom style for the title
			GUILayout.Space(100);
			if(GUILayout.Button("Start Game")) 	// TODO: Need a custom style for the menu items
			{
				//TODO: Do some kind of transition to the next UI state
				gameState = GameState.CharacterSelection;
			}
			GUILayout.EndVertical();
			break;
			
		case GameState.CharacterSelection:
			
			GUILayout.BeginHorizontal();
			
			GUILayout.BeginVertical();
			GUILayout.Box("Melee");
			//TODO: Display an image below the title
			if(GUILayout.Button("Select"))
			{
				SelectCharacter(Character.Type.Fighter);
			}
			GUILayout.EndVertical();
			////
			GUILayout.BeginVertical();
			GUILayout.Box("Ranged");
			//TODO: Display an image below the title
			if(GUILayout.Button("Select"))
			{
				SelectCharacter(Character.Type.Fighter);
			}
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			break;
			
		case GameState.BattleStart:
			
			// Setup the queue for the next
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			foreach(BattleActor enemy in enemies)
			{
				GUILayout.Box(string.Format("{0}", enemy.name));
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Start Battle"))
			{
				StartBattle();
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Box(string.Format("{0}", playerCharacter.name));
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			break;
			
		case GameState.BattleMode:
			
			GUILayout.BeginVertical();
			
			switch(currentTurn.state)
			{
			case BattleRound.State.SelectAction:
				
				GUILayout.Box("Select an Action");
				GUILayout.BeginHorizontal();
				foreach(BattleAction action in currentTurn.actor.actions.Values)
				{
					if(GUILayout.Button(string.Format("{0}", action.name)))
					{
						SelectedAction(action.name);
					}
				}
				GUILayout.EndHorizontal();
				break;
				
			case BattleRound.State.SelectTarget:
				GUILayout.Box("Select a Target");
				GUILayout.BeginHorizontal();
				// TODO: The action chosen will have an impact on who the possible targets can be
				foreach(BattleActor enemy in enemies)
				{
					if(GUILayout.Button(string.Format("{0}", enemy.name)))
					{
						SelectedEnemy(enemy);
					}
				}
				GUILayout.EndHorizontal();
				break;
				
			case BattleRound.State.ToHitRolling:
				
				break;
				
			case BattleRound.State.DamageRolling:
				
				break;
				
			case BattleRound.State.Finished:
				
				break;
			}
			
			GUILayout.EndVertical();
			break;
			
		case GameState.BattleOver:
			
			break;
		}
		
		GUILayout.EndArea();
	}
	
	private void SelectCharacter(Character.Type type)
	{
		switch(type)
		{
		case Character.Type.Conjurer:
			//TODO: playerCharacter = new Character("Rip Terror", 30, 5, pistol, 5);
			break;
		case Character.Type.Fighter:
			playerCharacter = new Character("Brucely", 30, 5, 5, weapons["nunchucks"]);
			break;
		case Character.Type.Shooter:
			playerCharacter = new Character("Rip Terror", 30, 5, 5, weapons["pistol"]);
			break;
		}
		
		// Generate enemies //TODO ...based on character
		CreateEnemies();
		
		gameState = GameState.BattleStart;
	}
	
	private void CreateEnemies(/*TODO: options*/)
	{
		enemies = new List<BattleActor>();
		// BatHead //
		enemies.Add(new BattleActor("BatHead"/*Name*/,
									20/*Health*/,
									8/*Speed*/,
									6/*Defense*/,
									weapons["bat"]/*Weapon*/));
		// Stabby //
		enemies.Add(new BattleActor("Stabby"/*Name*/,
									14/*Health*/,
									6/*Speed*/,
									4/*Defense*/,
									weapons["knife"]/*Weapon*/));
	}
	
	private void StartBattle()
	{
		Debug.Log("Starting Battle Mode");

		roundCount = 0;
		battleActorTurnCounts = new Dictionary<string, int>();
		battleActorTurnCounts.Add(playerCharacter.name, 0);
		foreach(BattleActor enemy in enemies)
		{
			battleActorTurnCounts.Add(enemy.name, 0);
		}

		//queue = new Queue<BattleRound>(); //Not in use, yet
		
		currentTurn = CalculateTurn();
		ExecuteRound();
		
		// Switch the gamestate
		gameState = GameState.BattleMode;
	}
	
	private BattleRound CalculateTurn()
	{
		BattleActor actorWithInitiative = playerCharacter;
		
		foreach(BattleActor enemy in enemies)
		{
			// Compare the current actor with initiative to the enemy, select the min of the two
			if(battleActorTurnCounts[actorWithInitiative.name] * actorWithInitiative.speed >
				battleActorTurnCounts[enemy.name] * enemy.speed)
			{
				actorWithInitiative = enemy;
			}
		}
		
		roundCount += actorWithInitiative.speed;
		return new BattleRound(actorWithInitiative);
	}
	
	private void ExecuteRound()
	{
		Debug.Log(string.Format("round: {0}", roundCount));

		// Process the current round
		switch(currentTurn.state)
		{
		case BattleRound.State.SelectAction:
			Debug.Log(string.Format("Beginning of {0}'s turn", currentTurn.actor.name));
			// Display choices
			break;
		case BattleRound.State.ToHitRolling:
			// Sometimes this isn't necessary.
			break;
		case BattleRound.State.DamageRolling:
			// There are two rolls, ToHit, and Damage
			break;
		case BattleRound.State.Finished:
			
			if(0 == enemies.Count)
			{
				gameState = GameState.BattleOver;
			}
			else {
				currentTurn = CalculateTurn();
			}
			break;
		}
	}
	
	void SelectedAction(string actionName)
	{
		selectedAction = currentTurn.actor.actions[actionName];
		currentTurn.state = BattleRound.State.SelectTarget;
	}
	
	Transform diceBox;
	Roll currentRoll;
	bool bThrowingDice;
	
	Transform getDiceBox()
	{
		if(null == diceBox)
			GameObject.Find("DiceBox");
		
		return diceBox;
	}
	
	void SelectedEnemy(BattleActor actor)
	{
		selectedActor = actor;
		currentTurn.state = BattleRound.State.ToHitRolling;
		
		currentRoll = selectedAction.roll;
		// Throw a dice corresponding to the action
		// TODO: This call is just a mockup and doesn't do everything necessary to return a result
		ThrowDice(currentRoll, getDiceBox());
	}

	void ThrowDice(Roll roll, Transform diceBox)
	{		
		for(int d=0; d < roll.count; d++)
		{
			GameObject die = CreateDie(roll.dieName);

			transform.rotation = Quaternion.LookRotation(Random.onUnitSphere);
			die.transform.position = 
			die.rigidbody.velocity = Camera.main.transform.forward * -15 + Camera.main.transform.right * 8;
			die.rigidbody.angularVelocity = Vector3.right * 25;
			
			startPosition = startPosition + new Vector3(die.transform.localScale.x + 1, 0, 0);
			
			//Camera.main.GetComponent<CamControl>().LookAtDice(die.transform);
		}
	}
	
	GameObject CreateDie(string dieName)
	{
		if(dieName.Equals("d4")){
			return InstantiateDie(d4);
		}
		else if(dieName.Equals("d6_s_p")) {
			return InstantiateDie(d6_s_p);
		}
		else if(dieName.Equals("d6_s_d")) {
			return InstantiateDie(d6_s_d);
		}
		else if(dieName.Equals("d6_r_p")) {
			return InstantiateDie(d6_r_p);
		}
		else if(dieName.Equals("d6_r_d")) {
			return InstantiateDie(d6_r_d);
		}
		else if(dieName.Equals("d8")) {
			return InstantiateDie(d8);
		}
		else if(dieName.Equals("d10")) {
			return InstantiateDie(d10);
		}
		else if(dieName.Equals("d12")) {
			return InstantiateDie(d12);
		}
		else if(dieName.Equals("d20")) {
			return InstantiateDie(d20);
		}
		else if(dieName.Equals("d100")) {
			return InstantiateDie(d100);
		}
		else {
			return null;
		}
	}
	
	GameObject InstantiateDie(GameObject diePrefab)
	{
		GameObject newDie = Instantiate(diePrefab) as GameObject;
		
		newDie.name = string.Format("{0}_{1}", newDie.name, dieCount);
		
		// Give it a random texture
		newDie.renderer.materials = new Material[2] {DiceMats[Random.Range(0, DiceMats.Count)], DiceMats[Random.Range(0, DiceMats.Count)]};
		
		dieCount++;
		
		return newDie;
	}
	
}
