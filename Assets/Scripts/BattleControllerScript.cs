using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using GameState = GameControllerScript.GameState;
using EnemyDefinition = GameControllerScript.EnemyDefinition;
using BattleAction = GameControllerScript.BattleAction;
using Attack = GameControllerScript.Attack;
using Weapon = GameControllerScript.Weapon;

using Roll = DiceControllerScript.Roll;

public class BattleControllerScript : MonoBehaviour {

	public class BattleActor
	{
		public string name {get;set;}
		public Weapon weapon {get;set;}
		public int health {get;set;}
		public int remainingHealth {get;set;}
		public int speed {get;set;}
		public int defense {get;set;}

		public BattleActor(string actorName, Weapon actorWeapon)
		{
			name = actorName;
			weapon = actorWeapon;
			remainingHealth = 1;			// Default
			speed = 5;						// Default
			defense = 0;					// Default
		}

		public BattleActor(string actorName, Weapon actorWeapon, int actorHealth, int actorSpeed, int actorDefense)
		{
			name = actorName;
			weapon = actorWeapon;
			health = remainingHealth = actorHealth;
			speed = actorSpeed;
			defense = actorDefense;
		}
		
		public void addDamage(int dmg)
		{
			remainingHealth -= dmg;
		}

		public void changeWeapon(Weapon theWeapon)
		{
			// Set this as the current weapon
			weapon = theWeapon; // Accept the change
		}

		public List<Attack> getActions()
		{
			return new List<Attack>(weapon.attacks.Values);
		}

		public int getDefense()
		{
			return defense + weapon.defenseModifier;
		}

		public int getSpeed()
		{
			return speed + weapon.speedModifier;
		}
	}

	public class Player : BattleActor
	{
		private static int PLAYER_LEVEL_MAX = 200;
		private static Dictionary<int, float> levelTable = new Dictionary<int, float>()
		{
			{1, 0},{2, 83},{3, 174},{4, 276},{5, 388},{6, 512},{7, 650},{8, 801},{9, 969},
			{10, 1154},{11, 1358},{12, 1584},{13, 1833},{14, 2107},
			{15, 2411},{16, 2746},{17, 3115},{18, 3523},{19, 3973},
			{20, 4470},{21, 5018},{22, 5624},{23, 6291},{24, 7028},
			{25, 7842},{26, 8740},{27, 9730},{28, 10824},{29, 12031},
			{30, 13363},{31, 14833},{32, 16456},{33, 18247},{34, 20224},
			{35, 22406},{36, 24815},{37, 27473},{38, 30408},{39, 33648},
			{40, 37224},{41, 41171},{42, 45529},{43, 50339},{44, 55649},
			{45, 61512},{46, 67983},{47, 75127},{48, 83014},{49, 91721},
			{50, 101333},{51, 111945},{52, 123660},{53, 136594},{54, 150872},
			{55, 166636},{56, 184040},{57, 203254},{58, 224466},{59, 247886},
			{60, 273742},{61, 302288},{62, 333804},{63, 368599},{64, 407015},
			{65, 449428},{66, 496254},{67, 547953},{68, 605032},{69, 668051},
			{70, 737627},{71, 814445},{72, 899257},{73, 992895},{74, 1096278},
			{75, 1210421},{76, 1336443},{77, 1475581},{78, 1629200},{79, 1798808},
			{80, 1986068},{81, 2192818},{82, 2421087},{83, 2673114},{84, 2951373},
			{85, 3258594},{86, 3597792},{87, 3972294},{88, 4385776},{89, 4842295},
			{90, 5346332},{91, 5902831},{92, 6517253},{93, 7195629},{94, 7944614},
			{95, 8771558},{96, 9684577},{97, 10692629},{98, 11805606},{99, 13034431},
			{100, 14391160},{101, 15889109},{102, 17542976},{103, 19368992},{104, 21385073},
			{105, 23611006},{106, 26068632},{107, 28782069},{108, 31777943},{109, 35085654},
			{110, 38737661},{111, 42769801},{112, 47221641},{113, 52136869},{114, 57563718},
			{115, 63555443},{116, 70170840},{117, 77474828},{118, 85539082},{119, 94442737},
			{120, 104273167},{121, 115126838},{122, 127110260},{123, 140341028},{124, 154948977},
			{125, 171077457},{126, 188884740},{127, 208545572},{128, 230252886},{129, 254219702},
			{130, 280681209},{131, 309897078},{132, 342154009},{133, 377768545},{134, 417090179},
			{135, 460504778},{136, 508438379},{137, 561361362},{138, 619793069},{139, 684306901},
			{140, 755535943},{141, 834179178},{142, 921008346},{143, 1016875516},{144, 1122721449},
			{145, 1239584831},{146, 1368612462},{147, 1511070513},{148, 1668356950},{149, 1842015252},
			{150, 2033749558},{151, 2245441392},{152, 2479168121},{153, 2737223349},{154, 3022139416},
			{155, 3336712255},{156, 3684028823},{157, 4067497401},{158, 4490881032},{159, 4958334456},
			{160, 5474444875},{161, 6044276973},{162, 6673422613},{163, 7368055713},{164, 8134992831},
			{165, 8981760056},{166, 9916666866},{167, 10948887667},{168, 12088551825},{169, 13346843067},
			{170, 14736109228},{171, 16269983424},{172, 17963517835},{173, 19833331415},{174, 21897772978},
			{175, 24177101254},{176, 26693683698},{177, 29472215980},{178, 32539964331},{179, 35927033113},
			{180, 39666660232},{181, 43795543315},{182, 48354199826},{183, 53387364671},{184, 58944429193},
			{185, 65079925854},{186, 71854063374},{187, 79333317570},{188, 87591083692},{189, 96708396670},
			{190, 106774726318},{191, 117888855318},{192, 130159848595},{193, 143708123591},{194, 158666631937},
			{195, 175182164138},{196, 193416790048},{197, 213549449297},{198, 235777707252},{199, 260319693761},
			{200, 287416243706}  // That'll keep'em busy...
		};

