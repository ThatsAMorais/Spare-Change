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


public class GUIControllerScript : MonoBehaviour {

	private static int GUI_SCREEN_BORDER = 15;
	private static int GUI_TEXTFIELD_PAD = 25;
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
	Rect actionSelectionRect = new Rect (600, 420, 500, 300);
	Rect battleTextRect = new Rect (10, 420, 500, 300);
	Rect battleQueueRect = new Rect (850, 0, 250, 300);
	Rect battleStatsRect = new Rect (0, 40, 350, 500);

	float HroizSliderValue = 0.5f;
	float VertSliderValue = 0.5f;
	bool ToggleBTN = false;

	public GUISkin guiSkin;
	public GUIText BattleTextPrefab;

	GameState previousGameState;

	string battleText;
	Vector2 scrollPosition;

	string characterNameInput;
	string passwordInput;
	string loginMessage;

	bool bSubmitted;

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
			loginMessage = "";
		}

		switch(gameState)
		{
		case GameState.Title:
			GUI_TitleScreen();
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
			Utilities().setGameState(GameState.PlayerProfile);
		}
	}

	public void RequestFailed(string error)
	{
		loginMessage = error;
	}


	// Use this for initialization
	void Awake ()
	{
		previousGameState = GameState.Title;

		battleText = "";
		characterNameInput = "";
		passwordInput = "";
		loginMessage = "";

		bSubmitted = false;

		scrollPosition = new Vector2(0, Mathf.Infinity);
	}
	
	// Update is called once per frame
	void Update ()
	{
	}


	void GUI_TitleScreen()
	{

		GUILayout.BeginArea(new Rect(0 + GUI_SCREEN_BORDER,
									 0 + GUI_SCREEN_BORDER,
									 Screen.width - GUI_SCREEN_BORDER,
									 Screen.height - GUI_SCREEN_BORDER));
		bSubmitted = false;

		GUILayout.BeginVertical();
		GUILayout.Space(100);
		GUILayout.Box("Spare Change", "TitleBox");
		//GUILayout.FlexibleSpace();
		if(GUILayout.Button("Start a New Guy")) 	// TODO: Need a custom style for the menu items
		{
			Utilities().setGameState(GameState.Register);
		}
		if(GUILayout.Button("Load Existing")) 		// TODO: Need a custom style for the menu items
		{
			Utilities().setGameState(GameState.Login);
		}
		GUILayout.EndVertical();

		GUILayout.EndArea();
	}

	void SubmitInput()
	{
		if(false == bSubmitted)
		{
			bSubmitted = true;
			if(GameState.Register == Utilities().getGameState())
			{
				loginMessage = "Attempting to Register...";
				// -- Registration: Step 1 - Attempt to create an account with the user's input
				Utilities().Register(characterNameInput, passwordInput);
			}
			else
			{
				loginMessage = "Attempting to Load...";
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

		GUILayout.Space(100);

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

		if(false == loginMessage.Equals(""))
			GUILayout.Box(loginMessage);

		GUILayout.FlexibleSpace();

		if(GUILayout.Button("Back"))
		{
			Utilities().setGameState(GameState.Title);
		}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();

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

		GUILayout.EndArea();
	}


	void GUI_PlayerProfile()
	{
		GUILayout.BeginArea(new Rect(0 + GUI_SCREEN_BORDER,
							 0 + GUI_SCREEN_BORDER,
							 Screen.width - GUI_SCREEN_BORDER,
							 Screen.height - GUI_SCREEN_BORDER));

		Player player = Utilities().getCurrentCharacter();

		GUILayout.BeginVertical();

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();
		GUILayout.Box(string.Format("Name: {0}\nLevel: {1}\nXP: {2}\nChange: {3}\nWeapon: {4}\nKills: {5}",
									player.name, player.level, player.xp, player.change, player.weapon.name, player.kills));
		GUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();
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

		GUILayout.FlexibleSpace();

		GUILayout.EndVertical();

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
		GUILayout.Box("Select a Target", "CurrentAction");
		////
		GUILayout.FlexibleSpace();
		////
		GUILayout.BeginHorizontal();
		foreach(BattleActor enemy in Utilities().getBattleEnemies())
		{
			if(GUILayout.Button(string.Format("{0}", enemy.name)))
			{
				// The action chosen will have an impact on who the possible targets can be
				Utilities().SelectedEnemy(enemy);
			}
		}
		if(GUILayout.Button("Back"))
		{
			Utilities().setBattleRoundState(BattleRound.State.SelectAction);
		}
		GUILayout.EndHorizontal();
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
		GUILayout.Space(8);

		GUILayout.BeginHorizontal();
		
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		GUILayout.Label(string.Format("{0}", battleText), "PlainText");
		GUILayout.EndScrollView();
		
		GUILayout.EndHorizontal();
	}
	
	void GUI_BattleMode_BattleQueue(int windowID)
	{
		GUILayout.Space(32);

		GUILayout.BeginVertical();

		BattleRound currentTurn = Utilities().getCurrentTurn();
		Queue<BattleRound> queue = Utilities().getBattleQueue();

		if(null != currentTurn)
		{
			GUILayout.Box(string.Format("({0}) {1}", Utilities().getCurrentTurnActor().remainingHealth, Utilities().getCurrentTurnActor().name), "BattleQueue");
		}

		if(null != queue)
		{
			foreach(BattleRound round in queue)
			{
				GUILayout.Box(string.Format("({0}) {1}", round.actor.remainingHealth, round.actor.name), "BattleQueue");
			}
		}
		
		GUILayout.EndVertical();
	}

	/*
	void GUI_BattleMode_BattleStat(BattleActor actor)
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
		
		GUI_BattleStat(Utilities().getCurrentCharacter());
		
		GUILayout.FlexibleSpace();
		
		GUILayout.EndVertical();
	}
	*/

	bool bBattleStarted = false;
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

		actionSelectionRect = GUI.Window(0, actionSelectionRect, GUI_BattleMode_actionSelection, "Actions");
	 	//now adjust to the group. (0,0) is the topleft corner of the group.
		GUI.BeginGroup(new Rect (0,0,100,100));
		// End the group we started above. This is very important to remember!
		GUI.EndGroup();
		battleTextRect = GUI.Window(1, battleTextRect, GUI_BattleMode_BattleText, "");

		battleQueueRect = GUI.Window(2, battleQueueRect, GUI_BattleMode_BattleQueue, "Battle Queue");

		//battleStatsRect = GUI.Window(3, battleStatsRect, GUI_BattleMode_BattleStats, "");

		/*
		GUILayout.BeginVertical();
		GUI_BattleMode_BattleText();
		
		
		GUILayout.BeginHorizontal();
		////
		
		GUILayout.FlexibleSpace();
		GUI_BattleMode_BattleQueue();
		GUILayout.Space(5);
		//GUI_BattleStats();

		////
		GUILayout.EndHorizontal();
		*/




		/*
		GUILayout.EndVertical();
		*/
	}
	
	void GUI_BattleOver()
	{
		GUILayout.BeginArea(new Rect(0 + GUI_SCREEN_BORDER,
							 0 + GUI_SCREEN_BORDER,
							 Screen.width - GUI_SCREEN_BORDER,
							 Screen.height - GUI_SCREEN_BORDER));

		Character playerCharacter = Utilities().getCurrentCharacter();

		GUILayout.BeginVertical();
		GUILayout.Box("Battle Over", "TitleBox"); 		//TODO: Need a custom style for the title
		GUILayout.Box(string.Format("{0} is the victor",
									(playerCharacter.remainingHealth > 0 ? playerCharacter.name : "The Enemy")));
		GUILayout.EndVertical();
		GUILayout.EndArea();
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

	void AddSpikes(float winX)
	{
		spikeCount = Mathf.Floor(winX - 152)/22;
		GUILayout.BeginHorizontal();
		GUILayout.Label ("", "SpikeLeft");//-------------------------------- custom
		/*
		for (int i = 0; i < spikeCount; i++)
        {
			GUILayout.Label ("", "SpikeMid");//-------------------------------- custom
        }
        */
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
