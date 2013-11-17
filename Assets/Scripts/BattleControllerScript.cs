using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using GameState = GameControllerScript.GameState;
using BattleActorDefinition = GameControllerScript.BattleActorDefinition;
using EnemyDefinition = GameControllerScript.EnemyDefinition;
using BattleAction = GameControllerScript.BattleAction;
using Attack = GameControllerScript.Attack;
using Weapon = GameControllerScript.Weapon;

using Roll = DiceControllerScript.Roll;

public class BattleControllerScript : MonoBehaviour {

	public class BattleActor
	{
		public string name {get;set;}
		public int remainingHealth {get;set;}
		public Weapon weapon {get;set;}
		BattleActorDefinition definition {get;set;}

		public void addDamage(int dmg)
		{
			remainingHealth -= dmg;
		}

		public BattleActor(BattleActorDefinition actorDefinition)
		{
			name = actorDefinition.name;
			remainingHealth = actorDefinition.health;
			weapon = actorDefinition.weapon;
			definition = actorDefinition;
		}

		public void AddWeapon(Weapon theWeapon)
		{
			// Set this as the current weapon
			weapon = theWeapon; // Accept the change
		}

		public int getSpeed()
		{
			return definition.speed;
		}

		public int getDefense()
		{
			return definition.defense;
		}

		public List<Attack> getActions()
		{
			return new List<Attack>(weapon.attacks.Values);
		}
	}

	public class Player : BattleActor
	{
		public string type {get;set;}
		public double change {get;set;}
		public double xp {get;set;}
		public int level {get;set;}
		public int kills {get;set;}

		public Player(BattleActorDefinition actorDefinition, string theName) : base(actorDefinition)
		{
			name = theName;
			type = actorDefinition.name;
			change = 0;
			xp = 0;
			kills = 0;
		}

		public void AddChange(double amountOfChange)
		{
			change += amountOfChange;
		}

		public void AddExperience(double amountOfExp)
		{
			xp += amountOfExp;
		}

		public void AddKills(int killCount)
		{
			kills += killCount;
		}
	}

	public class Enemy : BattleActor
	{
		public EnemyDefinition enemyDefinition {get;set;}

		public Enemy(EnemyDefinition enemyDef) : base(enemyDef.definition)
		{
			enemyDefinition = enemyDef;
		}

		public string getName()
		{
			return enemyDefinition.name;
		}

		public double getChangeValue()
		{
			return enemyDefinition.changeValue;
		}

		public double getExperienceValue()
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
	double accruedExperience;
	double accruedChange;
	int killsThisBattle;


	// -- Unity

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


	// -- Public APIs

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


	/// <summary>
	/// Starts the battle.
	/// </summary>
	public void StartBattle()
	{
		// Player Character
		playerCharacter = Utilities().getCurrentCharacter();

		Dictionary<string, int> enemyTypes = new Dictionary<string, int>();
		enemyTypes.Add("Stabby", 1);	// TODO: Don't have hard-coded enemies, here, unless the types were created here.
		enemyTypes.Add("Bathead", 1);
		CreateEnemies(enemyTypes);

		// Initialize the battleQueue
		InitializeQueue();
		
		Utilities().AppendBattleText(string.Format("{0} is engaged by some thugs...", playerCharacter.name));
		foreach(Enemy enemy in enemies)
		{
			Utilities().AppendBattleText(string.Format("\t{0}", enemy.name));
		}
		Utilities().AppendBattleText("Beat'em and make some ~change~!");
		
	}

	void CreateEnemies(Dictionary<string, int> enemyTypes)
	{
		enemies = new List<BattleActor>();
		foreach(string enemyName in enemyTypes.Keys)
		{
			// Create 'count' of 'enemyName' type of enemy-definition
			for(int count=0; count < enemyTypes[enemyName]; count++)
			{
				enemies.Add(new Enemy(Utilities().getEnemy(enemyName)));
			}
		}
	}

	// -- Battle Queue

