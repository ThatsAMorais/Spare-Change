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
	public Texture d4Icon;
	public Texture d6Icon;
	public Texture d8Icon;
	public Texture d10Icon;
	public Texture d12Icon;
	public Texture d20Icon;
	public Texture d100Icon;
	
	private static float REWARD_TICK_TIMEOUT = 1;
	
	private static int TITLE_SCREEN_DICE_MAX = 15;
	private static float TITLE_SCREEN_DICE_THROW_TIMEOUT = 15;

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

	public class BattleTextEntry
	{
		//Dictionary<
	}

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
	bool bRandomWeaponChoicesSet;

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
	bool bDoInstructionsWindow;
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

	Dictionary<string,Texture> diceIcons;

	NewCharacter character;

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

		diceIcons = new Dictionary<string, Texture>();
		diceIcons.Add("d4",d4Icon);
		diceIcons.Add("d6",d6Icon);
		diceIcons.Add("d8",d8Icon);
		diceIcons.Add("d10",d10Icon);
		diceIcons.Add("d12",d12Icon);
		diceIcons.Add("d20",d20Icon);
		diceIcons.Add("d100",d100Icon);

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

		currentGUIColor = GUI.color;

		// Throw some dice out for fun
		Utilities().ThrowDice(new Roll("d4", 1), false);
		Utilities().ThrowDice(new Roll("d6", 1), false);
		Utilities().ThrowDice(new Roll("d8", 1), false);
		Utilities().ThrowDice(new Roll("d10", 1), false);
		Utilities().ThrowDice(new Roll("d12", 1), false);
		Utilities().ThrowDice(new Roll("d20", 1), false);
		Utilities().ThrowDice(new Roll("d100", 1), false);

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
		windowRects["Instructions"]		= new Rect(gameMenuRect);

		battleWindowRects["BattleQueueOn"] 		= new Rect(Screen.width - Screen.width*0.35f, 5f, Screen.width*0.35f, Screen.height*0.3f);
		battleWindowRects["BattleTextOn"] 		= new Rect(Screen.width*0.45f, 5f, Screen.width - Screen.width*0.45f, Screen.height*0.45f);
		battleWindowRects["ActionSelectionOn"]	= new Rect(Screen.width*0.008f, Screen.height*0.6f, Screen.width*0.5f, Screen.height*0.4f);
		battleWindowRects["BattleStatsOn"]		= new Rect(Screen.width - Screen.width*0.25f,
		                                               		Screen.height*0.4f,
		                                               		Screen.width*0.25f, Screen.height*0.6f);
		battleWindowRects["CurrentWeaponOn"]	= new Rect(Screen.width*0.008f, 5f, Screen.width*0.3f, Screen.height*0.65f);
		/*= new Rect(Screen.width*0.5f, Screen.height*0.45f,
		           Screen.width*0.25f, Screen.height*0.55f);*/

		battleWindowRects["BattleQueueOff"] 	= new Rect(Screen.width, -Screen.height*0.2f, Screen.width*0.25f, Screen.height*0.2f);
		battleWindowRects["BattleTextOff"] 		= new Rect(Screen.width, -Screen.height*45f, Screen.width*0.45f, Screen.height*0.45f);
		battleWindowRects["ActionSelectionOff"]	= new Rect(Screen.width*0.008f, Screen.height, Screen.width*0.55f, Screen.height*0.45f);
		battleWindowRects["BattleStatsOff"]		= new Rect(-Screen.width*0.2f, -Screen.height*0.6f, Screen.width*0.25f, Screen.height*0.6f);
		battleWindowRects["CurrentWeaponOff"]	= new Rect(Screen.width*0.55f, Screen.height, Screen.width*0.25f, Screen.height*0.55f);

		windowRects["ActionSelection"] 	= new Rect(battleWindowRects["ActionSelectionOff"]);
		windowRects["BattleStats"] 		= new Rect(battleWindowRects["BattleStatsOff"]);
		windowRects["BattleQueue"] 		= new Rect(battleWindowRects["BattleQueueOff"]);
		windowRects["BattleText"] 		= new Rect(battleWindowRects["BattleTextOff"]);
		windowRects["CurrentWeapon"]	= new Rect(battleWindowRects["CurrentWeaponOff"]);


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

		FadeWindows(bFadeWindows);
	}
	
	void OnGUI()
	{
		int windowID = 0;
		GameState gameState = Utilities().getGameState();
		GUI.skin = guiSkin;
		GUI.color = currentGUIColor;
		
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

			case GameState.Instructions:
				windowRects["Instructions"] = new Rect(offScreenRects[Direction.Right]);
				bDoInstructionsWindow = true;
				GUI.FocusWindow(windowIDs["Instructions"]);
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
				FadeWindows(true);
				bDoBattleWindow = false;
				bThrowingDice = true;
				bDoBattleOverWindow = true;
				
				bFinishedRewarding = false;
				bChangedWeapon = false;
				bUpdatingWeapon = false;
				bRandomWeaponChoicesSet = false;
				break;
			}
			
			previousGameState = gameState;
		}
		
		windowIDs["Title"] = windowID++;
		GUI_Title_Window(windowIDs["Title"]);
		
		windowIDs["Login"] = windowID++;
		GUI_Login_Window(windowIDs["Login"]);

		windowIDs["Instructions"] = windowID++;
		GUI_Instructions_Window(windowIDs["Instructions"]);

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

			bFadeWindows = false;

			bFinishedTransitioning = TransitionToTarget("Title", Direction.Right);

			if(true == bFinishedTransitioning)
			{
				bDoLoginWindow = false;
				bDoInstructionsWindow = false;
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

		case GameState.Instructions:
			
			bFinishedTransitioning = TransitionToTarget("Instructions", Direction.Left);
			
			if(true == bFinishedTransitioning)
			{
				bDoTitleWindow = false;
				bDoTransition = false;
				previousRect = "Instructions";
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

			bStartedBattle = false;

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
		
			bFadeWindows = false;
			bRandomWeaponChoicesSet = false;

			if(false == bTransitioningBattleWindows)
			{
				bDoBattleWindow = false;

				bFinishedTransitioning = TransitionToTarget("BattleOver", Direction.Left);
				
				if(true == bFinishedTransitioning)
				{
					GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 1f);
					bDoTransition = false;

					previousRect = "BattleOver";
				}
			}

			break;
		}
	}


	// TODO: Kinda deprecated, as is obvious if you read this and see that all are left on
	void GUI_BattleWindows_PlaceOffScreen()
	{
		windowRects["ActionSelection"] = new Rect(battleWindowRects["ActionSelectionOn"]);
		windowRects["BattleQueue"] = new Rect(battleWindowRects["BattleQueueOn"]);
		windowRects["BattleText"] = new Rect(battleWindowRects["BattleTextOn"]);
		windowRects["BattleStats"] = new Rect(battleWindowRects["BattleStatsOn"]);
		windowRects["CurrentWeapon"] = new Rect(battleWindowRects["CurrentWeaponOn"]);
	}

	bool bFadeWindows = false;

	public void TransitionBattleWindowsOff()
	{
		if(Utilities().getGameState().Equals(GameState.BattleMode))
		{
			bTransitioningBattleWindowsOn = false;
			bTransitioningBattleWindows = true;
			bFadeWindows = true;
		}
	}

	public void TransitionBattleWindowsOn()
	{
		bTransitioningBattleWindowsOn = true;
		bTransitioningBattleWindows = true;
		bFadeWindows = false;
	}

	static float MINIMUM_OPACITY = .4f;
	static float MAXIMUM_ACCELERATION = 10;
	Color currentGUIColor;

	void FadeWindows(bool bOn)
	{
		float targetOpacity = 0.85f;
		
		if(true == bOn)
		{
			targetOpacity = MINIMUM_OPACITY;
		}

		currentGUIColor = new Color(currentGUIColor.r, currentGUIColor.g, currentGUIColor.b, Mathf.MoveTowards(currentGUIColor.a, targetOpacity, 0.75f*Time.deltaTime));
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
			currentAcceleration = Mathf.MoveTowards(currentAcceleration, MAXIMUM_ACCELERATION, 2*Time.deltaTime);
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

	void GUI_Instructions_Window(int windowID)
	{
		if(true == bDoInstructionsWindow) windowRects["Instructions"] = GUI.Window(windowID, windowRects["Instructions"], GUI_Instructions, "");
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
		if(GUILayout.Button("Instructions")) 		// TODO: Need a custom style for the menu items
		{
			Utilities().setGameState(GameState.Instructions);
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

		GUILayout.FlexibleSpace();

		GUILayout.Label("Character Name", "LegendaryText");
		characterNameInput = GUILayout.TextField(characterNameInput);
		if (Event.current.Equals(Event.KeyboardEvent("return")))
		{
			SubmitInput();
		}

		if(bRegisteringNew)
		{
			GUILayout.Label("E-Mail (Optional: For Password Retrieval)", "LegendaryText");
			emailInput = GUILayout.TextField(emailInput);
			if (Event.current.Equals(Event.KeyboardEvent("return")))
			{
				SubmitInput();
			}
		}

		GUILayout.Label("Password", "LegendaryText");
		passwordInput = GUILayout.PasswordField(passwordInput, "*"[0]);
		if (Event.current.Equals(Event.KeyboardEvent("return")))
		{
			SubmitInput();
		}

		GUILayout.Space (30);

		GUILayout.BeginHorizontal();	// | #Submit# <---> "message" <---> #Back# |
		if(GUILayout.Button("Back"))
		{
			Utilities().setGameState(GameState.Title);
		}

		GUILayout.FlexibleSpace();

		if(false == responseMessage.Equals(""))
			GUILayout.Box(responseMessage, "LegendaryText");

		GUILayout.FlexibleSpace();
		
		if(GUILayout.Button("Submit"))
		{
			SubmitInput();
		}

		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
	}

	void Instructions_Title(string title, string description)
	{
		GUILayout.Label(title, "LegendaryText", GUILayout.Height(10));
		GUILayout.Space(5);
		if(description.Length > 0)
		{
			GUILayout.Label(description, "PlainText");
			GUILayout.Space(5);
		}
	}

	void Instructions_Element(string elementName, string description)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(elementName + ":", "ShortLabel", GUILayout.Width(200));
		GUILayout.Space(15);
		GUILayout.Label(description, "CursedText");
		GUILayout.EndHorizontal();
	}

	Vector2 instructionScroll = Vector2.zero;
	void GUI_Instructions(int windowID)
	{
		List<GUILayoutOption> instructionPanelOptions = new List<GUILayoutOption>();
		instructionPanelOptions.Add(GUILayout.Width(windowRects["Instructions"].width-140));
		instructionPanelOptions.Add(GUILayout.Height(windowRects["Instructions"].height-300));

		GUI.FocusWindow(windowID);
		
		AddSpikes(windowRects["Instructions"].width-80, true);

		GUILayout.BeginVertical();

		GUILayout.Label("Instructions", "InstructionsTitleBox");

		GUILayout.Space(20);

		instructionScroll = GUILayout.BeginScrollView(instructionScroll);

		GUILayout.BeginVertical();

		GUILayout.Space(10);
		GUILayout.Box("", "Divider");
		GUILayout.Space(10);

		GUILayout.BeginHorizontal(instructionPanelOptions.ToArray());
		GUILayout.BeginVertical();
		Instructions_Title("Overview", "Spare Change is about battling foes, collecting change, choosing weapons, and unlocking new challenges.");
		Instructions_Element("Registration", "Players create a Character Name and Password.");
		Instructions_Element("Loading", "Players may Login and continue their progress.");
		Instructions_Element("Profile","The Profile is the buffer between each Battle where the Player's Stats are displayed");
		Instructions_Element("Battling", "Battles are randomized based on level, and present a variety of foes.");
		Instructions_Element("Rewards", "Players are awarded XP and Spare Change for defeating all foes.");
		Instructions_Element("Changing Weapons", "Upon completion of a Battle a Player may choose a new weapon.");
		Instructions_Element("Leveling Up","By earning enough XP, the player will increase in Health, Gain access to new Weapons, and face new Enemies");
		Instructions_Element("Character Saving","After each battle, the Character's stats are updated to the Spare Change scorekeeper.");
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();

		GUILayout.Space(10);
		GUILayout.Box("", "Divider");
		GUILayout.Space(10);
		
		GUILayout.BeginHorizontal(instructionPanelOptions.ToArray());
		GUILayout.BeginVertical();
		Instructions_Title("Battling", "Where Change and XP are awarded.");
		Instructions_Element("Turn-based", "Combatants take turns in a queue based on their Speed + Modifiers.");
		Instructions_Element("Selecting Actions", "Attack choices are presented based on the Player's Weapon");
		Instructions_Element("Roll Chance-to-Hit", "Combatants must roll 10 or better on a d20, + modifiers, to Hit an opponent.");
		Instructions_Element("Roll Damage", "Upon a successful Hit, the weapons Damage Roll is thrown, and total damage calculated");
		Instructions_Element("Battle Finish", "The Battle ends when the Player kills all Enemies or is killed.");
		Instructions_Element("Rewards","For defeating the enemies, XP and Spare Change are awarded.");
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();

		GUILayout.Space(10);
		GUILayout.Box("", "Divider");
		GUILayout.Space(10);

		GUILayout.BeginHorizontal(instructionPanelOptions.ToArray());
		GUILayout.BeginVertical();
		Instructions_Title("The Player Card", "The card that all Players carry.");
		Instructions_Element("Character Name", "...of Legend.");
		Instructions_Element("Level", "Current XP Level achieved by this Character.");
		Instructions_Element("XP", "Number of Experience Points from winning battles.");
		Instructions_Element("Spare Change", "Money collected.");
		Instructions_Element("Kills", "Number of kills achieved in battle.");
		Instructions_Element("Weapon", "Brief description of the Equipped Weapon.");
		Instructions_Element("Boss Battle Indicator", "Indicates the Next Battle will be a Boss.");
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
		
		GUILayout.Space(10);
		
		GUILayout.BeginVertical();
		Weapon bat = Utilities().getWeapon("Bat");
		Player samplePlayer = new Player("Chu-chu-chuck Changes", bat, 25, 10, 6);
		PlayerCard(samplePlayer, true);
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();

		GUILayout.Space(10);
		GUILayout.Box("", "Divider");
		GUILayout.Space(10);

		GUILayout.BeginHorizontal(instructionPanelOptions.ToArray());
		GUILayout.BeginVertical();
		Instructions_Title("The Weapon Card", "A Weapon Card displays the details of a Weapon:");
		Instructions_Element("Weapon Type Icon", "Melee, Ranged, or Magic.");
		Instructions_Element("Weapon Name", "What you call it.");
		Instructions_Element("Damage Roll", "Type and Quantity of Dice rolled for Damage.");
		Instructions_Element("Damage Roll Icon", "Icon displaying the Type of Dice");
		Instructions_Element("Level", "A measure of the Weapon's quality.");
		Instructions_Element("Damage Modifier", "Affects all Damage rolls.");
		Instructions_Element("Speed Modifier", "Affects the weilder's Initiative in battle.");
		Instructions_Element("Defense Modifier", "Affects Chance-To-Hit rolls targeted at the weilder.");
		Instructions_Element("Actions", "Battle Options and their modifiers when used.");
		Instructions_Element("Action Hit Modifier", "Affects the ToHit.");
		Instructions_Element("Action Damage Modifier", "Affects Damage.");
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();

		GUILayout.Space(10);

		GUILayout.BeginVertical();
		WeaponSelectItem(bat);
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();

		GUILayout.Space(10);
		GUILayout.Box("", "Divider");
		GUILayout.Space(10);

		GUILayout.EndVertical(); // scrolling panel container

		GUILayout.EndScrollView();


		// | #Back# <---> |
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Back"))
		{
			Utilities().setGameState(GameState.Title);
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.EndVertical();
	}

	void WeaponItemModifier(string modifierName, int stat, string style="WeaponModifier", bool bShowPlus=true)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(5);
		GUILayout.Box(string.Format("{0}", modifierName), "LightOutlineText");
		GUILayout.FlexibleSpace();
		GUILayout.Box(string.Format("{0}{1}", (bShowPlus && stat > 0 ? "+" : ""), stat), style);
		GUILayout.Space(5);
		GUILayout.EndHorizontal();
	}

	void WeaponItemTopBar(string name, Weapon.Type type, Roll roll)
	{
		string weaponType = "WeaponTypeIcon";
		string dieColor = "WeaponRoll_";
		
		dieColor += roll.dieName;
		
		if(type == Weapon.Type.Melee)
			weaponType += "Melee";
		else if(type == Weapon.Type.Ranged)
			weaponType += "Ranged";
		else if(type == Weapon.Type.Magical)
			weaponType += "Magical";

		GUILayout.BeginHorizontal(GUILayout.Height(25));

		GUILayout.Space(10);
		
		GUILayout.Box("", weaponType, GUILayout.Width(30), GUILayout.Height(30));
		GUILayout.FlexibleSpace();
		GUILayout.Box(string.Format("{0}", name), "ShortLabel", GUILayout.Width(180));
		GUILayout.FlexibleSpace();
		GUILayout.Box(string.Format("{0}{1}", roll.count, roll.dieName), "WeaponLevel", GUILayout.Width(30), GUILayout.Height(30));
		//GUILayout.Box(string.Format("{0}", roll.count), dieColor, GUILayout.Width(30), GUILayout.Height(30));

		GUILayout.Space(10);
		
		GUILayout.EndHorizontal();
	}

	void WeaponItemDetails(Weapon weapon)
	{
		GUILayout.BeginHorizontal();

		GUILayout.Space(15);
		
		// Type image
		GUILayout.Box(diceIcons[weapon.roll.dieName], GUILayout.Width(85), GUILayout.Height(85));

		GUILayout.FlexibleSpace();

		// Vertical column of modifiers
		GUILayout.BeginVertical();

		WeaponItemModifier("Level: ", weapon.level, "WeaponLevel", false);
		WeaponItemModifier("Damage: ", weapon.damageModifier, weapon.damageModifier < 0 ? "WeaponModifierNegative" : "WeaponModifier" );
		WeaponItemModifier("Speed: ", weapon.speedModifier, weapon.speedModifier > 0 ? "WeaponModifierNegative" : "WeaponModifier" );
		WeaponItemModifier("Defense: ", weapon.defenseModifier, weapon.defenseModifier > 0 ? "WeaponModifierNegative" : "WeaponModifier");

		GUILayout.EndVertical();

		GUILayout.Space(15);
		
		GUILayout.EndHorizontal();
	}

	void WeaponItemAttacks(Dictionary<string,Attack>.ValueCollection attacks)
	{
		int attackCount = 0;

		GUILayout.Box("Actions", "WeaponActionTitle");
		GUILayout.BeginHorizontal();
		GUILayout.Space(10);
		GUILayout.BeginVertical();
		GUILayout.Box("", "Divider");
		foreach(Attack attack in attacks)
		{
			attackCount++;
			GUILayout.BeginHorizontal();

			GUILayout.Box(attack.name, "WeaponActionName", GUILayout.Width(100));
			WeaponItemModifier("Hit: ", attack.hitModifier, attack.hitModifier < 0 ? "WeaponModifierNegative" : "WeaponModifier" );
			GUILayout.Space(5);
			WeaponItemModifier("Dmg: ", attack.damageModifier, attack.damageModifier < 0 ? "WeaponModifierNegative" : "WeaponModifier" );

			GUILayout.EndHorizontal();

			GUILayout.Box("", "Divider");
		}

		GUILayout.EndVertical();
		GUILayout.Space(10);
		GUILayout.EndHorizontal();
	}

	static float WEAPON_SELECT_ITEM_WIDTH = 210;
	static float WEAPON_SELECT_ITEM_HEIGHT = 350;

	bool WeaponSelectItem(Weapon weapon, bool bSelectable=false)
	{
		bool selected = false;
		Rect lastRect;

		if(null == weapon)
			return false;

		/******/
		GUILayout.BeginVertical(GUILayout.Width(WEAPON_SELECT_ITEM_WIDTH), GUILayout.Height(WEAPON_SELECT_ITEM_HEIGHT));

		GUILayout.Space(10);

		WeaponItemTopBar(weapon.name, weapon.type, weapon.roll);

		GUILayout.Box("", "Divider");

		WeaponItemDetails(weapon);

		GUILayout.Box("", "Divider");

		WeaponItemAttacks(weapon.attacks.Values);

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if(bSelectable && (GUILayout.Button("Select", "ShortButton")))
		{
			selected = true;
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.Space(10);

		GUILayout.EndVertical();

		lastRect = GUILayoutUtility.GetLastRect();

		GUI.Box(lastRect, "");

		return selected;
	}

	Vector2 weaponSelectScrollview = Vector2.zero;

	void GUI_WeaponSelection(int windowID)
	{
		GUI.FocusWindow(windowID);

		int weaponCount = 0;

		AddSpikes(windowRects["WeaponSelect"].width-80, true);

		GUILayout.FlexibleSpace();

		weaponSelectScrollview = GUILayout.BeginScrollView(weaponSelectScrollview);

		GUILayout.BeginHorizontal();
		foreach(Weapon weaponType in Utilities().getWeaponTypes(0))
		{
			bool selected = WeaponSelectItem(weaponType, true);
			if(selected)
			{
				Utilities().UpdatePlayer(character.name, 0, 0, 0, 1, weaponType.name);
			}

			GUILayout.Space(10);

			weaponCount++;

		}
		GUILayout.EndHorizontal();

		GUILayout.EndScrollView();
	
		GUILayout.FlexibleSpace();
	}

	void PlayerCard(Player player, bool bBossFightNextIndicator=false)
	{
		Rect lastRect;
		
		if(null == player)
			return;
		
		/******/
		GUILayout.BeginVertical(GUILayout.Width(WEAPON_SELECT_ITEM_WIDTH)
		                        //, GUILayout.Height(WEAPON_SELECT_ITEM_HEIGHT)
		                        );
		
		GUILayout.Space(10);
		
		GUILayout.BeginHorizontal(GUILayout.Height(25));
		GUILayout.Space(15);
		GUILayout.Box(string.Format("{0}", player.name), "ShortLabel");
		GUILayout.Space(15);
		GUILayout.EndHorizontal();
		
		GUILayout.Box("", "Divider");

		GUILayout.BeginHorizontal();
		GUILayout.Space(25);
		GUILayout.BeginVertical();
		WeaponItemModifier("Level: ", player.level, "WeaponLevel", false);
		WeaponItemModifier("XP: ", (int)player.xp, "WeaponLevel", false);
		WeaponItemModifier("Spare Change: ", (int)player.change, "WeaponLevel", false);
		WeaponItemModifier("Health: ", (int)player.health, "WeaponLevel", false);
		WeaponItemModifier("Kills: ", player.kills, "WeaponLevel", false);
		GUILayout.EndVertical();
		GUILayout.Space(25);
		GUILayout.EndHorizontal();
		
		GUILayout.Box("", "Divider");
		
		GUILayout.Box("Weapon", "WeaponActionTitle");

		GUILayout.BeginHorizontal();
		GUILayout.Space(10);
		GUILayout.BeginVertical();
		GUILayout.Box("", "Divider");

		GUILayout.BeginHorizontal();
		GUILayout.Box(player.weapon.name, "WeaponActionName", GUILayout.Width(90));

		GUILayout.Box(string.Format("{0}{1}", player.weapon.roll.count, player.weapon.roll.dieName), "WeaponLevel", GUILayout.Width(30), GUILayout.Height(30));

		WeaponItemModifier("Level: ", player.weapon.level, "WeaponLevel", false);

		GUILayout.EndHorizontal();
		
		GUILayout.Box("", "Divider");

		if((Utilities().PlayerIsAtBossFight(player)))
			GUILayout.Box("Boss Fight Next Battle","ShortLabel");

		GUILayout.EndVertical();
		GUILayout.Space(10);
		GUILayout.EndHorizontal();
		
		GUILayout.FlexibleSpace();

		GUILayout.Space(10);
		
		GUILayout.EndVertical();
		
		lastRect = GUILayoutUtility.GetLastRect();
		
		GUI.Box(lastRect, "");
	}



	bool bStartedBattle = false;
	void GUI_PlayerProfile(int windowID)
	{
		Player player = Utilities().getCurrentCharacter();

		AddSpikes(windowRects["PlayerProfile"].width-80, true);

		GUILayout.BeginVertical();
		GUILayout.Space(10);
		
		GUILayout.Box("Profile", "SubTitleBox");

		GUILayout.BeginHorizontal();

		GUILayout.FlexibleSpace();

		GUILayout.BeginVertical();
		GUILayout.Space(10);
		Instructions_Title("Player", "");
		GUILayout.Space(10);
		PlayerCard(player);
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();

		GUILayout.FlexibleSpace();

		GUILayout.BeginVertical();
		GUILayout.Space(10);
		Instructions_Title("Weapon", "");
		GUILayout.Space(10);
		WeaponSelectItem(player.weapon);
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();

		GUILayout.FlexibleSpace();

		GUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if(GUILayout.Button("Log Off"))
		{
			Utilities().setGameState(GameState.Title);
		}
		GUILayout.Space(100);
		if(GUILayout.Button("Start Battle"))
		{
			if(!bStartedBattle)
			{
				bStartedBattle = true;
				// Now that the battle is initialized, switch the gamestate to battle-mode
				Utilities().StartBattle();
				Utilities().setGameState(GameState.BattleMode);
			}
		}

		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		//GUILayout.FlexibleSpace();

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
		int buttonCount = 0;

		GUILayout.Box("Select an Action", "CurrentAction");
		////
		GUILayout.FlexibleSpace();
		////
		GUILayout.BeginHorizontal();
		foreach(BattleAction action in Utilities().getCurrentTurnActor().getActions())
		{
			int hitModifier = action.hitModifier;
			string hitModifierText = "";
			int dmgModifier = (action as Attack).damageModifier;
			string dmgModifierText = "";

			buttonCount++;

			if(hitModifier != 0)
			{
				hitModifierText = string.Format("(Hit:{0}{1})", (hitModifier > 0) ? "+" : "", hitModifier.ToString());
			}

			if(dmgModifier != 0)
			{
				dmgModifierText = string.Format("(Dmg:{0}{1})", (dmgModifier > 0) ? "+" : "", dmgModifier.ToString());
			}

			if(GUILayout.Button(string.Format("{0} {1} {2}", action.name, hitModifierText, dmgModifierText), GUILayout.MaxWidth(300)))
			{
				Utilities().SelectedAction(action);
			}
			GUILayout.FlexibleSpace();
			// newline
			if(buttonCount >= 2)
			{
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
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

		//foreach()
		// TODO: Loop over the BattleText elements
		GUILayout.Label(string.Format("{0}", battleText), "PlainText");

		GUILayout.EndScrollView();

		GUILayout.EndHorizontal();
	}

	void GUI_BattleMode_CurrentWeapon(int windowID)
	{
		AddSpikes(windowRects["CurrentWeapon"].width);
		//GUILayout.Space(2);
		BattleActor actor = Utilities().getCurrentTurnActor();

		if(null == actor)
		{
			return;
		}

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		WeaponSelectItem(actor.weapon);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	void GUI_BattleMode_BattleQueue()
	{
		float width_height = 50;
		Rect rect;

		Queue<BattleRound> queue = Utilities().getBattleQueue();

		GUILayout.BeginVertical();
		GUILayout.Box("Battle Queue", "PlainText");

		GUILayout.Box("", "Divider");

		GUILayout.BeginHorizontal();

		if(null != queue)
		{
			foreach(BattleRound round in queue)
			{
				//GUILayout.Box("", (round.bIsPlayer) ? "QueueMember" : "QueueMemberEnemy");
				GUILayout.BeginVertical();
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Box("", "QueueMember", GUILayout.Width(width_height), GUILayout.Height(width_height));
				rect = GUILayoutUtility.GetLastRect();
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.EndVertical();

				GUI.Box(rect, string.Format("{0}{1}", round.actor.name[0], round.actor.name[1]), "QueueMemberOverlay");
				GUILayout.Space(7);
				width_height -= 5;
			}
		}

		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
	}

	void GUI_BattleMode_BattleStat(BattleActor actor)
	{
		string lifeMeterStyle = "LifeMeterName";
		BattleRound currentTurn = Utilities().getCurrentTurn();

		GUILayout.BeginHorizontal();

		if((null != currentTurn) && (actor == currentTurn.actor))
			GUILayout.Box("","QueueCurrentTurnIcon");
		else
			GUILayout.Space(24);

		GUILayout.BeginVertical();

		if((null != currentTurn) && (actor == Utilities().getCurrentCharacter()))
			lifeMeterStyle = "LifeMeterNamePlayer";
	
		GUILayout.Label(string.Format("{0}", actor.name), lifeMeterStyle);										// Actor name

		GUILayout.BeginHorizontal();
		GUILayout.Box("", "LifeMeter");																			// Life-meter ribbon

		Rect tempRect = GUILayoutUtility.GetLastRect();
		
		GUI.Label(tempRect, string.Format("{0}", actor.remainingHealth), "LifeMeterAmount");					// Life amount

		float space = 100f * (((actor.health - actor.remainingHealth) * 1.0f)/actor.health)*tempRect.width;
		GUILayout.Space(space);																					// Space repr Health Lost
		GUILayout.EndHorizontal();

		GUILayout.EndVertical();

		GUILayout.EndHorizontal();
	}

	void GUI_BattleMode_BattleStats(int windowID)
	{
		AddSpikes(windowRects["BattleStats"].width);

		GUILayout.BeginVertical();
		
		GUI_BattleMode_BattleStat(Utilities().getCurrentCharacter());

		foreach(BattleActor enemy in Utilities().getBattleEnemies())
		{
			GUILayout.Space(15);
			GUI_BattleMode_BattleStat(enemy);
		}

		GUILayout.Space(3);
		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUI_BattleMode_BattleQueue();
		GUILayout.EndHorizontal();

		GUILayout.EndVertical();
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

	
	List<Weapon> GetRandomWeaponOptions(int level, int count)
	{
		List<Weapon> allWeaponsForLevel = Utilities().getWeaponTypes(level);
		List<Weapon> randomSelection = new List<Weapon>();

		for(int w=0; w < count && 0 < allWeaponsForLevel.Count; w++)
		{
			int randomIndex = Random.Range(0, randomSelection.Count);
			Weapon randomWeapon = allWeaponsForLevel[randomIndex];
			randomSelection.Add(randomWeapon);
			allWeaponsForLevel.Remove(randomWeapon);	// Don't show the same weapon more than once
		}
		
		return randomSelection;
	}
	
	Vector2 changeWeaponScroll = Vector3.zero;
	bool bUpdatingWeapon = false;

	void GUI_BattleOver(int windowID)
	{
		Player player = Utilities().getCurrentCharacter();
		
		AddSpikes(windowRects["BattleOver"].width-80, true);
		
		GUILayout.BeginVertical();
		GUILayout.Space(10);
		
		GUILayout.Box("Battle Over", "SubTitleBox");
		
		GUILayout.BeginHorizontal();
		GUILayout.Space(15);
		
		GUILayout.BeginVertical();
		
		GUILayout.Box(string.Format("{0} the victor",
		                            (bPlayerIsVictorious ? player.name + " is" : "The Enemy was")),
		              "LegendaryText");

		/*
		GUILayout.Label(string.Format("Spare Change: {0}", playerCharacter.change), "LegendaryText");
		GUILayout.Space(5);
		GUILayout.Label(string.Format("Experience: {0}", playerCharacter.xp), "LegendaryText");
		GUILayout.Space(5);
		GUILayout.Label(string.Format("Kills: {0}", playerCharacter.kills), "LegendaryText");
		*/

		PlayerCard(player);

		GUILayout.FlexibleSpace();
		
		if(true == bFinishedRewarding && !(bChangedWeapon && bUpdatingWeapon))
		{
			GUILayout.BeginHorizontal();
			
			if(GUILayout.Button("Return to Player Profile"))
			{
				Utilities().setGameState(GameState.PlayerProfile);
			}
			
			GUILayout.EndHorizontal();
		}
		
		//GUILayout.FlexibleSpace();
		
		if(false == responseMessage.Equals(""))
			GUILayout.Box(responseMessage, "LegendaryText");
		
		GUILayout.EndVertical();
		
		GUILayout.Space(15);
		
		if(true == bFinishedRewarding)
		{
			if(false == bChangedWeapon)
			{
				if(false == bRandomWeaponChoicesSet)
				{
					randomWeaponChoices = GetRandomWeaponOptions(Utilities().getCurrentCharacter().level, 3);
					bRandomWeaponChoicesSet = true;
				}
				
				// Show possible weapon change options, here
				GUILayout.BeginVertical();
				
				GUILayout.Label("Change Weapons?", "LegendaryText", GUILayout.Height(55));
				
				changeWeaponScroll = GUILayout.BeginScrollView(changeWeaponScroll);
				GUILayout.BeginHorizontal();
				foreach(Weapon weapon in randomWeaponChoices)
				{
					if(WeaponSelectItem(weapon, true))
					{
						responseMessage = "Updating Weapon...";
						bChangedWeapon = true;
						bUpdatingWeapon = true;
						bRandomWeaponChoicesSet = false;
						
						player.weapon = weapon;
						
						Utilities().UpdatePlayer(player.name, player.change, player.xp,
						                         player.kills, player.level, player.weapon.name);
					}
				}
				GUILayout.EndHorizontal();
				GUILayout.EndScrollView();
				
				GUILayout.Space(15);
				GUILayout.EndVertical();
			}
			/*
			else
			{
				GUILayout.FlexibleSpace();
			}*/
		}
		/*
		else
		{
			GUILayout.FlexibleSpace();
		}
		*/
		
		GUILayout.EndHorizontal();
		
		GUILayout.EndVertical();
	}

	int GUI_BattleMode_Window(int windowID)
	{
		if(true == bDoBattleWindow)
		{
			windowRects["ActionSelection"] = GUI.Window(windowID++, windowRects["ActionSelection"], GUI_BattleMode_actionSelection, "Actions");
			windowRects["BattleText"] = GUI.Window(windowID++, windowRects["BattleText"], GUI_BattleMode_BattleText, "History");
			//windowRects["BattleQueue"] = GUI.Window(windowID++, windowRects["BattleQueue"], GUI_BattleMode_BattleQueue, "Battle Queue");
			windowRects["BattleStats"] = GUI.Window(windowID++, windowRects["BattleStats"], GUI_BattleMode_BattleStats, "Fighters");
			windowRects["CurrentWeapon"] = GUI.Window(windowID++,windowRects["CurrentWeapon"], GUI_BattleMode_CurrentWeapon, "Current Weapon");
		}
		return windowID;
	}


	public void PlayerIsVictorious(float exp, float change, float kills)
	{
		bPlayerIsVictorious = true;
		bRewardsAwarded = true;
		bFinishedRewarding = false;
		bUpdatingWeapon = false;

		awardedExp = exp;
		awardedChange = change;
		numberOfKills = kills;
	}

	public void EnemyIsVictorious()
	{
		bPlayerIsVictorious = false;
		bRewardsAwarded = true;
		bFinishedRewarding = true;
		bUpdatingWeapon = false;
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

		x = Mathf.Clamp(x, 0.05f, 0.85f); // clamp position to screen to ensure
		y = Mathf.Clamp(y, 0.05f, 0.95f);  // the string will be visible

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
		/**HACK: The only reason this is stored in this case is to support if a player registers, does not select a weapon,
		 	logs in later, and is brought to the weaponselect screen.  The weaponselection screen is designed to read a new-
		 	character object so we fill this out just in case.  ***/
		// Start a new character object
		character = new NewCharacter();	//TODO: Since this is global, should be forcefully dereferenced
		// Store these details between this step and the weapon selection phase
		character.name = player.playerName;
		character.password = passwordInput;

		Utilities().setCurrentCharacter(player);
	}

	// -- Registration: Step 2 - Username and Password were available, and a "Player" was created
	public void PlayerRegistered()
	{
		// Start a new character object
		character = new NewCharacter();

		// Store these details between this step and the character selection phase
		character.name = characterNameInput;
		character.password = passwordInput;

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
			Utilities().Login(character.name, character.password);
		}
		else if(GameState.BattleOver == gameState)
		{
			if(bChangedWeapon && bUpdatingWeapon)
			{
				bUpdatingWeapon = false;
				responseMessage = "Weapon Updated!";
			}
			else
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

			Utilities().ThrowDice(new Roll(currentDie, Random.Range(1,2)), false);

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
