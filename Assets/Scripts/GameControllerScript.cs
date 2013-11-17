using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Roll = DiceControllerScript.Roll;
using Player = BattleControllerScript.Player;
using ScoreoidPlayer = ScoreoidInterface.ScoreoidPlayer;

public class GameControllerScript : MonoBehaviour {


	// -- Privates

	GameState gameState;
	Player currentCharacter;

	public Dictionary<string,Weapon> weapons;
	public Dictionary<string,EnemyDefinition> enemies;
	public Dictionary<string,BattleActorDefinition> actorTypes;

	// -- Unity

	void Awake ()
	{
		gameState = GameState.Title;
	}
	
	void Update ()
	{
	}


	void OnEnable ()
	{
		// -- Weapons
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

		// -- Create actor types
		actorTypes = new Dictionary<string, BattleActorDefinition>();
		actorTypes.Add("Shooter", new BattleActorDefinition("Shooter", 30, 5, 5, weapons["pistol"]));
		actorTypes.Add("Fighter", new BattleActorDefinition("Fighter", 30, 5, 5, weapons["bat"]));
		actorTypes.Add("Burglar", new BattleActorDefinition("Burglar", 30, 5, 5, weapons["knife"]));
		actorTypes.Add("Boxer", new BattleActorDefinition("Boxer", 30, 5, 5, weapons["nunchucks"]));
		/////////////

		// -- Create a few enemies
		enemies = new Dictionary<string, EnemyDefinition>();
		// BatHead //
		enemies.Add("Bathead", new EnemyDefinition("Bathead"									/* Enemy Name */,
										5											/* Experience Value */,
										15											/* Change Value */,
										new BattleActorDefinition("BatHead"			/* Name */,
																	20				/* Health */,
																	8				/* Speed */,
																	6				/* Defense */,
																	weapons["bat"]	/* Weapon */)));
		// Stabby //
		enemies.Add("Stabby", new EnemyDefinition("Stabby", 4, 10, new BattleActorDefinition("Stabby", 14, 6, 4, weapons["knife"])));
		// Bossy Hoss //
		enemies.Add("Bossy Hoss", new EnemyDefinition("Bossy Hoss", 20, 30, new BattleActorDefinition("Bossy Hoss", 30, 5, 8, weapons["pistol"])));
		/////////////
	}

	// -- APIs

	public void setGameState(GameState newGameState)
	{
		gameState = newGameState;
	}

	public GameState getGameState()
	{
		return gameState;
	}

	public Player getCurrentCharacter()
	{
		return currentCharacter;
	}

	public void setCurrentCharacter(ScoreoidPlayer scoreoidPlayer)
	{
		Player player = new Player(actorTypes[scoreoidPlayer.actorClassName], scoreoidPlayer.playerName);

		player.xp = scoreoidPlayer.xp;
		player.level = scoreoidPlayer.level;
		player.change = scoreoidPlayer.change;
		player.weapon = weapons[scoreoidPlayer.weapon];
		player.kills = scoreoidPlayer.numberOfKills;

		currentCharacter = player;

		Utilities().setGameState(GameState.PlayerProfile);
	}

	public List<string> getActorTypeStrings()
	{
		return new List<string>(actorTypes.Keys);
	}

	public List<BattleActorDefinition> getActorTypes()
	{
		return new List<BattleActorDefinition>(actorTypes.Values);
	}

	public Weapon getWeapon(string weaponName)
	{
		if(true == weapons.ContainsKey(weaponName))
		{
			return weapons[weaponName];
		}

		return null;
	}

	public EnemyDefinition getEnemy(string enemyName)
	{
		if(true == enemies.ContainsKey(enemyName))
		{
			return enemies[enemyName];
		}

		return null;
	}


	/// <summary>
	/// Game state.
	/// </summary>
	public enum GameState
	{
		Title,
		Register,
		Login,
		CharacterSelection,
		PlayerProfile,
		BattleMode,
		BattleOver
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

	// Description of an Enemy
	public class EnemyDefinition
	{
		public string name {get;set;}
		public double experienceValue {get;set;}
		public double changeValue {get;set;}
		public BattleActorDefinition definition {get;set;}

		public EnemyDefinition(string enemyName, double expVal, double changeVal, BattleActorDefinition actorDefinition)
		{
			name = enemyName;
			experienceValue = expVal;
			changeValue = changeVal;
			definition = actorDefinition;
		}
	}

	// Description of a Battle Actor
	public class BattleActorDefinition
	{
		public string name {get;set;}
		public int health {get;set;}
		public int speed {get;set;}
		public int defense {get;set;}
		public Weapon weapon {get;set;}

		public BattleActorDefinition(string theName, int theHealth, int theSpeed, int theDefense, Weapon theWeapon)
		{
			name = theName;
			health = theHealth;
			speed = theSpeed;
			defense = theDefense;
			weapon = theWeapon;
		}
	}


	UtilitiesScript utilitiesScript;
	UtilitiesScript Utilities()
	{
		if(null == utilitiesScript)
			utilitiesScript = GameObject.Find("Utilities").GetComponent<UtilitiesScript>();

		return utilitiesScript;
	}
}
