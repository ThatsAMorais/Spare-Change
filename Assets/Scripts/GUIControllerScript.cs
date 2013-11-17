using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Type imports
using GameState = GameControllerScript.GameState;
using BattleActorDefinition = GameControllerScript.BattleActorDefinition;
using BattleAction = GameControllerScript.BattleAction;
using BattleActor = BattleControllerScript.BattleActor;
using Character = BattleControllerScript.Player;
using BattleRound = BattleControllerScript.BattleRound;
using Player = BattleControllerScript.Player;
using ScoreoidPlayer = ScoreoidInterface.ScoreoidPlayer;


public class GUIControllerScript : MonoBehaviour {

	private static int GUI_SCREEN_BORDER = 15;
	private static int GUI_TEXTFIELD_PAD = 25;
	//private static int GUI_BATTLESTAT_PAD = 20;

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
		bSubmitted = false;

		GUILayout.BeginVertical();
		GUILayout.Space(100);
		GUILayout.Box("Spare Change", "TitleBox");
		//GUILayout.FlexibleSpace();
		if(GUILayout.Button("Start a New Guy")) 	// TODO: Need a custom style for the menu items
		{
			//TODO: Do some kind of transition to the next UI state
			Utilities().setGameState(GameState.Register);
		}
		if(GUILayout.Button("Load Existing")) 		// TODO: Need a custom style for the menu items
		{
			//TODO: Do some kind of transition to the next UI state
			Utilities().setGameState(GameState.Login);
		}
		GUILayout.EndVertical();
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
		GUILayout.BeginVertical();

		GUILayout.Space(100);

		GUILayout.BeginHorizontal();
		GUILayout.Box(string.Format("{0}", GameState.Register == Utilities().getGameState() ? "Register New" : "Load"), "TitleBox");
		GUILayout.Space(10);
		GUILayout.EndHorizontal();

		GUILayout.Label("Character Name", "BattleQueue");
		characterNameInput = GUILayout.TextField(characterNameInput);
		if (Event.current.Equals(Event.KeyboardEvent("return")))
		{
			SubmitInput();
		}

		GUILayout.Label("Password", "BattleQueue");
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
	}

	void GUI_CharacterSelection()
	{
		// TODO: Wire the SelectCharacter function to this script
		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();

		GUILayout.FlexibleSpace();

		GUILayout.BeginVertical();

		foreach(BattleActorDefinition actorType in Utilities().getActorTypes())
		{
			if(GUILayout.Button(string.Format("{0}", actorType.name)))
			{
				Utilities().UpdatePlayer(newCharacter.name, 0, 0, 0, 1, actorType.weapon.name, actorType.name);
			}
			GUILayout.Space(20);
		}
		GUILayout.EndVertical();

		GUILayout.FlexibleSpace();

		GUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();
	}

	void Login(string username, string password)
	{
		// TODO: Request player from Score Keeper
	}

	void GUI_PlayerProfile()
	{
		Player player = Utilities().getCurrentCharacter();

		GUILayout.BeginVertical();

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();
		GUILayout.Box(string.Format("Name: {0}\nLevel: {1}\nXP: {2}\nChange: {3}\nWeapon: {4}\nKills: {5}",
									player.name, player.level, player.xp, player.change, player.weapon.name, player.kills));
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


		GUILayout.FlexibleSpace();

		GUILayout.EndVertical();
	}
	
	void GUI_BattleMode_enemyTurn()
	{
		GUILayout.Box(string.Format("{0}'s turn", Utilities().getCurrentTurnActor().name));
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
		GUILayout.Box("Select a Target");
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
		GUILayout.Box(string.Format("Rolling {0}", (true == bRollingDamage ? "-Damage-" : "-Chance to Hit-") ));
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

		foreach(BattleActor enemy in Utilities().getBattleEnemies())
		{
			GUILayout.Space(10);
			GUI_BattleStat(enemy);
		}

		GUILayout.Space(20);
		
		GUI_BattleStat(Utilities().getCurrentCharacter());
		
		GUILayout.FlexibleSpace();
		
		GUILayout.EndVertical();
	}
	*/

	bool bBattleStarted = false;
	void GUI_BattleMode()
	{
		BattleRound currentTurn = Utilities().getCurrentTurn();

		GUILayout.BeginVertical();
		GUI_BattleText();
		
		
		GUILayout.BeginHorizontal();
		////
		
		GUILayout.FlexibleSpace();
		GUI_BattleQueue();
		GUILayout.Space(5);
		//GUI_BattleStats();
		
		////
		GUILayout.EndHorizontal();
		
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
				GUILayout.Label("Queueing Next Turn");
			else
				GUILayout.Label("Starting Battle");
		}
		
		GUILayout.EndVertical();
	}
	
	void GUI_BattleOver()
	{
		Character playerCharacter = Utilities().getCurrentCharacter();

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
		GameState gameState = Utilities().getGameState();
		GUI.skin = guiSkin;

		if(previousGameState != gameState)
		{
			// Reset the inputs between states
			characterNameInput = "";
			passwordInput = "";
			loginMessage = "";
		}

		GUILayout.BeginArea(new Rect(0 + GUI_SCREEN_BORDER,
									 0 + GUI_SCREEN_BORDER,
									 Screen.width - GUI_SCREEN_BORDER,
									 Screen.height - GUI_SCREEN_BORDER));
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
		GUILayout.EndArea();

		previousGameState = gameState;
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
	/*
	void AddSpikes(float winX)
	{
		spikeCount = Mathf.Floor(winX - 152)/22;
		GUILayout.BeginHorizontal();
		GUILayout.Label ("", "SpikeLeft");//-------------------------------- custom
		for (i = 0; i < spikeCount; i++)
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
*/


	UtilitiesScript utilitiesScript;
	UtilitiesScript Utilities()
	{
		if(null == utilitiesScript)
			utilitiesScript = GameObject.Find("Utilities").GetComponent<UtilitiesScript>();

		return utilitiesScript;
	}


}