		public float change {get;set;}
		public float xp {get;set;}
		public int level {get;set;}
		public int kills {get;set;}
		public bool bLeveledUp {get;set;}

		public Player(string playerName, Weapon playerWeapon, float playerXp, int playerLevel, float playerChange, int playerKills)
			: base(playerName, playerWeapon)
		{
			name = playerName;
			xp = playerXp;
			level = playerLevel;
			change = playerChange;
			kills = playerKills;
			health = 30 * (int)(1 + level*0.1f);
			remainingHealth = health;
		}

		public void AddChange(float amountOfChange)
		{
			change += amountOfChange;
		}

		public void AddExperience(float amountOfExp)
		{
			if(xp < levelTable[PLAYER_LEVEL_MAX])
			{
				xp = Mathf.Min(xp + amountOfExp, levelTable[PLAYER_LEVEL_MAX]);

				// Calculate current level
				if((PLAYER_LEVEL_MAX > level) && (xp >= levelTable[level+1]))
					level++;
			}
		}

		public void AddKills(int killCount)
		{
			kills += killCount;
		}

		public double getChange()
		{
			return change;
		}
	}

	public class Enemy : BattleActor
	{
		private EnemyDefinition enemyDefinition;

		public Enemy(EnemyDefinition enemyDef) : base(enemyDef.name, enemyDef.weapon, enemyDef.health, enemyDef.speed, enemyDef.defense)
		{
			enemyDefinition = enemyDef;
		}

		public string getName()
		{
			return enemyDefinition.name;
		}

		public float getChangeValue()
		{
			return enemyDefinition.changeValue;
		}

		public float getExperienceValue()
		{
			return enemyDefinition.experienceValue;
		}
	}

	
	/// <summary>
	/// Battle round.
	/// </summary>
	public class BattleRound
	{
		public static Roll ToHitRoll = new Roll("d20", 1);
		
		public enum State
		{
			SelectAction,
			SelectTarget,
			Act,
			Finished
		}
		
		public State state {get;set;}
		public BattleActor actor {get;set;}
		public bool bIsPlayer {get;set;}
		public bool bRolledChanceToHit {get;set;}
		public bool bChanceToHitSuccess {get;set;}
		public int numberOfDamageDiceStillRolling {get;set;}
		public int rolledDamage {get;set;}
		public bool bActionSelected {get;set;}
		public bool bTargetSelected {get;set;}
		public bool bPlayerActed {get;set;}
		public BattleAction selectedAction {get;set;}
		public BattleActor targetedActor {get;set;}
		
		public BattleRound(BattleActor turnActor)
		{
			actor = turnActor;
			state = State.SelectAction;
			
			bIsPlayer = (turnActor.GetType() == typeof(Player));
			
			bRolledChanceToHit = false;
			bChanceToHitSuccess = false;
			numberOfDamageDiceStillRolling = 0;
			rolledDamage = 0;
			bActionSelected = false;
			bTargetSelected = false;
			bPlayerActed = false;
		}
	}
	
