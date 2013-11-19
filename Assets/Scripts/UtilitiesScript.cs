using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// -- GameController
using GameState = GameControllerScript.GameState;
using EnemyDefinition = GameControllerScript.EnemyDefinition;
using BattleAction = GameControllerScript.BattleAction;
using Weapon = GameControllerScript.Weapon;

// -- Dice Controller
using Roll = DiceControllerScript.Roll;

// -- Battle Controller
using BattleRound = BattleControllerScript.BattleRound;
using BattleActor = BattleControllerScript.BattleActor;
using Player = BattleControllerScript.Player;

// -- Scoreoid
using ScoreoidPlayer = ScoreoidInterface.ScoreoidPlayer;

public class UtilitiesScript : MonoBehaviour {


	// -- Dice Box

	GameObject diceBox;
	public Transform getDiceBox()
	{
		if(null == diceBox)
			diceBox = GameObject.Find("DiceBox");
		
		return diceBox.transform;
	}


	// -- BattleController

	BattleControllerScript battleControllerScript;
	public BattleControllerScript BattleController()
	{
		if(null == battleControllerScript)
			battleControllerScript = GameObject.Find("BattleController").GetComponent<BattleControllerScript>();
		
		return battleControllerScript;
	}

	public void StartBattle()
	{
		BattleController().StartBattle();
	}

	public void AwardRewards()
	{
		BattleController().AwardRewards();
	}

	public List<BattleActor> getBattleEnemies()
	{
		return BattleController().getBattleEnemies();
	}

	public BattleActor getCurrentTurnActor()
	{
		return BattleController().getCurrentTurnActor();
	}

	public Queue<BattleRound> getBattleQueue()
	{
		return BattleController().getBattleQueue();
	}

	public BattleRound getCurrentTurn()
	{
		return BattleController().getCurrentTurn();
	}

	public BattleRound.State getBattleRoundState()
	{
		return BattleController().getBattleRoundState();
	}

	public void setBattleRoundState(BattleRound.State state)
	{
		BattleController().setBattleRoundState(state);
	}

	public void BattleActorActed()
	{
		BattleController().BattleActorActed();
	}

	public void SelectedAction(BattleAction action)
	{
		BattleController().SelectedAction(action);
	}

	public void SelectedEnemy(BattleActor actor)
	{
		BattleController().SelectedEnemy(actor);
	}

	// -- Dice Controller

	DiceControllerScript diceControllerScript;
	public DiceControllerScript DiceController()
	{
		if(null == diceControllerScript)
			diceControllerScript = GameObject.Find("DiceController").GetComponent<DiceControllerScript>();

		return diceControllerScript;
	}

	public void ThrowDice(Roll roll)
	{
		DiceController().ThrowDice(roll);
	}
	public void ClearDice()
	{
		DiceController().ClearDice();
	}

	public void DiceRolled(GameObject die, int rollValue)
	{
		// Pass this method call to the battle controller
		DiceThrower().DiceRolled(die, rollValue);

		//// Low - priority
		//TODO: In this file, track which dice are rolled, and by what object.
		// As a Dice is Rolled (DiceRolled), the appropriate caller should be
		// sent a message (SendMessage()) for "DiceRolled" and that object should
		// implement a function with this one's prototype.
		////
	}


	// -- "Dice Thrower"

	//TODO: Allow other throwers
	public BattleControllerScript DiceThrower()
	{
		// the BattleController is the presumed thrower, at this time.
		return BattleController();
	}


	// -- GUI Controller

	GUIControllerScript guiControllerScript;
	public GUIControllerScript GUIController()
	{
		if(null == guiControllerScript)
			guiControllerScript = GameObject.Find("GUIController").GetComponent<GUIControllerScript>();

		return guiControllerScript;
	}

	public void PlayerLoggedIn(ScoreoidPlayer player)
	{
		GUIController().PlayerLoggedIn(player);
	}

	public void PlayerRegistered()
	{
		GUIController().PlayerRegistered();
	}

	public void UpdatedPlayerSuccessfully()
	{
		GUIController().UpdatedPlayerSuccessfully();
	}

	public void RequestFailed(string error)
	{
		GUIController().RequestFailed(error);
	}


	// -- GUI

	public void AppendBattleText(string newBattleText)
	{
		GUIController().AppendBattleText(newBattleText);
	}

	public void SpawnPts(string text, float x, float y, Color color)
	{
		GUIController().SpawnPts(text, x, y, color);
	}

	// -- Score Keeper

	ScoreoidInterface scoreoidInterface;
	public ScoreoidInterface ScoreKeeper()
	{
		if(null == scoreoidInterface)
			scoreoidInterface = GameObject.Find("ScoreKeeper").GetComponent<ScoreoidInterface>();

		return scoreoidInterface;
	}

	public void Register(string playerName, string password)
	{
		ScoreKeeper().CreatePlayer(playerName, password);
	}
	public void Login(string playerName, string password)
	{
		ScoreKeeper().GetPlayer(playerName, password);
	}
	public void UpdatePlayer(string playerName, float money, float xp, int kills, int current_level, string weapon)
	{
		ScoreKeeper().UpdatePlayer(playerName, money, xp, kills, current_level, weapon);
	}

	/*
	public void UpdateChange(string playerName, float money)
	{
		ScoreKeeper().UpdateChange(playerName, money);
	}
	public void UpdateKills(string playerName, int kills)
	{
		ScoreKeeper().UpdateKills(playerName, kills);
	}
	public void UpdateWeapon(string playerName, string weapon)
	{
		ScoreKeeper().UpdateWeapon(playerName, weapon);
	}
	public void UpdateLevel(string playerName, int level)
	{
		ScoreKeeper().UpdateLevel(playerName, level);
	}
	public void UpdateXP(string playerName, int xp)
	{
		ScoreKeeper().UpdateXP(playerName, xp);
	}
	public void UpdateClass(string playerName, string actorClassName)
	{
		ScoreKeeper().UpdateClass(playerName, actorClassName);
	}
	*/


	// -- Game Controller

	GameControllerScript gameControllerScript;
	public GameControllerScript GameController()
	{
		if(null == gameControllerScript)
			gameControllerScript = GameObject.Find("GameController").GetComponent<GameControllerScript>();

		return gameControllerScript;
	}


	// -- Game State

	public void setGameState(GameState newGameState)
	{
		GameController().setGameState(newGameState);
	}

	public GameState getGameState()
	{
		return GameController().getGameState();
	}


	// -- Player / Character

	public Player getCurrentCharacter()
	{
		return GameController().getCurrentCharacter();
	}

	public void setCurrentCharacter(ScoreoidPlayer character)
	{
		GameController().setCurrentCharacter(character);
	}


	// -- Camera

	public void ResetCamera()
	{
		Camera.main.GetComponent<CamControl>().ResetCamera();
	}

	public void LookAtDice(List<GameObject> dice)
	{
		Camera.main.GetComponent<CamControl>().LookAtDice(dice);
	}


	// -- Object Definitions

	public Weapon getWeapon(string weaponName)
	{
		return GameController().getWeapon(weaponName);
	}

	public List<Weapon> getWeaponTypes(int level)
	{
		return GameController().getWeaponTypes(level);
	}

	public EnemyDefinition getEnemy(string enemyName)
	{
		return GameController().getEnemy(enemyName);
	}
}
