using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Type imports
using GameState = GameControllerScript.GameState;
using BattleAction = GameControllerScript.BattleAction;
using Weapon = GameControllerScript.Weapon;
using BattleActor = BattleControllerScript.BattleActor;
using Character = BattleControllerScript.Player;
using BattleRound = BattleControllerScript.BattleRound;
using Player = BattleControllerScript.Player;
using ScoreoidPlayer = ScoreoidInterface.ScoreoidPlayer;
using Roll = DiceControllerScript.Roll;


public class GUIControllerScript : MonoBehaviour {

	private static int GUI_SCREEN_BORDER = 15;
	//private static int GUI_TEXTFIELD_PAD = 25;
	//private static int GUI_BATTLESTAT_PAD = 20;

	float leafOffset;
	float frameOffset;
	float skullOffset;

	float RibbonOffsetX;
	float FrameOffsetX;
	float SkullOffsetX;
	float RibbonOffsetY;
	float FrameOffsetY;
	float SkullOffsetY;

	float WSwaxOffsetX;
	float WSwaxOffsetY;
	float WSribbonOffsetX;
	float WSribbonOffsetY;

	float spikeCount;

	//if you're using the spikes you'll need to find sizes that work well with them these are a few...
	Rect actionSelectionRect /*= new Rect (600, 420, 500, 300)*/;
	Rect battleTextRect /*= new Rect (10, 420, 500, 300)*/;
	Rect battleQueueRect/* = new Rect (850, 0, 250, 300)*/;
	Rect battleStatsRect/* = new Rect (0, 40, 350, 500)*/;

	//float HroizSliderValue = 0.5f;
	//float VertSliderValue = 0.5f;
	//bool ToggleBTN = false;

	public GUISkin guiSkin;
	public GUIText BattleTextPrefab;

	GameState previousGameState;

	string battleText;
	Vector2 scrollPosition;

	string characterNameInput;
	string passwordInput;
	string responseMessage;

	bool bSubmitted;

	bool bThrowingDice;
	int diceCount;
	float diceThrowTimer;

	List<Weapon> randomWeaponChoices;

	string currentDie = "d4";
	float rewardTickTimer = 0.0f;
	private static float REWARD_TICK_TIMEOUT = 1;

	static int TITLE_SCREEN_DICE_MAX = 200;
	static float TITLE_SCREEN_DICE_THROW_TIMEOUT = 8;

	bool bBattleStarted;
	bool bRewardsAwarded;
	bool bFinishedRewarding;
	bool bChangedWeapon;
	float awardedExp;
	float awardedChange;
	float numberOfKills;

	NewCharacter newCharacter;

	public class NewCharacter
	{
		public string name;
		public string password;
		public string actorClass;
	}


	/// <summary>
	/// Raises the GU event.
	/// </summary>
	void OnGUI()
	{
		GameState gameState = Utilities().getGameState();
		GUI.skin = guiSkin;

		if(previousGameState != gameState)
		{
			// Reset the inputs between states
			characterNameInput = "";
			passwordInput = "";
			responseMessage = "";
		}

		switch(gameState)
		{
		case GameState.Title:
			GUI_TitleScreen();
			bThrowingDice = true;
			break;

		case GameState.Register:
		case GameState.Login:
			GUI_Register_Login();
			break;

		case GameState.CharacterSelection:
			GUI_CharacterSelection();
			break;

		case GameState.PlayerProfile:
			GUI_PlayerProfile();
			break;

		case GameState.BattleMode:
			bThrowingDice = false;
			GUI_BattleMode();
			break;

		case GameState.BattleOver:
			GUI_BattleOver();
			break;
		}

		previousGameState = gameState;
	}

	public void PlayerLoggedIn(ScoreoidPlayer player)
	{
		Utilities().setCurrentCharacter(player);
	}