	// Public
	public Color hitTextColor;
	public Color missTextColor;
	public Color damageTextColor;

	// Static
	private static int BATTLE_QUEUE_MAX = 4;
	private static float NEXT_TURN_DELAY_AFTER_RESULT = 10;
	private static float THROW_DAMAGE_DELAY = 5;
	
	// Private
	Player playerCharacter;

	// Turn data
	List<BattleActor> enemies;
	Queue<BattleRound> queue;
	BattleRound currentTurn;
	int roundCount;
	Dictionary<string,int> battleActorTurnCounts;
	
	// GUI
	bool bGoToNextTurn;
	float nextTurnTimer;
	bool bThrowDamageRoll;
	float throwDamageTimer;
	
	// Current Battle
	float accruedExperience;
	float accruedChange;
	int killsThisBattle;


	//,- Unity

	void Awake ()
	{
		bGoToNextTurn = false;
		nextTurnTimer = 0;
		bThrowDamageRoll = false;
		throwDamageTimer = 0;

		// Current Battle
		accruedExperience = 0.0f;
		accruedChange = 0.0f;
		killsThisBattle = 0;

		enemies = new List<BattleActor>();
	}

	void Update()
	{
		if(bGoToNextTurn)
		{
			nextTurnTimer += Time.deltaTime;
			
			if(NEXT_TURN_DELAY_AFTER_RESULT < nextTurnTimer)
			{
				nextTurnTimer = 0;
				bGoToNextTurn = false;
				switchToNextTurnInQueue();
			}
		}
		
		if(bThrowDamageRoll)
		{
			throwDamageTimer += Time.deltaTime;
			
			if(THROW_DAMAGE_DELAY < throwDamageTimer)
			{
				throwDamageTimer = 0;
				bThrowDamageRoll = false;
				BattleActorActed();
			}
		}
	}

	void OnEnable()
	{

	}


	//,- Public APIs

	public List<BattleActor> getBattleEnemies()
	{
		return enemies;
	}

	public BattleRound getCurrentTurn()
	{
		return currentTurn;
	}

	public BattleActor getCurrentTurnActor()
	{
		if(null == currentTurn)
			return null;

		return currentTurn.actor;
	}

	public Queue<BattleRound> getBattleQueue()
	{
		return queue;
	}

	public BattleRound.State getBattleRoundState()
	{
		return currentTurn.state;
	}

	public void setBattleRoundState(BattleRound.State state)
	{
		currentTurn.state = state;
	}

	void GenerateBattle(bool bBossFight=false, int numberOfEnemies=2)
	{
		List<BattleActor> opponents = new List<BattleActor>();
		int numberOfEnemiesCreated = 0;

		if(true == bBossFight)
		{
			List<EnemyDefinition> possibleEnemies = Utilities().getBossPerLevel(playerCharacter.level);
			opponents.Add(new Enemy(possibleEnemies[Random.Range(0, possibleEnemies.Count)]));
		}
		else
		{
			List<EnemyDefinition> possibleEnemies = Utilities().getEnemiesPerLevel(playerCharacter.level);

			/* Easy-fight test code
			List<EnemyDefinition> possibleEnemies = new List<EnemyDefinition>();
			List<EnemyDefinition> allEnemies = Utilities().getEnemiesPerLevel(playerCharacter.level);

			foreach(EnemyDefinition enemyDef in allEnemies)
			{
				if(enemyDef.name == "Bunny")
					possibleEnemies.Add(enemyDef);
			}
			*/

			while(numberOfEnemies > numberOfEnemiesCreated)
			{
				EnemyDefinition enemyType = possibleEnemies[Random.Range(0, possibleEnemies.Count)];

				if(enemyType.level < playerCharacter.level)
				{
					opponents.Add(new Enemy(enemyType));
					opponents.Add(new Enemy(enemyType));
				}
				else
				{
					opponents.Add(new Enemy(enemyType));
				}

				numberOfEnemiesCreated++;
			}
		}

		// Set the enemies for the battle to use
		enemies = opponents;
	}

