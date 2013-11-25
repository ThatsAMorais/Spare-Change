using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Type imports
using GameState = GameControllerScript.GameState;
using BattleAction = GameControllerScript.BattleAction;
using Weapon = GameControllerScript.Weapon;
using Attack = GameControllerScript.Attack;
using BattleActor = BattleControllerScript.BattleActor;
using Character = BattleControllerScript.Player;
using BattleRound = BattleControllerScript.BattleRound;
using Player = BattleControllerScript.Player;
using ScoreoidPlayer = ScoreoidInterface.ScoreoidPlayer;
using Roll = DiceControllerScript.Roll;


public class GUIControllerScript : MonoBehaviour {


	public GUISkin guiSkin;
	public GUIText BattleTextPrefab;

	
	private static float REWARD_TICK_TIMEOUT = 1;
	
	private static int TITLE_SCREEN_DICE_MAX = 200;
	private static float TITLE_SCREEN_DICE_THROW_TIMEOUT = 8;

	private static int GUI_SCREEN_BORDER = 0;
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
	Rect gameMenuRect;

	GameState previousGameState;

	string battleText;
	Vector2 scrollPosition;

	string characterNameInput;
	string emailInput;
	string passwordInput;
	string responseMessage;

	bool bSubmitted;

	bool bThrowingDice;
	int diceCount;
	float diceThrowTimer;

	List<Weapon> randomWeaponChoices;

	string currentDie = "d4";
	float rewardTickTimer = 0.0f;

	bool bPlayerIsVictorious;
	bool bBattleStarted;
	bool bRewardsAwarded;
	bool bFinishedRewarding;
	bool bChangedWeapon;
	float awardedExp;
	float awardedChange;
	float numberOfKills;

	bool bDoTitleWindow;
	bool bDoLoginWindow;
	bool bDoWeaponSelectWindow;
	bool bDoPlayerProfileWindow;
	bool bDoBattleWindow;
	bool bDoBattleOverWindow;

	bool bRegisteringNew = false;
	bool bDoTransition = false;
	bool bTransitioningBattleWindows = false;
	bool bTransitioningBattleWindowsOn = false;

	Dictionary<string,int> windowIDs;
	Dictionary<string,Rect> windowRects;
	Dictionary<Direction,Rect> offScreenRects;
	Dictionary<string,Rect> battleWindowRects;

	private static float TransitionSpeed = 25;
	private static float MaxAcceleration = 10;
	float currentAcceleration = 1;

	string previousRect;
	
	NewCharacter newCharacter;

	public class NewCharacter
	{
		public string name;
		public string password;
		public string actorClass;
	}
	
	public enum Direction
	{
		Up,
		Down,
		Left,
		Right,
	}