	// -- Registration: Step 2 - Username and Password were available, and a "Player" was created
	public void PlayerRegistered()
	{
		// Start a new character object
		newCharacter = new NewCharacter();

		// Store these details between this step and the character selection phase
		newCharacter.name = characterNameInput;
		newCharacter.password = passwordInput;

		// Start the character selection
		Utilities().setGameState(GameState.CharacterSelection);
	}

	// -- Registration: Step 3 - Updating the player's class was successful
	public void UpdatedPlayerSuccessfully()
	{
		GameState gameState = Utilities().getGameState();

		if(GameState.CharacterSelection == gameState)
		{
			// Now that the player is created and the character class has been selected,
			//	login as this user which will tap into the normal Login logic as the
			//	player is retrieved from the server. (Step 4)
			Utilities().Login(newCharacter.name, newCharacter.password);
		}
		else if(GameState.BattleOver == gameState)
		{
			responseMessage = "Player Data Saved!";
		}
	}

	public void RequestFailed(string error)
	{
		responseMessage = error;
		bSubmitted = false;
	}


	// Use this for initialization
	void Awake ()
	{
		previousGameState = GameState.Title;

		battleText = "";
		characterNameInput = "";
		passwordInput = "";
		responseMessage = "";

		bSubmitted = false;
		bBattleStarted = false;
		bRewardsAwarded = false;
		bFinishedRewarding = false;
		bChangedWeapon = false;

		scrollPosition = new Vector2(0, Mathf.Infinity);

		Utilities().ThrowDice(new Roll("d4", 5), false);
		Utilities().ThrowDice(new Roll("d6", 5), false);
		Utilities().ThrowDice(new Roll("d8", 5), false);
		Utilities().ThrowDice(new Roll("d10", 5), false);
		Utilities().ThrowDice(new Roll("d12", 5), false);
		Utilities().ThrowDice(new Roll("d20", 5), false);
		Utilities().ThrowDice(new Roll("d100", 5), false);

		battleQueueRect 	= new Rect(Screen.width*0.75f,
										5f,
										Screen.width*0.25f,
										Screen.height*0.45f);

		battleTextRect 		= new Rect(Screen.width*0.008f,
										Screen.height*0.6f,
										Screen.width*0.4f,
										Screen.height*0.4f);

		actionSelectionRect = new Rect(Screen.width*0.4f,
										Screen.height*0.6f,
										Screen.width*0.6f,
										Screen.height*0.4f);

		battleStatsRect		= new Rect(Screen.width*0.008f,
			                             5f,
			                             Screen.width*0.2f,
			                             Screen.height*0.55f);
	}

	// Update is called once per frame
	void Update ()
	{
		if(true == bThrowingDice && TITLE_SCREEN_DICE_MAX > diceCount)
		{
			diceThrowTimer += Time.deltaTime;
			if(TITLE_SCREEN_DICE_THROW_TIMEOUT <= diceThrowTimer)
			{
				diceThrowTimer = 0;
				Utilities().ThrowDice(new Roll(currentDie, Random.Range(1,4)), false);
				diceCount++;
				switch(currentDie)
				{
				case "d4":
					currentDie = "d6";
					break;
				case "d6":
					currentDie = "d8";
					break;
				case "d8":
					currentDie = "d10";
					break;
				case "d10":
					currentDie = "d12";
					break;
				case "d12":
					currentDie = "d20";
					break;
				case "d20":
					currentDie = "d100";
					break;
				case "d100":
					currentDie = "d4";
					break;
				default:
					currentDie = "d4";
					break;
				}
			}
		}


		if(bRewardsAwarded)
		{
			rewardTickTimer += Time.deltaTime;

			if(REWARD_TICK_TIMEOUT <= rewardTickTimer)
			{
				rewardTickTimer = 0;

				Player playerCharacter = Utilities().getCurrentCharacter();

				if(true == bFinishedRewarding)
				{
					bRewardsAwarded = false;
					Utilities().UpdatePlayer(playerCharacter.name,
											playerCharacter.change,
											playerCharacter.xp,
											playerCharacter.kills,
											playerCharacter.level,
											playerCharacter.weapon.name);
				}
				else
				{
					if(0 != awardedExp)
					{
						playerCharacter.AddExperience(1);
						awardedExp -= 1;
					}

					if(0 != awardedChange)
					{
						playerCharacter.AddChange(1);
						awardedChange -= 1;
					}


					if(0 != numberOfKills)
					{
						playerCharacter.AddKills(1);
						numberOfKills -= 1;
					}

					if(0 == awardedExp + awardedChange + numberOfKills)
					{
						bFinishedRewarding = true;
					}
				}
			}
		}
	}