	/// <summary>
	/// Starts the battle.
	/// </summary>
	public void StartBattle()
	{
		//bool bBossFight = false;

		// Clear the title-screen dice
		Utilities().ClearDice();

		// Player Character
		playerCharacter = Utilities().getCurrentCharacter();

		/*
		Dictionary<string, int> enemyTypes = new Dictionary<string, int>();
		enemyTypes.Add("Stabby", 1);	// TODO: Don't have hard-coded enemies, here, unless the types were created here.
		enemyTypes.Add("Bathead", 1);
		CreateEnemies(enemyTypes);
		*/
		GenerateBattle();

		// Initialize the battleQueue
		InitializeQueue();
		
		Utilities().AppendBattleText(string.Format("{0} is engaged by some thugs...", playerCharacter.name));
		foreach(Enemy enemy in enemies)
		{
			Utilities().AppendBattleText(string.Format("\t{0}", enemy.name));
		}
		Utilities().AppendBattleText("Beat'em and make some ~change~!");
		
	}

	// Deprecated
	void CreateEnemies(Dictionary<string, int> enemyTypes)
	{
		enemies = new List<BattleActor>();
		foreach(string enemyName in enemyTypes.Keys)
		{
			// Create 'count' of 'enemyName' type of enemy-definition
			for(int count=0; count < enemyTypes[enemyName]; count++)
			{
				Debug.Log("getEnemy - deprecated");
 				//enemies.Add(new Enemy(Utilities().getEnemy(enemyName)));
			}
		}
	}

	// -- Battle Queue

	void InitializeQueue()
	{
		string enemyBaseName = "";
		int nameCount = 0;
		roundCount = 0;
		battleActorTurnCounts = new Dictionary<string, int>();
		battleActorTurnCounts.Add(playerCharacter.name, 1);
		foreach(BattleActor enemy in enemies)
		{
			nameCount = 2; // Because we don't number the first one
			enemyBaseName = enemy.name;
			while(battleActorTurnCounts.ContainsKey(enemy.name))
			{
				enemy.name = string.Format("{0} {1}", enemyBaseName, nameCount);
				nameCount++;
			}
			battleActorTurnCounts.Add(enemy.name, 1);
		}

		queue = new Queue<BattleRound>(); //Not in use, yet

		for(int i=0; i < BATTLE_QUEUE_MAX; i++)
		{
			CalculateTurn();
		}

		// Start the first turn of the battle
		NextTurn();
	}

	private void NextTurn()
	{
		bGoToNextTurn = true;
	}

	private void switchToNextTurnInQueue()
	{
		if(null == queue)
		{
			InitializeQueue();
		}

		if(0 == enemies.Count)
		{
			return;
		}

		BattleRound nextRound = queue.Dequeue();
		
		currentTurn = nextRound;
		
		for(int i=0; queue.Count != BATTLE_QUEUE_MAX; i++)
		{
			CalculateTurn();
		}
		
		Utilities().AppendBattleText(string.Format("--------- {0}'s turn,--------", currentTurn.actor.name));
		
		if(!currentTurn.bIsPlayer)
		{
			DoEnemyTurn();
		}
		else 
		{
			Utilities().TransitionBattleWindowsOn();
		}
	}
	
	/// <summary>
	/// Calculates the turn.
	/// </summary>
	/// <returns>
	/// The turn.
	/// </returns>
	private void CalculateTurn()
	{
		BattleActor actorWithInitiative = playerCharacter;
		
		foreach(BattleActor enemy in enemies)
		{
			// Compare the current actor with initiative to the enemy, select the min of the two
			if(battleActorTurnCounts[actorWithInitiative.name] * actorWithInitiative.getSpeed()
				> battleActorTurnCounts[enemy.name] * enemy.getSpeed())
			{
				actorWithInitiative = enemy;
			}
		}
		
		battleActorTurnCounts[actorWithInitiative.name]++;
		
		roundCount += actorWithInitiative.speed;
		
		// Enqueue this turn
		queue.Enqueue(new BattleRound(actorWithInitiative));
	}

	//,- Actions

	/// <summary>
	/// Dos the enemy turn.
	/// </summary>
	void DoEnemyTurn()
	{
		List<Attack> actions = new List<Attack>(currentTurn.actor.getActions());
		SelectedAction(actions[Random.Range(0,currentTurn.actor.getActions().Count)]);
		SelectedEnemy(playerCharacter);
		Utilities().AppendBattleText("Enemy Attacking");
		BattleActorActed();
	}
	
	/// <summary>
	/// Selecteds the action.
	/// </summary>
	/// <param name='actionName'>
	/// Action name.
	/// </param>
	public void SelectedAction(BattleAction action)
	{
		currentTurn.selectedAction = action;
		currentTurn.state = BattleRound.State.SelectTarget;
	}
	