	// Use this for initialization
	void OnEnable ()
	{
		previousGameState = GameState.NoState;

		battleText = "";
		characterNameInput = "";
		emailInput = "";
		passwordInput = "";
		responseMessage = "";

		bSubmitted = false;
		bBattleStarted = false;
		bRewardsAwarded = false;
		bFinishedRewarding = false;
		bChangedWeapon = false;

		bDoTitleWindow = true;
		bDoLoginWindow = false;
		bDoWeaponSelectWindow = false;
		bDoPlayerProfileWindow = false;
		bDoBattleWindow = false;
		bDoBattleOverWindow = false;

		scrollPosition = new Vector2(0, Mathf.Infinity);

		windowIDs = new Dictionary<string, int>();
		windowRects = new Dictionary<string, Rect>();
		battleWindowRects = new Dictionary<string, Rect>();
		offScreenRects = new Dictionary<Direction, Rect>();

		// Throw some dice out for fun
		Utilities().ThrowDice(new Roll("d4", 5), false);
		Utilities().ThrowDice(new Roll("d6", 5), false);
		Utilities().ThrowDice(new Roll("d8", 5), false);
		Utilities().ThrowDice(new Roll("d10", 5), false);
		Utilities().ThrowDice(new Roll("d12", 5), false);
		Utilities().ThrowDice(new Roll("d20", 5), false);
		Utilities().ThrowDice(new Roll("d100", 5), false);

		gameMenuRect		= new Rect(0 + GUI_SCREEN_BORDER, 0 + GUI_SCREEN_BORDER,
		                         		Screen.width - GUI_SCREEN_BORDER, Screen.height - GUI_SCREEN_BORDER);

		offScreenRects[Direction.Left]	= new Rect(-Screen.width, 0, Screen.width, Screen.height);
		offScreenRects[Direction.Right]	= new Rect(Screen.width, 0, Screen.width, Screen.height);
		offScreenRects[Direction.Up]	= new Rect(0, -Screen.height, Screen.width, Screen.height);
		offScreenRects[Direction.Down]	= new Rect(0, Screen.height, Screen.width, Screen.height);

		windowRects["Title"] 			= new Rect(offScreenRects[Direction.Right]);
		windowRects["Login"] 			= new Rect(gameMenuRect);
		windowRects["WeaponSelect"] 	= new Rect(gameMenuRect);
		windowRects["PlayerProfile"] 	= new Rect(gameMenuRect);
		windowRects["BattleOver"] 		= new Rect(gameMenuRect);

		battleWindowRects["BattleQueueOn"] 		= new Rect(Screen.width - Screen.width*0.35f, 5f, Screen.width*0.35f, Screen.height*0.3f);
		battleWindowRects["BattleTextOn"] 		= new Rect(Screen.width*0.55f, Screen.height*0.55f, Screen.width*0.45f, Screen.height*0.45f);
		battleWindowRects["ActionSelectionOn"]	= new Rect(Screen.width*0.008f, Screen.height*0.55f, Screen.width*0.55f, Screen.height*0.45f);
		battleWindowRects["BattleStatsOn"]		= new Rect(Screen.width*0.008f, 5f, Screen.width*0.25f, Screen.height*0.55f);

		battleWindowRects["BattleQueueOff"] 	= new Rect(Screen.width, -Screen.height*0.2f, Screen.width*0.25f, Screen.height*0.2f);
		battleWindowRects["BattleTextOff"] 		= new Rect(Screen.width*0.55f, Screen.height, Screen.width*0.45f, Screen.height*0.45f);
		battleWindowRects["ActionSelectionOff"]	= new Rect(Screen.width*0.008f, Screen.height, Screen.width*0.55f, Screen.height*0.45f);
		battleWindowRects["BattleStatsOff"]		= new Rect(-Screen.width*0.2f, -Screen.height*0.55f, Screen.width*0.25f, Screen.height*0.55f);

		windowRects["ActionSelection"] 	= new Rect(battleWindowRects["ActionSelectionOff"]);
		windowRects["BattleStats"] 		= new Rect(battleWindowRects["BattleStatsOff"]);
		windowRects["BattleQueue"] 		= new Rect(battleWindowRects["BattleQueueOff"]);
		windowRects["BattleText"] 		= new Rect(battleWindowRects["BattleTextOff"]);

	}
	
	// Update is called once per frame
	void Update ()
	{
		if(true == bThrowingDice && TITLE_SCREEN_DICE_MAX > diceCount)
		{
			DoGameMenuDice();
		}
		
		if(false == bDoTransition && false == bTransitioningBattleWindows && true == bRewardsAwarded)
		{
			DoRewards();
		}
		
		if(true == bTransitioningBattleWindows)
		{
			if(true == bTransitioningBattleWindowsOn)
			{
				TransitionBattleWindows(true);
			}
			else
			{
				TransitionBattleWindows(false);
			}
		}
		
		if(true == bDoTransition)
		{
			DoTransition();
		}
	}
	