	void GUI_TitleScreen()
	{
		GUI.skin = guiSkin;
		Rect titleRect = new Rect(0 + GUI_SCREEN_BORDER,
								 0 + GUI_SCREEN_BORDER,
								 Screen.width - GUI_SCREEN_BORDER,
								 Screen.height - GUI_SCREEN_BORDER);


		GUILayout.BeginArea(titleRect);
		bSubmitted = false;

		GUILayout.BeginVertical();
		GUILayout.Space(50);

		AddSpikes(Screen.width, true);

		GUILayout.Box("Spare Change", "TitleBox");
		GUILayout.Space(100);
		if(GUILayout.Button("Start a New Guy")) 	// TODO: Need a custom style for the menu items
		{
			Utilities().setGameState(GameState.Register);
		}
		if(GUILayout.Button("Load Existing")) 		// TODO: Need a custom style for the menu items
		{
			Utilities().setGameState(GameState.Login);
		}
		GUILayout.EndVertical();

		AddSpikes(Screen.width, true);
		GUILayout.EndArea();
	}

	void SubmitInput()
	{
		if(false == bSubmitted)
		{
			bSubmitted = true;
			if(GameState.Register == Utilities().getGameState())
			{
				responseMessage = "Attempting to Register...";
				// -- Registration: Step 1 - Attempt to create an account with the user's input
				Utilities().Register(characterNameInput, passwordInput);
			}
			else
			{
				responseMessage = "Attempting to Load...";
				// -- Attempt to login with the character-name and password
				Utilities().Login(characterNameInput, passwordInput);
			}
		}
	}

	void GUI_Register_Login()
	{
		GUILayout.BeginArea(new Rect(0 + GUI_SCREEN_BORDER,
									 0 + GUI_SCREEN_BORDER,
									 Screen.width - GUI_SCREEN_BORDER,
									 Screen.height - GUI_SCREEN_BORDER));
		GUILayout.BeginVertical();

		GUILayout.Space(50);
		AddSpikes(Screen.width, true);

		GUILayout.BeginHorizontal();
		GUILayout.Box(string.Format("{0}", GameState.Register == Utilities().getGameState() ? "Register New" : "Load"), "TitleBox");
		GUILayout.Space(10);
		GUILayout.EndHorizontal();

		GUILayout.Label("Character Name");
		characterNameInput = GUILayout.TextField(characterNameInput);
		if (Event.current.Equals(Event.KeyboardEvent("return")))
		{
			SubmitInput();
		}

		GUILayout.Label("Password");
		passwordInput = GUILayout.PasswordField(passwordInput, "*"[0]);
		if (Event.current.Equals(Event.KeyboardEvent("return")))
		{
			SubmitInput();
		}

		GUILayout.BeginHorizontal();	// | #Submit# <---> "message" <---> #Back# |
		if(GUILayout.Button("Submit"))
		{
			SubmitInput();
		}

		GUILayout.FlexibleSpace();

		if(false == responseMessage.Equals(""))
			GUILayout.Box(responseMessage, "LegendaryText");

		GUILayout.FlexibleSpace();

		if(GUILayout.Button("Back"))
		{
			Utilities().setGameState(GameState.Title);
		}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		AddSpikes(Screen.width, true);
		GUILayout.EndArea();
	}

