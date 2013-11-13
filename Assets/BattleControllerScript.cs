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
		public int remainingHealth {get;set;}
		public int speed {get;set;}
		public int defense {get;set;}
		public Weapon weapon {get;set;}
		public double experienceValue {get;set;}
		public double changeValue {get;set;}
		
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
		
		public BattleActor(string theName, int theHealth, int theSpeed, int theDefense, Weapon theWeapon,
							double expVal, double changeVal)
		{
			name = theName;
			health = theHealth;
			remainingHealth = health;
			speed = theSpeed;
			defense = theDefense;
			weapon = theWeapon;
			experienceValue = expVal;
			changeValue = changeVal;
			
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
		: base(theName, theHealth, theSpeed, theDefense, theWeapon, 0, 0)
		{
			inventory = new Inventory();
			inventory.AddWeapon(theWeapon);
		}
		
		public void AddChange(double amountOfChange)
		{
			inventory.change += amountOfChange;
		}
		
		public void AddExperience(double amountOfExp)
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
		public List<GameObject> dice {get;set;}
		public bool bActionSelected {get;set;}
		public bool bTargetSelected {get;set;}
		public bool bPlayerActed {get;set;}
		public BattleAction selectedAction {get;set;}
		public BattleActor targetedActor {get;set;}
		
		public BattleRound(BattleActor turnActor)
		{
			actor = turnActor;
			state = State.SelectAction;
			
			bIsPlayer = (turnActor.GetType() == typeof(Character));
			
			bRolledChanceToHit = false;
			bChanceToHitSuccess = false;
			numberOfDamageDiceStillRolling = 0;
			rolledDamage = 0;
			bActionSelected = false;
			bTargetSelected = false;
			bPlayerActed = false;
		}
		
		public void ClearDice()
		{
			if(null != dice)
			{
				foreach(GameObject die in dice)
				{
					GameObject.Destroy(die);
				}
			}
			
			dice = new List<GameObject>();
			numberOfDamageDiceStillRolling = 0;
			rolledDamage = 0;
			
			Camera.main.GetComponent<CamControl>().ResetCamera();
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
	public Color hitTextColor;
	public Color missTextColor;
	public Color damageTextColor;
	public GUIText BattleTextPrefab;

	// Static
	private static int GUI_SCREEN_BORDER = 15;
	private static int GUI_TEXTFIELD_PAD = 25;
	private static int GUI_BATTLESTAT_PAD = 20;
	private static int BATTLE_QUEUE_MAX = 4;
	private static float NEXT_TURN_DELAY_AFTER_RESULT = 10;
	private static float THROW_DAMAGE_DELAY = 5;
	
	// Private
	List<BattleActor> enemies;
	Character playerCharacter;
	Dictionary<string,Weapon> weapons;
	
	// State of the entire game
	GameState gameState;
	
	// Turn data
	Queue<BattleRound> queue;
	BattleRound currentTurn;
	int roundCount;
	Dictionary<string,int> battleActorTurnCounts;
	
	// GUI
	int dieCount = 0;
	string battleText;
	Vector2 scrollPosition;
	bool bGoToNextTurn = false;
	float nextTurnTimer = 0;
	bool bThrowDamageRoll = false;
	float throwDamageTimer = 0;
	
	// Current Battle
	double accruedExperience = 0.0f;
	double accruedChange = 0.0f;
	
	void Update()
	{
		if(bGoToNextTurn)
		{
			Debug.Log("Next-Turn-Delay Tick");
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
			Debug.Log("Next-Turn-Delay Tick");
			throwDamageTimer += Time.deltaTime;
			
			if(THROW_DAMAGE_DELAY < throwDamageTimer)
			{
				throwDamageTimer = 0;
				bThrowDamageRoll = false;
				PlayerActed();
			}
		}
	}
	
	void Awake ()
	{
		gameState = GameState.Title;
		battleText = "";
		
		bGoToNextTurn = false;
		nextTurnTimer = 0;
		bThrowDamageRoll = false;
		throwDamageTimer = 0;
	
		// Current Battle
		accruedExperience = 0.0f;
		accruedChange = 0.0f;
	}
	
	void OnEnable ()
	{
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
	
	
	void GUI_TitleScreen()
	{
		GUILayout.BeginVertical();
		GUILayout.Space(100);
		GUILayout.Box("Spare Change", "TitleBox"); 		//TODO: Need a custom style for the title
		//GUILayout.FlexibleSpace();
		if(GUILayout.Button("Start a New Guy")) 	// TODO: Need a custom style for the menu items
		{
			//TODO: Do some kind of transition to the next UI state
			gameState = GameState.CharacterSelection;
		}
		GUI.enabled = false;
		if(GUILayout.Button("Load Existing")) 	// TODO: Need a custom style for the menu items
		{
			//TODO: Do some kind of transition to the next UI state
			gameState = GameState.CharacterSelection;
		}
		GUI.enabled = true;
		GUILayout.EndVertical();
	}
	
	void GUI_CharacterSelection()
	{
		GUILayout.BeginHorizontal();
		
		
		GUILayout.BeginVertical();
		GUILayout.Space(20);
		GUILayout.Box("Melee");
		//TODO: Display an image below the title
		GUILayout.Box("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
		if(GUILayout.Button("Select"))
		{
			SelectCharacter(Character.Type.Fighter);
		}
		GUILayout.Space(20);
		GUILayout.EndVertical();
		
		GUILayout.BeginVertical();
		GUILayout.Space(20);
		GUILayout.Box("Ranged");
		//TODO: Display an image below the title
		GUILayout.Box("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
		if(GUILayout.Button("Select"))
		{
			SelectCharacter(Character.Type.Shooter);
		}
		GUILayout.Space(20);
		GUILayout.EndVertical();
		
		
		GUILayout.EndHorizontal();
	}
	
	void GUI_BattleStart()
	{
		// Setup the queue for the next
		GUILayout.BeginVertical();
		////
		GUILayout.FlexibleSpace();
		////
		GUILayout.BeginHorizontal();
		foreach(BattleActor enemy in enemies)
		{
			GUILayout.Box(string.Format("{0}", enemy.name));
		}
		GUILayout.EndHorizontal();
		////
		GUILayout.FlexibleSpace();
		////
		GUILayout.BeginHorizontal();
		////
		GUILayout.FlexibleSpace();
		////
		if(GUILayout.Button("Start Battle"))
		{
			StartBattle();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		////
		GUILayout.FlexibleSpace();
		////
		GUILayout.BeginHorizontal();
		GUILayout.Box(string.Format("{0}", playerCharacter.name));
		GUILayout.EndHorizontal();
		////
		GUILayout.FlexibleSpace();
		////
		GUILayout.EndVertical();
	}
	
	void GUI_BattleMode_enemyTurn()
	{
		GUILayout.Box(string.Format("{0}'s turn", currentTurn.actor.name));
		////
		GUILayout.FlexibleSpace();
		////
	}
	
	void GUI_BattleMode_playerSelectAction()
	{
		GUILayout.Box("Select an Action");
		////
		GUILayout.FlexibleSpace();
		////
		GUILayout.BeginHorizontal();
		GUI.enabled = !currentTurn.bActionSelected;
		foreach(BattleAction action in currentTurn.actor.actions.Values)
		{
			if(GUILayout.Button(string.Format("{0}", action.name)))
			{
				currentTurn.bActionSelected = true;
				SelectedAction(action.name);
			}
		}
		GUI.enabled = true;
		GUILayout.EndHorizontal();
		////
		GUILayout.FlexibleSpace();
		////
	}
	
	void GUI_BattleMode_playerSelectTarget()
	{
		GUILayout.Box("Select a Target");
		////
		GUILayout.FlexibleSpace();
		////
		GUILayout.BeginHorizontal();
		GUI.enabled = !currentTurn.bTargetSelected;
		foreach(BattleActor enemy in enemies)
		{
			if(GUILayout.Button(string.Format("{0}", enemy.name)))
			{
				currentTurn.bTargetSelected = true;
				// The action chosen will have an impact on who the possible targets can be
				SelectedEnemy(enemy);
			}
		}
		if(GUILayout.Button("Back"))
		{
			currentTurn.bActionSelected = false;
			currentTurn.state = BattleRound.State.SelectAction;
		}
		GUI.enabled = true;
		GUILayout.EndHorizontal();
		////
		GUILayout.FlexibleSpace();
		////
	}
	
	void GUI_BattleMode_playerAct()
	{
		GUILayout.BeginHorizontal();
		////
		GUILayout.FlexibleSpace();
		////
		GUI.enabled = !currentTurn.bPlayerActed;
		GUILayout.Box(string.Format("Rolling {0}", (false == currentTurn.bRolledChanceToHit ? "-Chance to Hit-" : "-Damage-")));
		GUI.enabled = true;
		////
		GUILayout.FlexibleSpace();
		////
		GUILayout.EndHorizontal();
	}
	
	void GUI_BattleText()
	{
		GUILayout.BeginHorizontal();
		
		GUILayout.Space(GUI_TEXTFIELD_PAD);
		
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		GUILayout.Label(string.Format("{0}", battleText));
		GUILayout.EndScrollView();
		
		GUILayout.Space(GUI_TEXTFIELD_PAD);
		
		GUILayout.EndHorizontal();
	}
	
	void GUI_BattleQueue()
	{
		GUILayout.BeginVertical();
		
		if(null != currentTurn)
		{
			GUILayout.Box(string.Format("{0}", currentTurn.actor.name[0]), "TitleBox");
			foreach(BattleRound round in queue)
			{
				GUILayout.Box(string.Format("{0}", round.actor.name[0]), "TitleBox");
			}
		}
		
		GUILayout.EndVertical();
	}
	
	void GUI_BattleStat(BattleActor actor)
	{
		// Frame
		GUILayout.Box("", "FrameBox");
		// Meter
		Rect tempRect = GUILayoutUtility.GetLastRect();
		tempRect.x += GUI_BATTLESTAT_PAD;
		tempRect.width -= GUI_BATTLESTAT_PAD*2;
		tempRect.y += GUI_BATTLESTAT_PAD;
		tempRect.height -= GUI_BATTLESTAT_PAD*2;
		GUI.Box(tempRect, "", "MeterBox");
		// Cutout
		GUI.Box(tempRect, "", "CutoutBox");
		// Name
		GUI.Box(tempRect, actor.name);
	}
	
	void GUI_BattleStats()
	{
		GUILayout.BeginVertical();
		
		foreach(BattleActor enemy in enemies)
		{
			GUI_BattleStat(enemy);
		}
		
		GUILayout.Space(10);
		
		GUI_BattleStat(playerCharacter);
		
		GUILayout.EndVertical();
	}
	
	void GUI_BattleMode()
	{
		GUILayout.BeginVertical();
		GUI_BattleText();
		
		
		GUILayout.BeginHorizontal();
		////
		GUI_BattleStats();
		GUILayout.Space(Screen.width*0.5f);
		GUI_BattleQueue();
		////
		GUILayout.EndHorizontal();
		
		if(null != currentTurn)
		{
			if(true == currentTurn.bIsPlayer)
			{
				switch(currentTurn.state)
				{
				case BattleRound.State.SelectAction:
					GUI_BattleMode_playerSelectAction();
					break;
				
				case BattleRound.State.SelectTarget:
					GUI_BattleMode_playerSelectTarget();
					break;
					
				case BattleRound.State.Act:
					GUI_BattleMode_playerAct();
					break;
				}
			}
			else
			{
				GUI_BattleMode_enemyTurn();
			}
		}
		else
		{
			GUILayout.Label("Queueing Next Turn");
		}
		
		GUILayout.EndVertical();
	}
	
	void GUI_BattleOver()
	{
		GUILayout.BeginVertical();
		GUILayout.Box("Battle Over", "TitleBox"); 		//TODO: Need a custom style for the title
		GUILayout.Box(string.Format("{0} is the victor",
									(playerCharacter.remainingHealth > 0 ? playerCharacter.name : "The Enemy")));
		GUILayout.EndVertical();
	}
	
	/// <summary>
	/// Raises the GU event.
	/// </summary>
	void OnGUI()
	{
		GUI.skin = guiSkin;
		////
		GUILayout.BeginArea(new Rect(0 + GUI_SCREEN_BORDER,
									 0 + GUI_SCREEN_BORDER,
									 Screen.width - GUI_SCREEN_BORDER,
									 Screen.height - GUI_SCREEN_BORDER));
		{
			switch(gameState)
			{
			case GameState.Title:
				GUI_TitleScreen();
				break;
				
			case GameState.CharacterSelection:
				GUI_CharacterSelection();
				break;
				
			case GameState.BattleStart:
				GUI_BattleStart();
				break;
			
			case GameState.BattleMode:
				GUI_BattleMode();
				break;
				
			case GameState.BattleOver:
				GUI_BattleOver();
				break;
			}
		}	
		GUILayout.EndArea();
	}
	
	/// <summary>
	/// Selects the character.
	/// </summary>
	/// <param name='type'>
	/// Type.
	/// </param>
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
	
	/// <summary>
	/// Creates the enemies.
	/// </summary>
	private void CreateEnemies(/*TODO: options*/)
	{
		enemies = new List<BattleActor>();
		// BatHead //
		enemies.Add(new BattleActor("BatHead"/*Name*/,
									1/*Health*/,
									8/*Speed*/,
									6/*Defense*/,
									weapons["bat"]/*Weapon*/,
									5,
									15));
		// Stabby //
		enemies.Add(new BattleActor("Stabby"/*Name*/,
									14/*Health*/,
									6/*Speed*/,
									4/*Defense*/,
									weapons["knife"]/*Weapon*/,
									4,
									10));
	}
	
	/// <summary>
	/// Starts the battle.
	/// </summary>
	private void StartBattle()
	{
		Debug.Log("Starting Battle Mode");

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
		
		// Switch the gamestate
		gameState = GameState.BattleMode;
		
		AppendBattleText(string.Format("{0} is engaged by some thugs...", playerCharacter.name));
		foreach(BattleActor enemy in enemies)
		{
			AppendBattleText(string.Format("\t{0}", enemy.name));
		}
		AppendBattleText("Beat'em and make some ~change~!");
		
	}

			
	private void NextTurn()
	{
		bGoToNextTurn = true;
	}
	
	private void switchToNextTurnInQueue()
	{
		BattleRound nextRound = queue.Dequeue();
		
		if(null != currentTurn)
			nextRound.dice = currentTurn.dice; // ....so that they are deleted when appropriate, later
		
		currentTurn = nextRound;
		
		for(int i=0; queue.Count != BATTLE_QUEUE_MAX; i++)
		{
			CalculateTurn();
		}
		
		AppendBattleText(string.Format("--------- {0}'s turn ---------", currentTurn.actor.name));
		
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
			if(battleActorTurnCounts[actorWithInitiative.name] * actorWithInitiative.speed >
				battleActorTurnCounts[enemy.name] * enemy.speed)
			{
				actorWithInitiative = enemy;
			}
		}
		
		battleActorTurnCounts[actorWithInitiative.name]++;
		
		roundCount += actorWithInitiative.speed;
		
		// Enqueue this turn
		queue.Enqueue(new BattleRound(actorWithInitiative));
	}
	
	/// <summary>
	/// Dos the enemy turn.
	/// </summary>
	void DoEnemyTurn()
	{
		List<string> keys = new List<string>(currentTurn.actor.actions.Keys);
		SelectedAction(keys[Random.Range(0,currentTurn.actor.actions.Count)]);
		SelectedEnemy(playerCharacter);
		AppendBattleText("Enemy Attacking");
		PlayerActed();
		AppendBattleText("Acted");
	}
	
	/// <summary>
	/// Selecteds the action.
	/// </summary>
	/// <param name='actionName'>
	/// Action name.
	/// </param>
	void SelectedAction(string actionName)
	{
		currentTurn.selectedAction = currentTurn.actor.actions[actionName];
		currentTurn.state = BattleRound.State.SelectTarget;
	}
	
	/// <summary>
	/// Selecteds the enemy.
	/// </summary>
	/// <param name='actor'>
	/// Actor.
	/// </param>
	void SelectedEnemy(BattleActor actor)
	{
		currentTurn.targetedActor = actor;
		currentTurn.state = BattleRound.State.Act;
		
		AppendBattleText(string.Format("{0} uses {1} on {2}",
						currentTurn.actor.name,
						currentTurn.selectedAction.name,
						currentTurn.targetedActor.name));
		
		PlayerActed();
	}
	
	/// <summary>
	/// Players the acted.
	/// </summary>
	void PlayerActed()
	{
		if(false == currentTurn.bRolledChanceToHit)
		{
			ThrowDice(BattleRound.ToHitRoll);
		}
		else if(true == currentTurn.bChanceToHitSuccess)
		{
			ThrowDice(currentTurn.selectedAction.roll);
		}
	}
	
	/// <summary>
	/// Throws the dice.
	/// </summary>
	/// <param name='roll'>
	/// Roll.
	/// </param>
	/// <param name='diceBox'>
	/// Dice box.
	/// </param>
	void ThrowDice(Roll roll)
	{
		//TODO: Many a magic number in this function
		
		Transform diceBox = Utilities().getDiceBox().transform;
		
		currentTurn.ClearDice();
		
		// Calculate the start position from the position of the box
		Vector3 startPosition = new Vector3(diceBox.position.x + diceBox.localScale.x*4,
											diceBox.position.y + 50,
											diceBox.position.z + diceBox.localScale.z*4);
		
		for(int d=0; d < roll.count; d++)
		{
			GameObject die = CreateDie(roll.dieName);
			AppendBattleText(string.Format("Rolling {0} for {1}", roll.dieName, (true == currentTurn.bRolledChanceToHit ? "Damage" : "Chance to Hit" )));
			
			// Position
			die.transform.position = startPosition;
			die.rigidbody.velocity = diceBox.right * 15 + diceBox.forward * -30;
			
			// Orientation
			die.transform.rotation = Quaternion.LookRotation(Random.onUnitSphere);
			die.rigidbody.angularVelocity = Vector3.right * -15;
			
			// Collect the dice into a list
			currentTurn.dice.Add(die);
			currentTurn.numberOfDamageDiceStillRolling++;
			
			// Position subsequent dice adjacently
			startPosition = startPosition + new Vector3(die.transform.localScale.x + 1, 0, 0);
		}
		
		Camera.main.GetComponent<CamControl>().LookAtDice(currentTurn.dice);
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
		Debug.Log("Got Dice Value Report");
		// The following logic is only for in the Act state
		if(BattleRound.State.Act == currentTurn.state)
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
					SpawnPts("Hit", v.x, v.y, hitTextColor); // 100 points picked
					
					// Describe the miss
					AppendBattleText(string.Format("{0} > 10 - Hit!", rollValue));
					// Hit was successful, roll for damage
					currentTurn.bChanceToHitSuccess = true;
					currentTurn.bPlayerActed = false;
					
					StartTimerForThrowDamageRoll();
				}
				else
				{
					// TODO: Create a "Miss" text
					Vector3 v = Camera.main.WorldToViewportPoint(die.transform.position);
					SpawnPts("Miss", v.x, v.y, missTextColor); // 100 points picked
					
					// Hit was unsuccessful, end turn
					FinishTurn();
				}
			}
			else if(true == currentTurn.bChanceToHitSuccess)
			{
				Vector3 v = Camera.main.WorldToViewportPoint(die.transform.position);
				SpawnPts(rollValue.ToString(), v.x, v.y, missTextColor); // 100 points picked
				currentTurn.rolledDamage += rollValue;
				
				if(0 == currentTurn.numberOfDamageDiceStillRolling)
				{
					FinishTurn();
				}
			}
		}
	}
	
	void SpawnPts(string text, float x, float y, Color color)
	{
	    x = Mathf.Clamp(x, 0.05f, 0.95f); // clamp position to screen to ensure
	    y = Mathf.Clamp(y, 0.05f, 0.9f);  // the string will be visible
	    GUIText gui = Instantiate(BattleTextPrefab, new Vector3(x,y,0), Quaternion.identity) as GUIText;
	    gui.guiText.text = text;
		gui.guiText.material.color = color; // set text color
	}
		
	void AccrueRewards(double change, double exp)
	{
		accruedChange += change;
		accruedExperience += exp;
	}
	
	void AwardRewards()
	{
		playerCharacter.AddExperience(accruedExperience);
		playerCharacter.AddChange(accruedChange);
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
				gameState = GameState.BattleOver;
			}
			
			List<BattleActor> killed = new List<BattleActor>();
			foreach(BattleActor enemy in enemies)
			{
				if(0 >= enemy.remainingHealth)
				{
					// Add the rewards to the battle totals, to be awarded at the end
					AccrueRewards(enemy.changeValue, enemy.experienceValue);
					killed.Add(enemy); // Must delete after this loop or the change in the collection will cause an error
				}
			}
			foreach(BattleActor killedEnemy in killed)
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
				gameState = GameState.BattleOver;
			}
			
			AppendBattleText(string.Format("{0} Damage to {1}", currentTurn.rolledDamage, currentTurn.targetedActor.name));
		}
		else
		{
			AppendBattleText("Missed");
		}
		
		NextTurn();
	}
	
	/// <summary>
	/// Creates the die.
	/// </summary>
	/// <returns>
	/// The die.
	/// </returns>
	/// <param name='dieName'>
	/// Die name.
	/// </param>
	GameObject CreateDie(string dieName)
	{
		if(dieName.Equals("d4")) return InstantiateDie(d4);
		else if(dieName.Equals("d6")) return InstantiateDie(d6_s_d);
		else if(dieName.Equals("d8")) return InstantiateDie(d8);
		else if(dieName.Equals("d10")) return InstantiateDie(d10);
		else if(dieName.Equals("d12")) return InstantiateDie(d12);
		else if(dieName.Equals("d20")) return InstantiateDie(d20);
		else if(dieName.Equals("d100")) return InstantiateDie(d100);
		else return null;
	}
	
	/// <summary>
	/// Instantiates the die.
	/// </summary>
	/// <returns>
	/// The die.
	/// </returns>
	/// <param name='diePrefab'>
	/// Die prefab.
	/// </param>
	GameObject InstantiateDie(GameObject diePrefab)
	{
		GameObject newDie = Instantiate(diePrefab) as GameObject;
		
		newDie.name = string.Format("{0} {1}", newDie.name.Replace("Clone", ""), dieCount);
		
		Material mat1 = DiceMats[Random.Range(0, DiceMats.Count)];
		Material mat2 = DiceMats[Random.Range(0, DiceMats.Count)];
		while (mat1 == mat2) // Don't let them be identical
		{
			mat2 = DiceMats[Random.Range(0, DiceMats.Count)];
		}
		
		// Give it a random texture
		newDie.renderer.materials = new Material[2] {mat1, mat2};
		
		dieCount++;
		
		return newDie;
	}
	
	UtilitiesScript utilitiesScript;
	
	UtilitiesScript Utilities()
	{
		if(null == utilitiesScript)
			utilitiesScript = GameObject.Find("Utilities").GetComponent<UtilitiesScript>();
		
		return utilitiesScript;
	}
	
	void AppendBattleText(string battleTextString)
	{
		//TODO: Could be good to add other decorations to the string
		battleText += string.Format("\n{0}", battleTextString);
		
		//scrollPosition.y = Mathf.Infinity;
	}
}
