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

	void NewWeapon(string theName, Weapon.Type theType,
	               int dmgMod, int spdMod, int defMod,
	               int weaponLevel, Roll weaponRoll,
	               params Attack[] attacks)
	{
		Weapon weapon;

		// Don't create a weapon that doesn't have any attacks
		if(null == attacks || 0 == attacks.Length)
			return;

		weapon = new Weapon(theName, theType, dmgMod, spdMod, defMod, weaponLevel, weaponRoll);
		
		// Add the attacks to the weapon
		foreach(Attack attack in attacks)
		{
			attack.roll = weaponRoll;
			weapon.AddAttack(attack);
		}

		// Ensure the weapons Dict is allocated
		if(null == weapons)
			weapons = new Dictionary<int, Dictionary<string, Weapon>>();

		// Ensure the specified level in 'weapons' is allocated
		if(false == weapons.ContainsKey(weaponLevel))
			weapons[weaponLevel] = new Dictionary<string, Weapon>();

		// Add the weapon
		weapons[weaponLevel].Add(weapon.name, weapon);
	}

	void NewEnemy(string enemyName, Weapon enemyWeapon,
	              float enemyExpValue, float enemyChangeValue,
	              int enemyHealth, int enemySpeed, int enemyDefense,
	              int enemyLevel, bool bBoss=false)
	{
		EnemyDefinition enemy;

		if(null == enemyWeapon)
			return;
		
		enemy = new EnemyDefinition(enemyName, enemyWeapon,
		                            enemyExpValue, enemyChangeValue,
		                            enemyHealth, enemySpeed, enemyDefense,
		                            enemyLevel);

		if(true == bBoss)
		{
			if(null == bosses)
				bosses = new Dictionary<int, Dictionary<string, EnemyDefinition>>();

			if(false == bosses.ContainsKey(enemyLevel))
				bosses[enemyLevel] = new Dictionary<string, EnemyDefinition>();

			bosses[enemyLevel].Add(enemy.name, enemy);
		}
		else
		{
			if(null == enemies)
				enemies = new Dictionary<int, Dictionary<string, EnemyDefinition>>();

			if(false == enemies.ContainsKey(enemyLevel))
				enemies[enemyLevel] = new Dictionary<string, EnemyDefinition>();

			enemies[enemyLevel].Add(enemy.name, enemy);
		}
	}

	void CreateWeapons()
	{
		int weaponLevel = 0;
		
		// Level 0 //

		/// Melee
		NewWeapon("Knife", Weapon.Type.Melee, 1, -1, 0, weaponLevel, new Roll("d4",1),
		          new Attack("Swing", -1, 1),
		          new Attack("Stab", 3, -1));
		NewWeapon("Bat", Weapon.Type.Melee, 1, 0, 0, weaponLevel, new Roll("d6",1),
		          new Attack("Swing"),
		          new Attack("Chop", 3, -2));
		/// Ranged
		NewWeapon("Crossbow", Weapon.Type.Ranged, 0, 1, 0, weaponLevel, new Roll("d6",1),
		          new Attack("Fire", 0, -1));
		/// Magic
		NewWeapon("Magical Spray", Weapon.Type.Magical, 0, -1, 1, weaponLevel, new Roll("d4", 1),
		          new Attack("Flu Gleek"),
		          new Attack("Snickers Spit", 2, -1));
		
		// Level 1 //

		weaponLevel++;
		NewWeapon("Rusty Hatchet", Weapon.Type.Melee, 1, -1, 0, weaponLevel, new Roll("d6", 1),
		          new Attack("Swipe"),
		          new Attack("Chop", 4, -3),
		          new Attack("Prod", -3, 3));
		NewWeapon("Pistol", Weapon.Type.Ranged, 0, 0, 0, weaponLevel, new Roll("d8",1),
		          new Attack("Fire", 0, 0),
		          new Attack("Sideways Cocked", 3, -2));
		NewWeapon("Expensive Crossbow", Weapon.Type.Ranged, 0, 1, 0, weaponLevel, new Roll("d6",2),
		          new Attack("Fire", 0, -1),
		          new Attack("Spread", 3, -2));
		NewWeapon("Magical Freezing", Weapon.Type.Magical, 0, -1, 1, weaponLevel, new Roll("d6", 1),
		          new Attack("Throw Ice Cubes"),
		          new Attack("Palm to Face", -2, 2));
		NewWeapon("Magical Shards", Weapon.Type.Magical, 1, -1, 1, weaponLevel, new Roll("d6", 2),
		          new Attack("Spare Change"),
		          new Attack("Coin Roll", -4, 3),
		          new Attack("Purse Punch", 3, -3));
		
		// Level 2 //
		weaponLevel++;
		NewWeapon("Nunchucks", Weapon.Type.Melee, 1, 0, 0, weaponLevel, new Roll("d4", 2),
		          new Attack("Sideward"),
		          new Attack("Upward", 3, -2));
		NewWeapon("Bo", Weapon.Type.Melee, 1, 0, -1, weaponLevel, new Roll("d6", 1),
		          new Attack("Thrust", 2, -1),
		          new Attack("Two-Step", -1, 1));
		NewWeapon("Throwing Stars", Weapon.Type.Ranged, 0, -1, 0, weaponLevel, new Roll("d4",2),
		          new Attack("Fling"),
		          new Attack("Critical", 5, -3));
		NewWeapon("Magical Burning", Weapon.Type.Magical, 1, -1, 1, weaponLevel, new Roll("d4", 2),
		          new Attack("Flaming Flyer"),
		          new Attack("Throw Lighter", 6, -4),
		          new Attack("Spit Gas", -2, 2));
		/////////////
	}

	void CreateEnemies()
	{
		// -- Create a few enemies
		int enemyLevel = 0;


		// Level 0 //

		NewEnemy("Bunny", getWeapon("Knife"), 6, 1, 3, 10, -1, enemyLevel);
		NewEnemy(
			/* Enemy Name */"Bathead",
			/* Weapon */getWeapon("Bat"),
			/* Experience Value */9,
			/* Change Value */15,
			/* Health */20,
			/* Speed */8,
			/* Defense */0,
			enemyLevel);
		// Boss
		NewEnemy("Bossy Hoss", getWeapon("Magical Shards"), 11, 30, 30, 5, -1, enemyLevel, true);


		// Level 1 //

		enemyLevel++;
		NewEnemy("Stabby", getWeapon("Knife"), 10, 10, 14, 6, 0, enemyLevel);
		NewEnemy("Pistol Peach", getWeapon("Pistol"), 12, 10, 16, 6, 0, enemyLevel);
		NewEnemy("Kris Crossbow", getWeapon("Crossbow"), 12, 10, 15, 6, 0, enemyLevel);
		//Boss
		NewEnemy("Thuggalo", getWeapon("Hatchet"), 20, 30, 40, 5, -1, enemyLevel, true);


		// Level 2 //

		enemyLevel++;
		NewEnemy("Rival Schooler", getWeapon("Bo"), 10, 10, 18, 6, 0, enemyLevel);
		NewEnemy("Anime Punk", getWeapon("Nunchucks"), 10, 10, 20, 6, 0, enemyLevel);
		NewEnemy("Kitty Kat", getWeapon("Throwing Stars"), 10, 10, 15, 6, 0, enemyLevel);
		//Boss
		NewEnemy("Dojo Property Mgr", getWeapon("Pistol"), 30, 30, 55, 4, -1, enemyLevel, true);

		/////////////
	}

	void OnEnable ()
	{
		CreateWeapons();

		CreateEnemies();
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
		Dictionary<string, EnemyDefinition> bossDict = new Dictionary<string, EnemyDefinition>();

		if(true == bosses.TryGetValue(level, out bossDict))
			return new List<EnemyDefinition>(bosses[level].Values);
		else
			return new List<EnemyDefinition>(bosses[0].Values);
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
		Dictionary<string,Weapon> weaponTypes = new Dictionary<string, Weapon>();

		if(weapons.TryGetValue(level, out weaponTypes))
			return new List<Weapon>(weaponTypes.Values);
		else
			return new List<Weapon>(weapons[0].Values);
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

		return getWeapon("Magical Shards");
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
		Instructions,
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
		
		public BattleAction(Type theType, string theName, int hitMod=0, Roll theRoll=null)
		{
			type = theType;
			name = theName;

			if(null != theRoll)	//  leave it uninitialized, bleh
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

		public Attack(string theName, int dmgMod=0, int hitMod=0,
		              BattleAction.Type actionType=BattleAction.Type.Attack)
			: base((BattleAction.Type.AttackAll == actionType ? actionType : BattleAction.Type.Attack),
			       theName, hitMod)
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