	void GUI_CharacterSelection()
	{
		GUILayout.BeginArea(new Rect(0 + GUI_SCREEN_BORDER,
									 0 + GUI_SCREEN_BORDER,
									 Screen.width - GUI_SCREEN_BORDER,
									 Screen.height - GUI_SCREEN_BORDER));
		// TODO: Wire the SelectCharacter function to this script
		GUILayout.FlexibleSpace();

		AddSpikes(Screen.width, true);

		GUILayout.BeginHorizontal();

		GUILayout.FlexibleSpace();

		GUILayout.BeginVertical();

		foreach(Weapon weaponType in Utilities().getWeaponTypes(1))
		{
			if(GUILayout.Button(string.Format("{0}", weaponType.name)))
			{
				Utilities().UpdatePlayer(newCharacter.name, 0, 0, 0, 1, weaponType.name);
			}
			GUILayout.Space(20);
		}
		GUILayout.EndVertical();

		GUILayout.FlexibleSpace();

		GUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();
		AddSpikes(Screen.width, true);
		GUILayout.EndArea();
	}


	void GUI_PlayerProfile()
	{
		Player player = Utilities().getCurrentCharacter();

		GUILayout.BeginArea(new Rect(0 + GUI_SCREEN_BORDER,
									 0 + GUI_SCREEN_BORDER,
									 Screen.width - GUI_SCREEN_BORDER,
									 Screen.height - GUI_SCREEN_BORDER));

		GUILayout.BeginVertical();

		GUILayout.FlexibleSpace();
		AddSpikes(Screen.width, true);
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Box(string.Format("Name: {0}\nLevel: {1}\nXP: {2}\nChange: {3}\nWeapon: {4}\nKills: {5}",
									player.name, player.level, player.xp, player.change, player.weapon.name, player.kills));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if(GUILayout.Button("Start Battle"))
		{
			// Now that the battle is initialized, switch the gamestate to battle-mode
			Utilities().StartBattle();
			Utilities().setGameState(GameState.BattleMode);
		}
		GUILayout.Space(20);
		if(GUILayout.Button("Log Off"))
		{
			Utilities().setGameState(GameState.Title);
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		AddSpikes(Screen.width, true);


		GUILayout.EndVertical();

		GUILayout.FlexibleSpace();

		GUILayout.EndArea();
	}
	
	void GUI_BattleMode_enemyTurn()
	{
		GUILayout.Box(string.Format("{0}'s turn", Utilities().getCurrentTurnActor().name), "CurrentAction");
		////
		GUILayout.FlexibleSpace();
		////
	}


	void GUI_BattleMode_playerSelectAction()
	{
		GUILayout.Box("Select an Action", "CurrentAction");
		////
		GUILayout.FlexibleSpace();
		////
		GUILayout.BeginHorizontal();
		foreach(BattleAction action in Utilities().getCurrentTurnActor().getActions())
		{
			if(GUILayout.Button(string.Format("{0}", action.name)))
			{
				Utilities().SelectedAction(action);
			}
		}
		GUILayout.EndHorizontal();
		////
		GUILayout.FlexibleSpace();
		////
	}
	
	void GUI_BattleMode_playerSelectTarget()
	{
		int enemyCount = 0;

		GUILayout.Box("Select a Target", "CurrentAction");
		////
		GUILayout.FlexibleSpace();
		////
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal();
		foreach(BattleActor enemy in Utilities().getBattleEnemies())
		{
			if(GUILayout.Button(string.Format("{0}", enemy.name)))
			{
				// The action chosen will have an impact on who the possible targets can be
				Utilities().SelectedEnemy(enemy);
			}

			enemyCount++;

			if(2 == enemyCount)
			{
				enemyCount = 0;
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
			}
		}
		if(GUILayout.Button("Back"))
		{
			Utilities().setBattleRoundState(BattleRound.State.SelectAction);
		}
		GUILayout.EndHorizontal();

		GUILayout.EndVertical();
		////
		GUILayout.FlexibleSpace();
		////
	}
	
	void GUI_BattleMode_playerAct()
	{
		BattleRound currentTurn = Utilities().getCurrentTurn();
		bool bRollingDamage = ((true == currentTurn.bRolledChanceToHit) && (true == currentTurn.bChanceToHitSuccess));

		GUILayout.BeginHorizontal();
		////
		GUILayout.FlexibleSpace();
		////
		GUILayout.Box(string.Format("Rolling {0}", (true == bRollingDamage ? "-Damage-" : "-Chance to Hit-") ), "CurrentAction");
		////
		GUILayout.FlexibleSpace();
		////
		GUILayout.EndHorizontal();
	}
	
	void GUI_BattleMode_BattleText(int windowID)
	{
		AddSpikes(battleTextRect.width);
		//GUILayout.Space(2);

		GUILayout.BeginHorizontal();
		
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		GUILayout.Label(string.Format("{0}", battleText), "PlainText");
		GUILayout.EndScrollView();
		
		GUILayout.EndHorizontal();
	}
	
	void GUI_BattleMode_BattleQueue(int windowID)
	{
		AddSpikes(battleQueueRect.width);

		GUILayout.BeginVertical();

		BattleRound currentTurn = Utilities().getCurrentTurn();
		Queue<BattleRound> queue = Utilities().getBattleQueue();

		if(null != currentTurn)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Box(string.Format("({0})", Utilities().getCurrentTurnActor().remainingHealth), "BattleQueue");
			GUILayout.Box(string.Format("{0}", Utilities().getCurrentTurnActor().name), "BattleQueue");
			GUILayout.EndHorizontal();
		}

		if(null != queue)
		{
			foreach(BattleRound round in queue)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Box(string.Format("({0})", round.actor.remainingHealth), "BattleQueue");
				GUILayout.Box(string.Format("{0}", round.actor.name), "BattleQueue");
				GUILayout.EndHorizontal();
			}
		}
		