	void InitializeQueue()
	{
		roundCount = 0;
		battleActorTurnCounts = new Dictionary<string, int>();
		battleActorTurnCounts.Add(playerCharacter.name, 1);
		foreach(BattleActor enemy in enemies)
		{
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

		BattleRound nextRound = queue.Dequeue();
		
		currentTurn = nextRound;
		
		for(int i=0; queue.Count != BATTLE_QUEUE_MAX; i++)
		{
			CalculateTurn();
		}
		
		Utilities().AppendBattleText(string.Format("--------- {0}'s turn ---------", currentTurn.actor.name));
		
		if(!currentTurn.bIsPlayer)
		{
			DoEnemyTurn();
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
			if(battleActorTurnCounts[actorWithInitiative.name] * actorWithInitiative.getSpeed() >
				battleActorTurnCounts[enemy.name] * enemy.getSpeed())
			{
				actorWithInitiative = enemy;
			}
		}
		
		battleActorTurnCounts[actorWithInitiative.name]++;
		
		roundCount += actorWithInitiative.getSpeed();
		
		// Enqueue this turn
		queue.Enqueue(new BattleRound(actorWithInitiative));
	}

	// -- Actions

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

		Utilities().AppendBattleText(string.Format("Rolling {0} for {1}", roll.dieName, (true == currentTurn.bRolledChanceToHit ? "Damage" : "Chance to Hit" )));
		Utilities().ThrowDice(roll);
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
		// The following logic is only for in the Act state
		if(null != currentTurn && BattleRound.State.Act == currentTurn.state)
		{
			currentTurn.numberOfDamageDiceStillRolling--;
			
			// If a die roll comes back, bRolledChanceToHit is false, and its a d20,
			//	==> this is the chance-to-hit result.
			if(!currentTurn.bRolledChanceToHit && die.name.Contains("d20"))
			{
				currentTurn.bRolledChanceToHit = true;
				
				// Check if the rolled value was enough to hit the target
				if(rollValue > 10)
				{
					// TODO: Create a "HIT" text
					Vector3 v = Camera.main.WorldToViewportPoint(die.transform.position);
					Utilities().SpawnPts("Hit", v.x - 0.25f, v.y, hitTextColor); // 100 points picked
					
					// Describe the miss
					Utilities().AppendBattleText(string.Format("{0} > 10 - Hit!", rollValue));
					// Hit was successful, roll for damage
					currentTurn.bChanceToHitSuccess = true;
					currentTurn.bPlayerActed = false;
					
					StartTimerForThrowDamageRoll();
				}
				else
				{
					// TODO: Create a "Miss" text
					Vector3 v = Camera.main.WorldToViewportPoint(die.transform.position);
					Utilities().SpawnPts("Miss", v.x - 0.25f, v.y, missTextColor); // 100 points picked
					
					// Hit was unsuccessful, end turn
					FinishTurn();
				}
			}
			else if(true == currentTurn.bChanceToHitSuccess)
			{
				Vector3 v = Camera.main.WorldToViewportPoint(die.transform.position);
				Utilities().SpawnPts(rollValue.ToString(), v.x - 0.25f, v.y, damageTextColor); // 100 points picked
				currentTurn.rolledDamage += rollValue;
				
				if(0 == currentTurn.numberOfDamageDiceStillRolling)
				{
					FinishTurn();
				}
			}
		}
	}
		
	void AccrueRewards(double change, double exp, int kills)
	{
		accruedChange += change;
		accruedExperience += exp;
		killsThisBattle += kills;
	}
	
	public void AwardRewards()
	{
		playerCharacter.AddExperience(accruedExperience);
		playerCharacter.AddChange(accruedChange);
		playerCharacter.AddKills(killsThisBattle);
		Utilities().UpdatePlayer(playerCharacter.name,
								playerCharacter.change,
								playerCharacter.xp,
								playerCharacter.kills,
								playerCharacter.level,
								playerCharacter.weapon.name,
								playerCharacter.type);
	}
	
	/// <summary>
	/// Finishs the turn.
	/// </summary>
	void FinishTurn()
	{
		if((currentTurn.bRolledChanceToHit) && (currentTurn.bChanceToHitSuccess))
		{
			currentTurn.targetedActor.remainingHealth -= currentTurn.rolledDamage;
			
			if(0 >= playerCharacter.remainingHealth)
			{
				// Gameover man...
				Utilities().setGameState(GameState.BattleOver);
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
				Utilities().setGameState(GameState.BattleOver);
				AwardRewards();
			}
			
			Utilities().AppendBattleText(string.Format("{0} Damage to {1}", currentTurn.rolledDamage, currentTurn.targetedActor.name));
		}
		else
		{
			Utilities().AppendBattleText("Missed");
		}
		
		NextTurn();
	}

	
	UtilitiesScript utilitiesScript;
	
	UtilitiesScript Utilities()
	{
		if(null == utilitiesScript)
			utilitiesScript = GameObject.Find("Utilities").GetComponent<UtilitiesScript>();
		
		return utilitiesScript;
	}
}
