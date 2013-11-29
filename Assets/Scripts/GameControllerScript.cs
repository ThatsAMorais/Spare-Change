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

					// level 		// Weapon-Type 		// Weapon-Name
	public Dictionary<int,Dictionary<string,Weapon>> weapons;
					// level		// Enemy-Name
	public Dictionary<int, Dictionary<string, EnemyDefinition>> enemies;
					// level		// Enemy-Name
	public Dictionary<int, Dictionary<string, EnemyDefinition>> bosses;
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
		int weaponLevel = 0;
		weapons  = new Dictionary<int, Dictionary<string, Weapon>>();

		// -- Weapons
		weaponLevel++; // Level 1 Weapons

		weapons[weaponLevel] = new Dictionary<string, Weapon>();

		/// Melee
		Weapon bat = new Weapon("Bat", Weapon.Type.Melee, 1, 0, 0, weaponLevel, new Roll("d6",1));
		bat.AddAttack(new Attack("Swing", bat.roll));
		bat.AddAttack(new Attack("Chop", bat.roll, 3, -2));
		bat.AddAttack(new Attack("Prod", bat.roll, -3, 3));
		weapons[weaponLevel].Add(bat.name, bat);

		Weapon knife = new Weapon("Knife", Weapon.Type.Melee, 1, -1, 0, weaponLevel, new Roll("d4",1));
		knife.AddAttack(new Attack("Swing", knife.roll, -1, 1));
		knife.AddAttack(new Attack("Stab", knife.roll, 3, -1));
		weapons[weaponLevel].Add(knife.name, knife);

		Weapon nunchucks = new Weapon("Nunchucks", Weapon.Type.Melee, 1, 0, 0, weaponLevel, new Roll("d4", 2));
		nunchucks.AddAttack(new Attack("Sideward", nunchucks.roll));
		nunchucks.AddAttack(new Attack("Upward", nunchucks.roll, 3, -2));
		weapons[weaponLevel].Add(nunchucks.name, nunchucks);

		/// Ranged
		Weapon pistol = new Weapon("Pistol", Weapon.Type.Ranged, 0, 0, 0, weaponLevel, new Roll("d8",1));
		pistol.AddAttack(new Attack("Fire", pistol.roll, 0, 0));
		pistol.AddAttack(new Attack("Sideways Cocked", pistol.roll, 3, -2));
		weapons[weaponLevel].Add(pistol.name, pistol);

		Weapon crossbow = new Weapon("Crossbow", Weapon.Type.Ranged, 0, 1, 0, weaponLevel, new Roll("d6",2));
		crossbow.AddAttack(new Attack("Fire", crossbow.roll, 0, -1));
		weapons[weaponLevel].Add(crossbow.name, crossbow);

		Weapon throwingStars = new Weapon("Throwing Stars", Weapon.Type.Ranged, 0, -1, 0, weaponLevel, new Roll("d4",2));
		throwingStars.AddAttack(new Attack("Fling", throwingStars.roll));
		throwingStars.AddAttack(new Attack("Critical", throwingStars.roll, 5, -3));
		weapons[weaponLevel].Add(throwingStars.name, throwingStars);

		/// Magic
		Weapon iceCubes = new Weapon("Magic Ice Cubes", Weapon.Type.Magical, 0, -1, 1, weaponLevel, new Roll("d6", 1));
		iceCubes.AddAttack(new Attack("Throw Cold", iceCubes.roll));
		iceCubes.AddAttack(new Attack("Palm to Face", iceCubes.roll, -2, 2));
		weapons[weaponLevel].Add(iceCubes.name, iceCubes);

		Weapon firePaper = new Weapon("Magical Lit Paper", Weapon.Type.Magical, 1, -1, 1, weaponLevel, new Roll("d4", 2));
		firePaper.AddAttack(new Attack("Flaming Flyer", firePaper.roll));
		firePaper.AddAttack(new Attack("Throw Lighter", firePaper.roll, 6, -4));
		firePaper.AddAttack(new Attack("Spit Gas", firePaper.roll, -2, 2));
		weapons[weaponLevel].Add(firePaper.name, firePaper);

		Weapon metalShards = new Weapon("Magical Metal Shards", Weapon.Type.Magical, 1, -1, 1, weaponLevel, new Roll("d6", 2));
		metalShards.AddAttack(new Attack("Spare Change", metalShards.roll));
		metalShards.AddAttack(new Attack("Coin Roll", metalShards.roll, -4, 3));
		metalShards.AddAttack(new Attack("Purse Punch", metalShards.roll, 3, -3));
		weapons[weaponLevel].Add(metalShards.name, metalShards);
		/////////////


		// -- Create a few enemies
		int enemyLevel = 0;
		enemies = new Dictionary<int, Dictionary<string, EnemyDefinition>>();

		enemyLevel++;
		enemies[enemyLevel] = new Dictionary<string, EnemyDefinition>();

		// Easy
		enemies[enemyLevel].Add(
			"Bunny",
			new EnemyDefinition("Bunny", weapons[1]["Knife"], 1, 1, 3, 10, -1, enemyLevel));

		// BatHead //
		enemies[enemyLevel].Add(
			"Bathead", new EnemyDefinition("Bathead"	/* Enemy Name */,
			weapons[1]["Bat"],		/* Weapon */
			5,											/* Experience Value */
			15,											/* Change Value */
			20,											/* Health */
			8,											/* Speed */
			0,											/* Defense */
		    enemyLevel));
		// Stabby //
		enemies[enemyLevel].Add(
			"Stabby",
			new EnemyDefinition("Stabby", weapons[1]["Knife"], 4, 10, 14, 6, 0, enemyLevel));

		// Pistol Peach //
		enemies[enemyLevel].Add(
			"Pistol Peach",
			new EnemyDefinition("Pistol Peach", weapons[1]["Pistol"], 4, 10, 16, 6, 0, enemyLevel));
		
		// Kris Crossbow //
		enemies[enemyLevel].Add(
			"Kris Crossbow",
			new EnemyDefinition("Kris Crossbow", weapons[1]["Crossbow"], 4, 10, 15, 6, 0, enemyLevel));

		int bossLevel = 0;
		bosses = new Dictionary<int, Dictionary<string, EnemyDefinition>>();
		
		bossLevel++;
		bosses[bossLevel] = new Dictionary<string, EnemyDefinition>();

		// Bossy Hoss //
		bosses[bossLevel].Add(
			"Bossy Hoss",
			new EnemyDefinition("Bossy Hoss", weapons[1]["Magical Metal Shards"], 20, 30, 30, 5, -1, enemyLevel));
		/////////////
	}

	public List<EnemyDefinition> getEnemiesPerLevel(int level)
	{
		List<EnemyDefinition> enemiesAtLevel = new List<EnemyDefinition>();
		Dictionary<string, EnemyDefinition> temp = new Dictionary<string, EnemyDefinition>();

		// Attempt to get the monsters corresponding to the previous level
		if(enemies.TryGetValue(level-1, out temp))
		{
			// Add these definitions to the return list
			enemiesAtLevel.AddRange(temp.Values);
		}
		// Attempt to get the monsters corresponding to the current level
		if(enemies.TryGetValue(level, out temp))
		{
			// Add these definitions to the return list
			enemiesAtLevel.AddRange(temp.Values);
		}
		return enemiesAtLevel;
	}

	public List<EnemyDefinition> getBossPerLevel(int level)
	{
		return new List<EnemyDefinition>(bosses[level].Values);
	}

	public EnemyDefinition getEnemyByName(int level, string enemyName)
	{
		EnemyDefinition enemy;
		enemies[level].TryGetValue(enemyName, out enemy);

		return enemy;
	}

	public EnemyDefinition getBossByName(int level, string bossName)
	{
		EnemyDefinition boss;
		bosses[level].TryGetValue(bossName, out boss);
		
		return boss;
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
									getWeapon(scoreoidPlayer.weapon),
									scoreoidPlayer.xp,
									scoreoidPlayer.level,
									scoreoidPlayer.change,
									scoreoidPlayer.numberOfKills);

		currentCharacter = player;

		if(null == player.weapon)
			Utilities().setGameState(GameState.WeaponSelection);
		else
			Utilities().setGameState(GameState.PlayerProfile);
	}

	public List<Weapon> getWeaponTypes(int level)
	{
		//TODO: This will throw keyexception until I finish buiding out the content more
		//return new List<Weapon>(weapons[level].Values);
		return new List<Weapon>(weapons[1].Values);
	}

	public Weapon getWeapon(string weaponName)
	{
		/// HACK: To cover the fact that I changed the weapon names,
		///  and, also because I can't seem to use StringComparer
		///  with these dictionaries.  So, since there are already
		///  users with the weapons stored as "bat" and it is now
		///  called "Bat", they will encounter bad results
		if(null != weaponName && !weaponName.Equals(""))
		{
			if(char.IsLower(weaponName[0]))
				weaponName = char.ToUpper(weaponName[0]) + weaponName.Substring(1);

			foreach(Dictionary<string, Weapon> weaponLevel in weapons.Values)
			{
				if(true == weaponLevel.ContainsKey(weaponName))
				{
					return weaponLevel[weaponName];
				}
			}
		}

		return null;
	}

	public EnemyDefinition getEnemy(string enemyName)
	{
		foreach(Dictionary<string, EnemyDefinition> enemyLevel in enemies.Values)
		{
			if(true == enemyLevel.ContainsKey(enemyName))
			{
				return enemyLevel[enemyName];
			}
		}
		return null;
	}


	/// <summary>
	/// Game state.
	/// </summary>
	public enum GameState
	{
		NoState,
		Title,
		Register,
		Login,
		WeaponSelection,
		PlayerProfile,
		BattleMode,
		BattleOver,
	}


	// Description of an Enemy
	public class EnemyDefinition
	{
		public enum Type
		{
			Normal,
			Boss
		}

		public string name {get;set;}
		public Weapon weapon {get;set;}
		public float experienceValue {get;set;}
		public float changeValue {get;set;}
		public int health {get;set;}
		public int speed {get;set;}
		public int defense {get;set;}
		public int level {get;set;}

		public EnemyDefinition(string enemyName, Weapon enemyWeapon, float enemyExpValue, float enemyChangeValue, int enemyHealth, int enemySpeed, int enemyDefense, int enemyLevel)
		{
			name = enemyName;
			weapon = enemyWeapon;
			experienceValue = enemyExpValue;
			changeValue = enemyChangeValue;
			health = enemyHealth;
			speed = enemySpeed;
			defense = enemyDefense;
			level = enemyLevel;
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
		public Type type {get;set;}
		public int damageModifier {get;set;}
		public int speedModifier {get;set;}
		public int defenseModifier {get;set;}
		public int level {get;set;}
		public Roll roll {get;set;}

		public Dictionary<string,Attack> attacks {get;set;}

		public Weapon(string theName, Type theType, int dmgMod, int spdMod, int defMod, int weaponLevel, Roll weaponRoll)
		{
			name = theName;				// A human-readable name to be displayed in the UI
			type = theType;				// See Weapon.Type
			damageModifier = dmgMod;		// Additional modifiers that the weapon adds to the damage-roll
			speedModifier = spdMod;		// A modifier effecting the weilder's speed
			defenseModifier = defMod;	// A modifier effecting the weilder's defense
			level = weaponLevel;
			roll = weaponRoll;

			attacks = new Dictionary<string, Attack>();
		}

		public void AddAttack(Attack attack)
		{
			attacks.Add(attack.name,attack);
		}
	}

	public static Roll TO_HIT_ROLL = new Roll("d20",1);

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
		public int hitModifier {get;set;}

		
		public BattleAction(Type theType, string theName, Roll theRoll, int hitMod)
		{
			type = theType;
			name = theName;
			roll = theRoll;
			hitModifier = hitMod;
		}
	}
	
	/// <summary>
	/// Attack.
	/// </summary>
	public class Attack : BattleAction
	{
		public int damageModifier {get;set;}

		public Attack(string theName, Roll theRoll,
		              int dmgMod=0, int hitMod=0,
		              BattleAction.Type actionType=BattleAction.Type.Attack)
			: base((BattleAction.Type.AttackAll == actionType ? actionType : BattleAction.Type.Attack),
			       theName, theRoll, hitMod)
		{
			damageModifier = dmgMod;
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