		GUILayout.EndVertical();
	}
	
	void GUI_BattleMode_BattleStat(BattleActor actor)
	{
		/*
		// Frame
		GUILayout.Box("");
		// Meter
		Rect tempRect = GUILayoutUtility.GetLastRect();
		tempRect.x += GUI_BATTLESTAT_PAD;
		tempRect.width -= GUI_BATTLESTAT_PAD*2;
		tempRect.y += GUI_BATTLESTAT_PAD;
		tempRect.height -= GUI_BATTLESTAT_PAD*2;

		GUILayout.BeginHorizontal();
		GUI.Box(tempRect, "");
		GUILayout.Space(tempRect.width - ((actor.remainingHealth/actor.health) * tempRect.width));
		GUILayout.EndHorizontal();

		// Cutout
		GUI.Box(tempRect, "");
		// Name
		GUI.BeginGroup(tempRect);

		GUILayout.Box(actor.name, "LegendaryText");
		GUI.EndGroup();
		*/

		GUILayout.Box(string.Format("{0} : ({1})", actor.name,actor.remainingHealth));
	}

	void GUI_BattleMode_BattleStats(int windowID)
	{
		GUILayout.Space(8);

		GUILayout.BeginVertical();

		foreach(BattleActor enemy in Utilities().getBattleEnemies())
		{
			GUILayout.Space(10);
			GUI_BattleMode_BattleStat(enemy);
		}

		GUILayout.Space(20);
		
		GUI_BattleMode_BattleStat(Utilities().getCurrentCharacter());
		
		GUILayout.FlexibleSpace();
		
		GUILayout.EndVertical();
	}
	
	void GUI_BattleMode_actionSelection(int windowID)
	{
		AddSpikes(actionSelectionRect.width);

		BattleRound currentTurn = Utilities().getCurrentTurn();

		GUILayout.Space(8);

		if(null != currentTurn)
		{
			bBattleStarted = true;

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
			if(true == bBattleStarted)
			{
			}
			else
			{

			}
		}
	}

	void GUI_BattleMode()
	{
		/*
		// Test button TODO Remove
		if(GUI.Button(new Rect(Screen.width*0.45f, Screen.height*0.4f, Screen.width*0.1f, Screen.height*0.1f), "End Battle"))
		{
			Utilities().BattleOverTest(true);
		}
		*/

		actionSelectionRect = GUI.Window(0, actionSelectionRect, GUI_BattleMode_actionSelection, "Actions");
	 	//now adjust to the group. (0,0) is the topleft corner of the group.
		GUI.BeginGroup(new Rect (0,0,100,100));
		// End the group we started above. This is very important to remember!
		GUI.EndGroup();
		battleTextRect = GUI.Window(1, battleTextRect, GUI_BattleMode_BattleText, "");

		battleQueueRect = GUI.Window(2, battleQueueRect, GUI_BattleMode_BattleQueue, "Battle Queue");

		//battleStatsRect = GUI.Window(3, battleStatsRect, GUI_BattleMode_BattleStats, "");
	}

	List<Weapon> GetRandomWeaponOptions(int level, int count)
	{
		List<Weapon> allWeaponsForLevel = Utilities().getWeaponTypes(level);

		List<Weapon> randomSelection = new List<Weapon>();

		for(int w=0; w < count; w++)
		{
			randomSelection.Add(allWeaponsForLevel[Random.Range(0,randomSelection.Count)]);
		}

		return randomSelection;
	}
	
	void GUI_BattleOver()
	{
		Character playerCharacter = Utilities().getCurrentCharacter();

		if(null == randomWeaponChoices || 0 == randomWeaponChoices.Count)
		{
			randomWeaponChoices = GetRandomWeaponOptions(Utilities().getCurrentCharacter().level, 3);
		}

		GUILayout.BeginArea(new Rect(0 + GUI_SCREEN_BORDER,
									 0 + GUI_SCREEN_BORDER,
									 Screen.width - GUI_SCREEN_BORDER,
									 Screen.height - GUI_SCREEN_BORDER));


		GUILayout.BeginVertical();
		GUILayout.Space(10);

		AddSpikes(Screen.width, true);

		GUILayout.Box("Battle Over", "TitleBox"); 		//TODO: Need a custom style for the title
		GUILayout.Box(string.Format("{0} the victor",
									(playerCharacter.remainingHealth > 0 ? playerCharacter.name + " is" : "The Enemy was")),
		              				"LegendaryText");

		GUILayout.BeginHorizontal();
		GUILayout.Space(15);
		
		GUILayout.BeginVertical();
		GUILayout.Label(string.Format("Spare Change: {0}", playerCharacter.change), "LegendaryText");
		GUILayout.Space(5);
		GUILayout.Label(string.Format("Experience: {0}", playerCharacter.xp), "LegendaryText");
		GUILayout.Space(5);
		GUILayout.Label(string.Format("Kills: {0}", playerCharacter.kills), "LegendaryText");
		GUILayout.EndVertical();

		GUILayout.Space(15);

		if(true == bFinishedRewarding)
		{
			if(false == bChangedWeapon)
			{
				// Show possible weapon change options, here
				GUILayout.BeginHorizontal();
				GUILayout.Space(15);
				GUILayout.BeginVertical();
				GUILayout.Label("Change Weapons?");
				
				foreach(Weapon weapon in randomWeaponChoices)
				{
					if(GUILayout.Button(string.Format("{0} : Cost({1})", weapon.name, 3*weapon.level)))
					{
						bChangedWeapon = true;
						
						randomWeaponChoices = new List<Weapon>(); // Clear the weapon choices
						
						playerCharacter.changeWeapon(weapon);
						
						Utilities().UpdatePlayer(playerCharacter.name,
						                         playerCharacter.change,
						                         playerCharacter.xp,
						                         playerCharacter.kills,
						                         playerCharacter.level,
						                         playerCharacter.weapon.name);
					}
				}
				
				GUILayout.EndVertical();
				GUILayout.Space(15);
				GUILayout.EndHorizontal();
			}
		}

		GUILayout.EndHorizontal();
		

		if(true == bFinishedRewarding)
		{
			GUILayout.BeginHorizontal();

			if(GUILayout.Button("Return to Player Profile"))
			{
				Utilities().setGameState(GameState.PlayerProfile);
			}

			GUILayout.EndHorizontal();
		}

		GUILayout.FlexibleSpace();
			
		if(false == responseMessage.Equals(""))
			GUILayout.Box(responseMessage, "LegendaryText");

		AddSpikes(Screen.width, true);

		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
	
	public void PlayerIsVictorious(float exp, float change, float kills)
	{
		bRewardsAwarded = true;
		bFinishedRewarding = false;

		awardedExp = exp;
		awardedChange = change;
		numberOfKills = kills;
	}


	public void AppendBattleText(string battleTextString)
	{
		//TODO: Could be good to add other decorations to the string
		battleText += string.Format("\n{0}", battleTextString);

		scrollPosition.y = Mathf.Infinity;
	}


	public void SpawnPts(string text, float x, float y, Color color)
	{
	    x = Mathf.Clamp(x, 0.05f, 0.95f); // clamp position to screen to ensure
	    y = Mathf.Clamp(y, 0.05f, 0.9f);  // the string will be visible

	    GUIText gui = Instantiate(BattleTextPrefab, new Vector3(x,y,0), Quaternion.identity) as GUIText;

	    gui.guiText.text = text;
		gui.guiText.material.color = color; // set text color
	}


	// -- Necromancer Style

	void AddSpikes(float winX, bool bFullScreen=false)
	{
		int pad = 152;

		if(true == bFullScreen)
		{
			pad = 80;
		}

		spikeCount = Mathf.Floor(winX - pad)/22;
		GUILayout.BeginHorizontal();
		GUILayout.Label ("", "SpikeLeft");//-------------------------------- custom
		for (int i = 0; i < spikeCount; i++)
        {
			GUILayout.Label ("", "SpikeMid");//-------------------------------- custom
        }
		GUILayout.Label ("", "SpikeRight");//-------------------------------- custom
		GUILayout.EndHorizontal();
	}

	void FancyTop(float topX)
	{
		leafOffset = (topX/2)-64;
		frameOffset = (topX/2)-27;
		skullOffset = (topX/2)-20;
		GUI.Label(new Rect(leafOffset, 18, 0, 0), "", "GoldLeaf");//-------------------------------- custom
		GUI.Label(new Rect(frameOffset, 3, 0, 0), "", "IconFrame");//-------------------------------- custom
		GUI.Label(new Rect(skullOffset, 12, 0, 0), "", "Skull");//-------------------------------- custom
	}

	void WaxSeal(float x, float y)
	{
		WSwaxOffsetX = x - 120;
		WSwaxOffsetY = y - 115;
		WSribbonOffsetX = x - 114;
		WSribbonOffsetY = y - 83;

		GUI.Label(new Rect(WSribbonOffsetX, WSribbonOffsetY, 0, 0), "", "RibbonBlue");//-------------------------------- custom
		GUI.Label(new Rect(WSwaxOffsetX, WSwaxOffsetY, 0, 0), "", "WaxSeal");//-------------------------------- custom
	}

	void DeathBadge(float x, float y)
	{
		RibbonOffsetX = x;
		FrameOffsetX = x+3;
		SkullOffsetX = x+10;
		RibbonOffsetY = y+22;
		FrameOffsetY = y;
		SkullOffsetY = y+9;

		GUI.Label(new Rect(RibbonOffsetX, RibbonOffsetY, 0, 0), "", "RibbonRed");//-------------------------------- custom
		GUI.Label(new Rect(FrameOffsetX, FrameOffsetY, 0, 0), "", "IconFrame");//-------------------------------- custom
		GUI.Label(new Rect(SkullOffsetX, SkullOffsetY, 0, 0), "", "Skull");//-------------------------------- custom
	}


	UtilitiesScript utilitiesScript;
	UtilitiesScript Utilities()
	{
		if(null == utilitiesScript)
			utilitiesScript = GameObject.Find("Utilities").GetComponent<UtilitiesScript>();

		return utilitiesScript;
	}


}