	/// <summary>
	/// Selecteds the enemy.
	/// </summary>
	/// <param name='actor'>
	/// Actor.
	/// </param>
	public void SelectedEnemy(BattleActor actor)
	{
		currentTurn.targetedActor = actor;
		currentTurn.state = BattleRound.State.Act;
		
		Utilities().AppendBattleText(string.Format("{0} uses {1} on {2}",
						currentTurn.actor.name,
						currentTurn.selectedAction.name,
						currentTurn.targetedActor.name));
		
		BattleActorActed();
	}
	
	/// <summary>
	/// Players the acted.
	/// </summary>
	public void BattleActorActed()
	{
		Roll roll;

		if(false == currentTurn.bRolledChanceToHit)
		{
			roll = BattleRound.ToHitRoll;
		}
		else
		{
			roll = currentTurn.selectedAction.roll;
		}

		// Collect the dice into a list
		currentTurn.numberOfDamageDiceStillRolling = roll.count;

		Utilities().TransitionBattleWindowsOff();

		//Utilities().AppendBattleText(string.Format("Rolling {0} for {1}", roll.dieName, (true == currentTurn.bRolledChanceToHit ? "Damage" : "Chance to Hit" )));
		Utilities().ThrowDice(roll, true);
	}

	
	void StartTimerForThrowDamageRoll()
	{
		bThrowDamageRoll = true;
	}
	
	/// <summary>
	/// Dices the rolled.
	/// </summary>
	/// <param name='die'>
	/// Die.
	/// </param>
	/// <param name='rollValue'>
	/// Roll value.
	/// </param>
	public void DiceRolled(GameObject die, int rollValue)
	{
		//TODO: Magic Numbers

		if(Utilities().getGameState() != GameState.BattleMode)
		{
			Vector3 v = Camera.main.WorldToViewportPoint(die.transform.position);
			Utilities().SpawnPts(string.Format("{0}", rollValue), v.x, v.y, hitTextColor);
			return;
		}

		// The following logic is only for in the Act state
		if(null != currentTurn && BattleRound.State.Act == currentTurn.state)
		{
			currentTurn.numberOfDamageDiceStillRolling--;

			/************************** Chance To Hit ******************************/
			// If a die roll comes back, bRolledChanceToHit is false, and its a d20,
			//	==> this is the chance-to-hit result.
			if(!currentTurn.bRolledChanceToHit && die.name.Contains("d20"))
			{
				currentTurn.bRolledChanceToHit = true;
				int actorHitModifier = currentTurn.selectedAction.hitModifier;
				int targetDefenseModifier = currentTurn.targetedActor.getDefense();
				int hitValue = rollValue  + actorHitModifier + targetDefenseModifier;
				// Check if the rolled value was enough to hit the target (factoring in the action's hit modifier)
				if(hitValue >= 10)
				{
					// Create a "HIT" text
					Vector3 v = Camera.main.WorldToViewportPoint(die.transform.position);

					v.x -= 0.15f;
					Utilities().SpawnPts("Hit", v.x, v.y, hitTextColor);

					v.x -= 0.5f;
					v.y += 1;
					DoBattleModText(actorHitModifier, v, hitTextColor);
					v.x -= 0.5f;
					v.y += 1;
					DoBattleModText(targetDefenseModifier, v, damageTextColor);
					
					// Describe the miss
					Utilities().AppendBattleText(string.Format("Hit! : Roll:{0} + Weapon:{1} + {2}'s Def:{3} = {4} >= 10",
					                             rollValue, actorHitModifier, currentTurn.targetedActor.name, targetDefenseModifier, hitValue));

					// Hit was successful, roll for damage
					currentTurn.bChanceToHitSuccess = true;
					currentTurn.bPlayerActed = false;

					// Cue the damage roll
					StartTimerForThrowDamageRoll();
				}
				else
				{
					// Create a "Miss" text
					Vector3 v = Camera.main.WorldToViewportPoint(die.transform.position);

					Utilities().SpawnPts("Miss", v.x - 0.15f, v.y, missTextColor);

					Utilities().AppendBattleText(string.Format("Missed! : Roll:{0} + Weapon:{1} - Target-Def:{2} < 10",
					                                           rollValue, actorHitModifier, targetDefenseModifier));

					// Hit was unsuccessful, end turn
					FinishTurn();
				}
			}
			/************************** Damage *************************************/
			else if(true == currentTurn.bChanceToHitSuccess)
			{
				int damageAmount = rollValue;
				int weaponDamageMod = currentTurn.actor.weapon.damageModifier;
				int attackDamageMod = (currentTurn.selectedAction as Attack).damageModifier;
				string battleTextstring = string.Format("roll:{0}", rollValue);

				damageAmount += weaponDamageMod;
				if(0 < weaponDamageMod)
					string.Concat(string.Format("weapon-modifier:{0}", weaponDamageMod));

				damageAmount += attackDamageMod;
				if(0 < attackDamageMod)
					string.Concat(string.Format("attack-modifier:{0}", attackDamageMod));

				Vector3 v = Camera.main.WorldToViewportPoint(die.transform.position);

				v.x -= 0.25f;
				Utilities().SpawnPts(rollValue.ToString(), v.x, v.y, damageTextColor);
				v.x -= 0.5f;
				v.y += 1;
				DoBattleModText(weaponDamageMod, v, damageTextColor);
				v.x -= 0.5f;
				v.y += 1;
				DoBattleModText(attackDamageMod, v, damageTextColor);

				currentTurn.rolledDamage = Mathf.Max(1,damageAmount);

				Utilities().AppendBattleText(string.Format("{0}, Damage to {1}",
				                                          	battleTextstring, currentTurn.targetedActor.name));

				if(0 == currentTurn.numberOfDamageDiceStillRolling)
				{
					currentTurn.targetedActor.addDamage(currentTurn.rolledDamage);
					FinishTurn();
				}
			}
		}
	}

