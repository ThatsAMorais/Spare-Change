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
		
		public Weapon(string theName, Type theType, Roll theDamage)
		{
			name = theName;
			type = theType;
			damage = theDamage;
		}
	}
	
	/// <summary>
	/// Battle actor.
	/// </summary>
	public class BattleActor
	{
		/// <summary>
		/// Action.
		/// </summary>
		public class Action
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
			
			public Action(Type theType, string theName, Roll theRoll)
			{
				type = theType;
				name = theName;
				roll = theRoll;
			}
		}
		
		/// <summary>
		/// Attack.
		/// </summary>
		public class Attack : Action
		{			
			public Weapon weapon {get;set;}
			public int damageModifier {get;set;}
			public int hitModifier {get;set;}
			
			public Attack(string theName, Weapon theWeapon, int dmgMod=0, int hitMod=0, Action.Type actionType=Action.Type.Attack)
				: base((Action.Type.AttackAll == actionType ? actionType : Action.Type.Attack), theName, theWeapon.damage)
			{
				weapon = theWeapon;
				damageModifier = dmgMod;
				hitModifier = hitMod;
			}
		}
		
		/// <summary>
		/// Type.
		/// </summary>
		public enum Type 
		{
			Enemy,
			Character,
		}
		
		public string name {get;set;}
		public float health {get;set;}
		public float speed {get;set;}
		public Weapon weapon {get;set;}
		public float defense {get;set;}
		public List<Action> actions {get;set;}
		
		public BattleActor(string theName, float theHealth, float theSpeed, Weapon theWeapon, float theDefense, List<Action> theActions)
		{
			name = theName;
			health = theHealth;
			defense = theDefense;
			actions = theActions;
		}
	}
	
	/// <summary>
	/// Character.
	/// </summary>
	public class Character : BattleActor
	{
		public class Inventory
		{
			public double gold {get;set;}
			public double xp {get;set;}
			public List<Weapon> weapons {get;set;}
			
			public Inventory()
			{
				weapons = new List<Weapon>();
				gold = 0;
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
		
		public Character(string theName, float theHealth, float theSpeed, Weapon theWeapon, float theDefense, List<Action> theActions)
		: base(theName, theHealth, theSpeed, theWeapon, theDefense, theActions)
		{
			inventory = new Inventory();
			inventory.AddWeapon(theWeapon);
		}
		
		public void AddGold(int amountOfGold)
		{
			inventory.gold += amountOfGold;
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
	
	// Public
	public DiceThrower diceThrowerScript;
	
	// Private
	List<BattleActor> enemies;
	Character playerCharacter;
	GameState gameState;
	Queue<BattleRound> queue;
	
	void Awake ()
	{
		gameState = GameState.Title;  // TODO: This is where the game should eventually start
		
		//TEST DATA - to skip straight to GameState.BattleMode //
		Weapon bat = new Weapon("bat", Weapon.Type.Melee, new Roll("d6",1));
		BattleActor.Attack batSwing = new BattleActor.Attack("Swing", bat);
		
		Weapon knife = new Weapon("knife", Weapon.Type.Melee, new Roll("d4",1));
		BattleActor.Attack knifeSwing = new BattleActor.Attack("Swing", knife);
		BattleActor.Attack knifeStab = new BattleActor.Attack("Stab", knife);
				
		Weapon nunchucks = new Weapon("nunchucks", Weapon.Type.Melee, new Roll("d4", 2));
		BattleActor.Attack nunchuckSwing = new BattleActor.Attack("Swing", nunchucks, 0, 1);
		BattleActor.Attack nunchuckStrike = new BattleActor.Attack("Strike", nunchucks, 1, 0);
		
		Weapon pistol = new Weapon("pistol", Weapon.Type.Ranged, new Roll("d8",1));
		BattleActor.Attack pistolFire = new BattleActor.Attack("Fire", pistol);
		BattleActor.Attack pistolSideways = new BattleActor.Attack("Sideways Cocked", pistol, 1, -1);
		
		enemies = new List<BattleActor>();
		
		// BatHead //
		List<BattleActor.Action> batheadActions = new List<BattleActor.Action>();
		batheadActions.Add(batSwing);
		enemies.Add(new BattleActor("BatHead"/*Name*/,
									20/*Health*/,
									8/*Speed*/,
									bat/*Weapon*/,
									6/*Defense*/,
									batheadActions));
		// BatHead //
		List<BattleActor.Action> stabbyActions = new List<BattleActor.Action>();
		stabbyActions.Add(knifeSwing);
		stabbyActions.Add(knifeStab);
		enemies.Add(new BattleActor("Stabby"/*Name*/,
									14/*Health*/,
									6/*Speed*/,
									knife/*Weapon*/,
									4/*Defense*/,
									stabbyActions));
		
		// Test Player //
		List<BattleActor.Action> playerActions = new List<BattleActor.Action>();
		playerActions.Add(nunchuckSwing);
		playerActions.Add(nunchuckStrike);
		playerCharacter = new Character("Brucely", 30, 5, nunchucks, 5, playerActions);
		
		/////////////
	}
	
	public enum GameState 
	{
		Title,
		CharacterSelection,
		BattleStart,
		BattleMode,
		BattleOver,
	}
	
	class BattleRound
	{	
		public enum Type 
		{
			Start,
			ActorTurn,
			End,
		}
		
		public enum State
		{
			Beginning,
			ToHitRolling,
			DamageRolling,
			Finished
		}
		
		public State state {get;set;}
		public Type type {get;set;}
		public BattleActor actor {get;set;}
		
		public BattleRound(Type turnType)
		{
			type = turnType;
			state = State.Beginning;
		}
		
		public BattleRound(Type turnType, BattleActor turnActor)
		{
			type = turnType;
			actor = turnActor;
			state = State.Beginning;
		}
	}
	
	void Update ()
	{
		//TOdO: Since this is turn-based, there may be a way to avoid constant polling of the state every frame.
		// It is going to be very often the case that the system is waiting for user input, input that could drive
		// the state.  Yet, for now, this will avoid any need for signaling to drive the state.  Thats not to say
		// that signaling won't be necessary when waiting for rolls to be completed.
		
		switch(gameState)
		{
		case GameState.Title:
			Debug.Log("Skipping Title screen");
			gameState = GameState.CharacterSelection;
			break;
		case GameState.CharacterSelection:
			Debug.Log("Skipping Character Selection");
			gameState = GameState.BattleStart;
			break;
		case GameState.BattleStart:
			queue = new Queue<BattleRound>();
			queue.Enqueue(new BattleControllerScript.BattleRound(BattleRound.Type.Start));
			gameState = GameState.BattleMode;
			break;
		case GameState.BattleMode:
			Debug.Log("Entering Battle Mode");
			BattleRound currentRound = queue.Dequeue();
			switch(currentRound.type)
			{
			case BattleRound.Type.Start:
				// Setup the queue for the next 
				break;
			case BattleRound.Type.ActorTurn:
				// Process the current round
				switch(currentRound.state)
				{
				case BattleRound.State.Beginning:
					// Display choices
					break;
				case BattleRound.State.ToHitRolling:
					// Sometimes this isn't necessary.
					break;
				case BattleRound.State.ToHitRolling:
					// There are two rolls, ToHit, and Damage
					break;
				case BattleRound.State.Finished:
					// Switch the queue to the next
					break;
				}
				break;
			case BattleRound.Type.End:
				break;
			}
			break;
		case GameState.BattleOver:
			break;
		}
	}
	
	/*switch(currentRound.state)
	*/
}