	void OnGUI()
	{
		int windowID = 0;
		GameState gameState = Utilities().getGameState();
		GUI.skin = guiSkin;
		
		if(previousGameState != gameState)
		{
			// Reset the inputs between states
			characterNameInput = "";
			emailInput = "";
			passwordInput = "";
			responseMessage = "";
			
			bDoTransition = true;
			
			switch(gameState)
			{
			case GameState.Title:
				windowRects["Title"] = new Rect(offScreenRects[Direction.Left]);
				bThrowingDice = true;	// execute the dice throwing, demo code
				bDoTitleWindow = true;	// show the title window
				break;
				
			case GameState.Register:
			case GameState.Login:
				windowRects["Login"] = new Rect(offScreenRects[Direction.Right]);
				bDoLoginWindow = true;
				GUI.FocusWindow(windowIDs["Login"]);
				break;
				
			case GameState.WeaponSelection:
				windowRects["WeaponSelect"] = new Rect(offScreenRects[Direction.Right]);
				bDoWeaponSelectWindow = true;
				break;
				
			case GameState.PlayerProfile:
				windowRects["PlayerProfile"] = new Rect(offScreenRects[Direction.Right]);
				bDoPlayerProfileWindow = true;
				break;
				
			case GameState.BattleMode:
				bThrowingDice = false;
				
				// Wait until the transitioning out of the last window before enabling battlemode
				GUI_BattleWindows_PlaceOffScreen();
				bDoBattleWindow = false;
				break;
				
			case GameState.BattleOver:
				windowRects["BattleOver"] = new Rect(offScreenRects[Direction.Right]);
				TransitionBattleWindowsOff();
				bThrowingDice = true;
				bDoBattleOverWindow = true;
				break;
			}
			
			previousGameState = gameState;
		}
		
		windowIDs["Title"] = windowID++;
		GUI_Title_Window(windowIDs["Title"]);
		
		windowIDs["Login"] = windowID++;
		GUI_Login_Window(windowIDs["Login"]);
		
		windowIDs["WeaponSelect"] = windowID++;
		GUI_WeaponSelect_Window(windowIDs["WeaponSelect"]);
		
		windowIDs["PlayerProfile"] = windowID++;
		GUI_PlayerProfile_Window(windowIDs["PlayerProfile"]);
		
		windowID = GUI_BattleMode_Window(windowID);
		
		windowIDs["BattleOver"] = windowID++;
		GUI_BattleOver_Window(windowIDs["BattleOver"]);
	}

	Rect TransitionScreenRect(Rect rect, Direction direction, float target)
	{
		float speed = TransitionSpeed * currentAcceleration * Time.deltaTime;

		// Left Transition or Right Transition
		if((Direction.Left == direction) || (Direction.Right == direction))
		{
			return new Rect(Mathf.MoveTowards(rect.x, target, speed),
			                rect.y, rect.width, rect.height);
		}
		// Up Transition or Down Transition
		else
		{
			return new Rect(rect.x,
			                Mathf.MoveTowards(rect.y, target, speed*2),
			                rect.width, rect.height);
		}
	}

	void ControlAcceleration(Rect rect, Direction direction, Rect targetRect)
	{
		if((Direction.Left == direction && (rect.x > targetRect.x/2))
		   || (Direction.Right == direction && (rect.x < targetRect.x + targetRect.width + targetRect.width/2))
		   || (Direction.Up == direction && (rect.y < targetRect.y/2))
		   || (Direction.Down == direction && (rect.y > targetRect.y + targetRect.height + targetRect.height/2)))
			Mathf.Min(MaxAcceleration, currentAcceleration += 3*Time.deltaTime);
		else
			Mathf.Max(1, currentAcceleration - 3*Time.deltaTime);
	}
	
	bool TransitionToTarget(string currentRect, Direction direction)
	{
		bool result = false;
		Rect offScreenRect = offScreenRects[direction];

		if(null != previousRect && previousRect.Length > 0)			// Ignore this if no previousRect is specified
			windowRects[previousRect] = TransitionScreenRect(windowRects[previousRect], Direction.Right, offScreenRect.x);

		if(currentRect.Length > 0)			// Ignore this if no currentRect is specified
		{
			windowRects[currentRect] = TransitionScreenRect(windowRects[currentRect], Direction.Right, gameMenuRect.x);

			ControlAcceleration(windowRects[currentRect], direction, gameMenuRect);
			
			if(gameMenuRect.x == windowRects[currentRect].x)
			{
				result = true;
				currentAcceleration = 1;
			}
		}
		else if(previousRect.Length > 0)	// If there is no currentRect, base the finishing of this job on the previousRect's transition
		{
			ControlAcceleration(windowRects[previousRect], direction, offScreenRect);

			if(offScreenRect.x == windowRects[previousRect].x)
			{
				result = true;
				currentAcceleration = 1;
			}

		}

		return result;
	}