	void DoBattleModText(int value, Vector2 v, Color color)
	{
		if(0 != value)
		{
			Utilities().SpawnPts(string.Format("{0}", (value > 0 ? "+"+value.ToString() : value.ToString())), v.x, v.y, color);
		}
	}
		
	void AccrueRewards(float change, float exp, int kills)
	{
		accruedChange += change;
		accruedExperience += exp;
		killsThisBattle += kills;
	}

	
	/// <summary>
	/// Finishs the turn.
	/// </summary>
	void FinishTurn()
	{	
		if(0 >= playerCharacter.remainingHealth)
		{
			// Gameover man...
			BattleOver(false);
		}
		
		List<BattleActor> killed = new List<BattleActor>();
		foreach(Enemy enemy in enemies)
		{
			if(0 >= enemy.remainingHealth)
			{
				// Add the rewards to the battle totals, to be awarded at the end
				AccrueRewards(enemy.getChangeValue(), enemy.getExperienceValue(), 1);
				killed.Add(enemy); // Must delete after this loop or the change in the collection will cause an error
			}
		}
		foreach(Enemy killedEnemy in killed)
		{
			enemies.Remove(killedEnemy);

			List<BattleRound> deletedRounds = new List<BattleRound>();
			foreach(BattleRound round in queue)
			{
				if(killedEnemy == round.actor)
				{
					deletedRounds.Add(round);
				}
			}
			List<BattleRound> currentQueue = new List<BattleRound>(queue.ToArray());
			
			foreach(BattleRound round in deletedRounds)
			{
				currentQueue.Remove(round);
			}
			queue = new Queue<BattleRound>(currentQueue);
		}
		
		if(0 == enemies.Count)
		{
			BattleOver(true);
		}
		else
		{
			NextTurn();
		}
		
	}

	void BattleOver(bool bPlayerIsWinner)
	{
		string battleOverText;

		if(true == bPlayerIsWinner)
		{
			battleOverText = "Battle Won!";
			Utilities().PlayerIsVictorious(accruedExperience, accruedChange, killsThisBattle);
		}
		else
		{
			battleOverText = "Battle Lost";
			Utilities().EnemyIsVictorious();
		}

		Utilities().AppendBattleText(string.Format("----------------{0}----------------", battleOverText));

		Utilities().setGameState(GameState.BattleOver);

		Utilities().ClearDice();
	}

	// This is just for testing the battle-over screen without going through a battle
	public void BattleOverTest(bool bPlayerIsWinner)
	{
		AccrueRewards(3, 10, 5);

		BattleOver(bPlayerIsWinner);
	}
	
	UtilitiesScript utilitiesScript;
	
	UtilitiesScript Utilities()
	{
		if(null == utilitiesScript)
			utilitiesScript = GameObject.Find("Utilities").GetComponent<UtilitiesScript>();
		
		return utilitiesScript;
	}
}
