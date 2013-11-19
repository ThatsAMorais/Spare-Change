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

		Weapon bat = new Weapon("bat", Weapon.Type.Melee, new Roll("d6",1), 0, 0, 0);
		bat.AddAttack(new Attack("Swing", bat));
		weapons.Add(bat.name, bat);

		Weapon knife = new Weapon("knife", Weapon.Type.Melee, new Roll("d4",1), 0, 0, 0);
		knife.AddAttack(new Attack("Swing", knife));
		knife.AddAttack(new Attack("Stab", knife));
		weapons.Add(knife.name, knife);

		Weapon nunchucks = new Weapon("nunchucks", Weapon.Type.Melee, new Roll("d4", 2), 0, 0, 0);
		nunchucks.AddAttack(new Attack("Swing", nunchucks, 0, 1));
		nunchucks.AddAttack(new Attack("Strike", nunchucks, 1, 0));
		weapons.Add(nunchucks.name, nunchucks);

		Weapon pistol = new Weapon("pistol", Weapon.Type.Ranged, new Roll("d8",1), 0, 0, 0);
		pistol.AddAttack(new Attack("Fire", pistol));
		pistol.AddAttack(new Attack("Sideways Cocked", pistol, 1, -1));
		weapons.Add(pistol.name, pistol);
		/////////////


		// -- Create a few enemies
		enemies = new Dictionary<string, EnemyDefinition>();
		// BatHead //
		enemies.Add("Bathead", new EnemyDefinition("Bathead"	/* Enemy Name */,
					weapons["bat"],								/* Weapon */
					5,											/* Experience Value */
					15,											/* Change Value */
					20,											/* Health */
					8,											/* Speed */
					6));										/* Defense */

		// Stabby //
		enemies.Add("Stabby", new EnemyDefinition("Stabby", weapons["knife"], 4, 10, 14, 6, 4));
		// Bossy Hoss //
		enemies.Add("Bossy Hoss", new EnemyDefinition("Bossy Hoss", weapons["pistol"], 20, 30, 30, 5, 8));
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
		Player player = new Player(scoreoidPlayer.playerName,
									weapons[scoreoidPlayer.weapon],
									scoreoidPlayer.xp,
									scoreoidPlayer.level,
									scoreoidPlayer.change,
									scoreoidPlayer.numberOfKills);

		currentCharacter = player;

		Utilities().setGameState(GameState.PlayerProfile);
	}

	public List<Weapon> getWeaponTypes(int level)
	{
		return new List<Weapon>(weapons.Values);
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


	// Description of an Enemy
	public class EnemyDefinition
	{
		public string name {get;set;}
		public Weapon weapon {get;set;}
		public float experienceValue {get;set;}
		public float changeValue {get;set;}
		public int health {get;set;}
		public int speed {get;set;}
		public int defense {get;set;}

		public EnemyDefinition(string enemyName, Weapon enemyWeapon, float enemyExpValue, float enemyChangeValue, int enemyHealth, int enemySpeed, int enemyDefense)
		{
			name = enemyName;
			weapon = enemyWeapon;
			experienceValue = enemyExpValue;
			changeValue = enemyChangeValue;
			health = enemyHealth;
			speed = enemySpeed;
			defense = enemyDefense;
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
		public int dmgModifier {get;set;}
		public int speedModifier {get;set;}
		public int defenseModifier {get;set;}

		public Dictionary<string,Attack> attacks {get;set;}

		public Weapon(string theName, Type theType, Roll theDamage, int dmgMod, int spdMod, int defMod)
		{
			name = theName;				// A human-readable name to be displayed in the UI
			type = theType;				// See Weapon.Type
			damage = theDamage;			// 1 or more dice, as a set
			dmgModifier = dmgMod;		// Additional modifiers that the weapon adds to the damage-roll
			speedModifier = spdMod;		// A modifier effecting the weilder's speed
			defenseModifier = defMod;	// A modifier effecting the weilder's defense

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


	UtilitiesScript utilitiesScript;
	UtilitiesScript Utilities()
	{
		if(null == utilitiesScript)
			utilitiesScript = GameObject.Find("Utilities").GetComponent<UtilitiesScript>();

		return utilitiesScript;
	}
}