	void DoTransition()
	{
		bool bFinishedTransitioning = false;
		GameState gameState = Utilities().getGameState();

		switch(gameState)
		{
		case GameState.Title:

			bFinishedTransitioning = TransitionToTarget("Title", Direction.Right);

			if(true == bFinishedTransitioning)
			{
				bDoLoginWindow = false;
				bDoTransition = false;
				previousRect = "Title";
			}

			break;

		case GameState.Register:
		case GameState.Login:

			bFinishedTransitioning = TransitionToTarget("Login", Direction.Left);
			
			if(true == bFinishedTransitioning)
			{
				bDoTitleWindow = false;
				bDoTransition = false;
				previousRect = "Login";
			}
			break;

		case GameState.WeaponSelection:

			bFinishedTransitioning = TransitionToTarget("WeaponSelect", Direction.Left);
			
			if(true == bFinishedTransitioning)
			{
				//windowRects["WeaponSelect"] = new Rect(gameMenuRect);
				bDoLoginWindow = false;
				bDoTransition = false;
				previousRect = "WeaponSelect";
			}
			break;

		case GameState.PlayerProfile:

			bFinishedTransitioning = TransitionToTarget("PlayerProfile", Direction.Left);
			
			if(true == bFinishedTransitioning)
			{
				windowRects["PlayerProfile"] = new Rect(gameMenuRect);

				if(true == previousRect.Equals("BattleOver"))
					bDoBattleOverWindow = false;
				else if(true == previousRect.Equals("Login"))
					bDoLoginWindow = false;
				else if(true == previousRect.Equals("WeaponSelect"))
					bDoWeaponSelectWindow = false;

				bDoTransition = false;
				previousRect = "PlayerProfile";
			}
			break;

		case GameState.BattleMode:
			
			bFinishedTransitioning = TransitionToTarget("", Direction.Left);
			
			if(true == bFinishedTransitioning)
			{
				bDoBattleWindow = true;
				bDoTransition = false;
				bThrowingDice = false;
				TransitionBattleWindowsOn();
				previousRect = "";
			}
			break;

		case GameState.BattleOver:

			if(false == bTransitioningBattleWindows)
			{
				bDoBattleWindow = false;

				bFinishedTransitioning = TransitionToTarget("BattleOver", Direction.Left);
				
				if(true == bFinishedTransitioning)
				{
					bDoTransition = false;
					bThrowingDice = false;
					TransitionBattleWindowsOn();
					previousRect = "BattleOver";
				}
			}

			break;
		}
	}


	void GUI_BattleWindows_PlaceOffScreen()
	{
		// TODO: Place each window-rect at its respective off-screen location instantly, for transitioning in
		windowRects["ActionSelection"] = new Rect(battleWindowRects["ActionSelectionOn"]);
		windowRects["BattleQueue"] = new Rect(battleWindowRects["BattleQueueOn"]);
		windowRects["BattleText"] = new Rect(battleWindowRects["BattleTextOn"]);
		windowRects["BattleStats"] = new Rect(battleWindowRects["BattleStatsOn"]);
	}

	public void TransitionBattleWindowsOff()
	{
		bTransitioningBattleWindowsOn = false;
		bTransitioningBattleWindows = true;
	}

	public void TransitionBattleWindowsOn()
	{
		bTransitioningBattleWindowsOn = true;
		bTransitioningBattleWindows = true;
	}

	void TransitionBattleWindows(bool bOn)
	{
		string onOff = "Off";
		string windowName;
		float speed = TransitionSpeed * currentAcceleration * Time.deltaTime;
	
		if(true == bOn)
		{
			onOff = "On";
		}

		/*
		windowName = "BattleQueue"; 
		windowRects[windowName] = new Rect(Mathf.MoveTowards(windowRects[windowName].x, battleWindowRects[windowName+onOff].x,
		                                                     speed),
		                                   Mathf.MoveTowards(windowRects[windowName].y, battleWindowRects[windowName+onOff].y,
		                  									speed),
		                                   windowRects[windowName].width, windowRects[windowName].height);

		windowName = "BattleStats";
		windowRects[windowName] = new Rect(Mathf.MoveTowards(windowRects[windowName].x, battleWindowRects[windowName+onOff].x,
		                                                     speed),
		                                   Mathf.MoveTowards(windowRects[windowName].y, battleWindowRects[windowName+onOff].y,
		                  									speed),
		                                   windowRects[windowName].width, windowRects[windowName].height);
*/
		windowName = "ActionSelection";
		windowRects[windowName] = new Rect(Mathf.MoveTowards(windowRects[windowName].x, battleWindowRects[windowName+onOff].x,
		                                                     speed),
		                                   Mathf.MoveTowards(windowRects[windowName].y, battleWindowRects[windowName+onOff].y,
		                  									speed),
		                                   windowRects[windowName].width, windowRects[windowName].height);

		/*
		windowName = "BattleText";
		windowRects[windowName] = new Rect(Mathf.MoveTowards(windowRects[windowName].x, battleWindowRects[windowName+onOff].x,
		                                                     speed),
		                                   Mathf.MoveTowards(windowRects[windowName].y, battleWindowRects[windowName+onOff].y,
		                  									speed),
		                                   windowRects[windowName].width, windowRects[windowName].height);
		*/

		bool bActionSelectionDone = windowRects["ActionSelection"].y == battleWindowRects["ActionSelection"+onOff].y;
		bool bBattleQueueDone = true; //windowRects["BattleQueue"].y == battleWindowRects["BattleQueue"+onOff].y;
		bool bBattleStatsDone = true; //windowRects["BattleStats"].y == battleWindowRects["BattleStats"+onOff].y;
		bool bBattleTextDone = true; //windowRects["BattleText"].x == battleWindowRects["BattleText"+onOff].x;

		// Check to see if each window is in its proper place
		if(bActionSelectionDone && bBattleQueueDone && bBattleStatsDone && bBattleTextDone)
		{
			bTransitioningBattleWindows = false;
			currentAcceleration = 0;
		}
		else
		{
			currentAcceleration += 3*Time.deltaTime;
		}
	}

	void GUI_Title_Window(int windowID)
	{
		if(true == bDoTitleWindow) windowRects["Title"] = GUI.Window(windowID, windowRects["Title"], GUI_TitleScreen, "");
	}
	void GUI_Login_Window(int windowID)
	{
		if(true == bDoLoginWindow) windowRects["Login"] = GUI.Window(windowID, windowRects["Login"], GUI_Register_Login, "");
	}

	void GUI_WeaponSelect_Window(int windowID)
	{
		if(true == bDoWeaponSelectWindow) windowRects["WeaponSelect"] = GUI.Window(windowID, windowRects["WeaponSelect"], GUI_WeaponSelection, "");
	}

	void GUI_PlayerProfile_Window(int windowID)
	{
		if(true == bDoPlayerProfileWindow) windowRects["PlayerProfile"] = GUI.Window(windowID, windowRects["PlayerProfile"], GUI_PlayerProfile, "");
	}

	void GUI_BattleOver_Window(int windowID)
	{
		if(true == bDoBattleOverWindow) windowRects["BattleOver"] = GUI.Window(windowID, windowRects["BattleOver"], GUI_BattleOver, "");
	}

	void GUI_TitleScreen(int windowID)
	{
		bSubmitted = false;

		GUI.FocusWindow(windowID);

		AddSpikes(windowRects["Title"].width-80, true);
		
		GUILayout.BeginVertical();

		GUILayout.Space(50);
		GUILayout.Box("Spare Change", "TitleBox");
		GUILayout.Space(100);
		if(GUILayout.Button("Start a New Guy")) 	// TODO: Need a custom style for the menu items
		{
			bRegisteringNew = true;
			Utilities().setGameState(GameState.Register);
		}
		if(GUILayout.Button("Load Existing")) 		// TODO: Need a custom style for the menu items
		{
			bRegisteringNew = false;
			Utilities().setGameState(GameState.Login);
		}
		GUILayout.EndVertical();
	}

	void SubmitInput()
	{
		if(false == bSubmitted &&
		   characterNameInput.Length > 0 &&
		   passwordInput.Length > 0)
		{
			bSubmitted = true;
			if(GameState.Register == Utilities().getGameState())
			{
				responseMessage = "Attempting to Register...";
				// -- Registration: Step 1 - Attempt to create an account with the user's input
				Utilities().Register(characterNameInput, passwordInput, emailInput);
			}
			else
			{
				responseMessage = "Attempting to Load...";
				// -- Attempt to login with the character-name and password
				Utilities().Login(characterNameInput, passwordInput);
			}
		}
	}

	void GUI_Register_Login(int windowID)
	{
		GUI.FocusWindow(windowID);

		AddSpikes(windowRects["Login"].width-80, true);

		GUILayout.BeginVertical();

		GUILayout.Box(string.Format("{0}", bRegisteringNew ? "Register New" : "Load"), "SubTitleBox");

		GUILayout.Label("Character Name");
		characterNameInput = GUILayout.TextField(characterNameInput);
		if (Event.current.Equals(Event.KeyboardEvent("return")))
		{
			SubmitInput();
		}

		if(bRegisteringNew)
		{
			GUILayout.Label("E-Mail (Optional: For Password Retrieval)");
			emailInput = GUILayout.TextField(emailInput);
			if (Event.current.Equals(Event.KeyboardEvent("return")))
			{
				SubmitInput();
			}
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
	}

	bool WeaponSelectItem(Weapon weapon)
	{
		bool selected = false;
		string weaponType = "WeaponTypeIcon";
		string dieColor = "WeaponRoll_";

		if(weapon.type == Weapon.Type.Melee)
			weaponType += "Melee";
		else if(weapon.type == Weapon.Type.Ranged)
			weaponType += "Ranged";
		else if(weapon.type == Weapon.Type.Magical)
			weaponType += "Magical";

		dieColor += weapon.roll.dieName;

		GUILayout.BeginVertical();			// Main Panel

		GUILayout.Box(string.Format("{0}", weapon.name), "WeaponName");

		// Main Weapon Description Area
		GUILayout.BeginHorizontal();		// Weapon Description

		// The Column section containing the Level and Type of the weapon
		GUILayout.BeginVertical();			// Level/Type/Roll
		// Type image
		GUILayout.Box("", weaponType);
		// Level number
		GUILayout.Box(string.Format("{0}",weapon.level), "WeaponLevel");
		// Space to span down to the bottom of this area
		GUILayout.Box(string.Format("{0}{1}", weapon.roll.count, weapon.roll.dieName), dieColor);

		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();			// Level/Type/Roll

		GUILayout.BeginVertical();
		GUILayout.Box(string.Format("{0}", weapon.damageModifier), "WeaponModifier");
		GUILayout.Box(string.Format("{0}", weapon.speedModifier), "WeaponModifier");
		GUILayout.Box(string.Format("{0}", weapon.defenseModifier), "WeaponModifier");
		GUILayout.EndVertical();

		GUILayout.EndHorizontal();			// Weapon Description

		GUILayout.Box("", "Divider");

		GUILayout.Box("Actions", "WeaponActionTitle");
		foreach(Attack attack in weapon.attacks.Values)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Box(attack.name, "WeaponActionTitle");
			GUILayout.Box(string.Format("{0}", attack.hitModifier), "WeaponModifier");
			GUILayout.Box(string.Format("{0}", attack.damageModifier), "WeaponModifier");
			GUILayout.EndHorizontal();
		}

		GUILayout.FlexibleSpace();

		if(GUILayout.Button("Select", "ShortButton"))
		{
			selected = true;
		}

		GUILayout.EndVertical();			// Main Panel

		GUI.Box(GUILayoutUtility.GetLastRect(), "", "WeaponItemOutline");

		return selected;
	}

	void GUI_WeaponSelection(int windowID)
	{
		GUI.FocusWindow(windowID);

		int weaponCount = 0;

		AddSpikes(windowRects["WeaponSelect"].width-80, true);

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();

		GUILayout.FlexibleSpace();

		GUILayout.BeginVertical();

		foreach(Weapon weaponType in Utilities().getWeaponTypes(1))
		{
			bool selected = WeaponSelectItem(weaponType);
			if(selected)
			{
				Utilities().UpdatePlayer(newCharacter.name, 0, 0, 0, 1, weaponType.name);
			}

			GUILayout.Space(20);

			weaponCount++;

			if(2 == weaponCount)
			{
				weaponCount = 0;
				GUILayout.EndVertical();
				GUILayout.BeginVertical();
			}

		}
		GUILayout.EndVertical();

		GUILayout.FlexibleSpace();

		GUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();
	}


	void GUI_PlayerProfile(int windowID)
	{

		Player player = Utilities().getCurrentCharacter();

		AddSpikes(windowRects["PlayerProfile"].width-80, true);

		GUILayout.BeginVertical();

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Box(string.Format("Name: {0}\nLevel: {1}\nXP: {2}\nChange: {3}\nKills: {4}",
									player.name, player.level, player.xp, player.change, player.kills));
		GUILayout.FlexibleSpace();
		WeaponSelectItem(player.weapon);
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

		GUILayout.FlexibleSpace();

		GUILayout.EndVertical();

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
		AddSpikes(windowRects["BattleText"].width);
		//GUILayout.Space(2);

		GUILayout.BeginHorizontal();

		scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		GUILayout.Label(string.Format("{0}", battleText), "PlainText");
		GUILayout.EndScrollView();

		GUILayout.EndHorizontal();
	}

	void GUI_BattleMode_BattleQueue(int windowID)
	{
		AddSpikes(windowRects["BattleQueue"].width);

		GUILayout.BeginHorizontal();

		BattleRound currentTurn = Utilities().getCurrentTurn();
		Queue<BattleRound> queue = Utilities().getBattleQueue();

		if(null != currentTurn)
		{
			string name = Utilities().getCurrentTurnActor().name;
			GUILayout.Box("", "QueueMemberCurrentTurn");
			GUI.Box(GUILayoutUtility.GetLastRect(), string.Format("{0}{1}", name[0], name[1]), "QueueMemberOverlay");
		}

		if(null != queue)
		{
			foreach(BattleRound round in queue)
			{
				GUILayout.Box("", (round.bIsPlayer) ? "QueueMemberPlayer" : "QueueMemberEnemy");
				GUI.Box(GUILayoutUtility.GetLastRect(), string.Format("{0}{1}", round.actor.name[0], round.actor.name[1]), "QueueMemberOverlay");
			}
		}

		GUILayout.EndHorizontal();
	}

	float GUI_BattleMode_BattleStat(BattleActor actor)
	{
		float windowHeight = 0;

		GUILayout.Label(string.Format("{0}", actor.name), "LifeMeterName");
		windowHeight += GUILayoutUtility.GetLastRect().height;

		GUILayout.BeginHorizontal();
		GUILayout.Box("", "LifeMeter");

		Rect tempRect = GUILayoutUtility.GetLastRect();
		
		GUI.Label(tempRect, string.Format("{0}", actor.remainingHealth), "LifeMeterAmount");
		windowHeight += GUILayoutUtility.GetLastRect().height;


		float space = 100f * (((actor.health - actor.remainingHealth) * 1.0f)/actor.health)*tempRect.width;
		GUILayout.Space(space);

		windowHeight += GUILayoutUtility.GetLastRect().height;

		GUILayout.EndHorizontal();


		return windowHeight;
	}

	void GUI_BattleMode_BattleStats(int windowID)
	{
		float windowHeight = 200;

		AddSpikes(windowRects["BattleStats"].width);

		windowHeight += GUI_BattleMode_BattleStat(Utilities().getCurrentCharacter());

		GUILayout.Space(30);
		windowHeight += 30;

		GUILayout.BeginVertical();
		foreach(BattleActor enemy in Utilities().getBattleEnemies())
		{
			GUILayout.Space(10);
			windowHeight += 10;
			windowHeight += GUI_BattleMode_BattleStat(enemy);
		}

		GUILayout.Space(20);
		windowHeight += 20;

		GUILayout.FlexibleSpace();

		GUILayout.EndVertical();

		/* //TODO: Didn't quite figure this out.
		windowRects["BattleStats"] = new Rect(windowRects["BattleStats"].x, windowRects["BattleStats"].y,
		                                      windowRects["BattleStats"].width, windowHeight);
		*/
	}

	void GUI_BattleMode_actionSelection(int windowID)
	{
		AddSpikes(windowRects["ActionSelection"].width);

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

	int GUI_BattleMode_Window(int windowID)
	{
		if(true == bDoBattleWindow)
		{
			/*
			// Test button TODO Remove
			if(GUI.Button(new Rect(Screen.width*0.35f, Screen.height*0.4f, Screen.width*0.2f, Screen.height*0.15f), "End Battle"))
			{
				Utilities().BattleOverTest(false);
			}
			*/

			windowRects["ActionSelection"] = GUI.Window(windowID++, windowRects["ActionSelection"], GUI_BattleMode_actionSelection, "Actions");
			windowRects["BattleText"] = GUI.Window(windowID++, windowRects["BattleText"], GUI_BattleMode_BattleText, "");
			windowRects["BattleQueue"] = GUI.Window(windowID++, windowRects["BattleQueue"], GUI_BattleMode_BattleQueue, "Battle Queue");
			windowRects["BattleStats"] = GUI.Window(windowID++, windowRects["BattleStats"], GUI_BattleMode_BattleStats, "");
		}
		return windowID;
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

	void GUI_BattleOver(int windowID)
	{
		Character playerCharacter = Utilities().getCurrentCharacter();

		if(null == randomWeaponChoices || 0 == randomWeaponChoices.Count)
		{
			randomWeaponChoices = GetRandomWeaponOptions(Utilities().getCurrentCharacter().level, 3);
		}

		AddSpikes(windowRects["BattleOver"].width-80, true);

		GUILayout.BeginVertical();
		GUILayout.Space(10);

		GUILayout.Box("Battle Over", "SubTitleBox"); 		//TODO: Need a custom style for the title
		GUILayout.Box(string.Format("{0} the victor",
									(bPlayerIsVictorious ? playerCharacter.name + " is" : "The Enemy was")),
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
					/*
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
					*/

					WeaponSelectItem(weapon);
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

		GUILayout.EndVertical();
	}

	public void PlayerIsVictorious(float exp, float change, float kills)
	{
		bPlayerIsVictorious = true;
		bRewardsAwarded = true;
		bFinishedRewarding = false;

		awardedExp = exp;
		awardedChange = change;
		numberOfKills = kills;
	}

	public void EnemyIsVictorious()
	{
		bPlayerIsVictorious = false;
		bRewardsAwarded = true;
		bFinishedRewarding = true;
	}


	public void AppendBattleText(string battleTextString)
	{
		//TODO: Could be good to add other decorations to the string
		battleText += string.Format("\n{0}", battleTextString);

		scrollPosition.y = Mathf.Infinity;
	}


	public void SpawnPts(string text, float x, float y, Color color)
	{
		/*
	    x = Mathf.Clamp(x, 0.05f, 0.95f); // clamp position to screen to ensure
	    y = Mathf.Clamp(y, 0.05f, 0.9f);  // the string will be visible
		*/

		x = Mathf.Clamp(x, 0.35f, 0.75f); // clamp position to screen to ensure
		y = Mathf.Clamp(y, 0.35f, 0.7f);  // the string will be visible

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
		Utilities().setGameState(GameState.WeaponSelection);
	}

	// -- Registration: Step 3 - Updating the player's class was successful
	public void UpdatedPlayerSuccessfully()
	{
		GameState gameState = Utilities().getGameState();

		if(GameState.WeaponSelection == gameState)
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


	void DoGameMenuDice()
	{
		diceThrowTimer += Time.deltaTime;

		if(TITLE_SCREEN_DICE_THROW_TIMEOUT <= diceThrowTimer)
		{
			diceThrowTimer = 0;

			Utilities().ThrowDice(new Roll(currentDie, Random.Range(1,4)), false);

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

			diceCount++;
		}
	}

	void DoRewards()
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


	UtilitiesScript utilitiesScript;
	UtilitiesScript Utilities()
	{
		if(null == utilitiesScript)
			utilitiesScript = GameObject.Find("Utilities").GetComponent<UtilitiesScript>();

		return utilitiesScript;
	}

}
