using UnityEngine;
using System.Collections;

using SimpleJSON;

public class ScoreoidInterface : MonoBehaviour {


	private static string API_URL_BASE = "https://api.scoreoid.com/v1/";
	private static string API_KEY = "afd36537472e3fe1124dfe42fd65d47faaf4d1f0";
	private static string GAME_ID = "681f0765ba";
	private static string RESPONSE_TYPE = "JSON";


	// -- Container to transport Scoreoid player data to the game

	public class ScoreoidPlayer
	{
		public string playerName;
		public double change;
		public int numberOfKills;
		public string weapon;
		public int level;
		public double xp;
		public string actorClassName;

		public ScoreoidPlayer(string name, double money, int kills, string inventory, int lvl, double exp, string className)
		{
			playerName = name;
			change = money;
			numberOfKills = kills;
			weapon = inventory;
			level = lvl;
			xp = exp;
			actorClassName = className;
		}
	}


	// -- Requests

	public void CreatePlayer(string playerName, string password)
	{
		WWWForm form = StartScoreoidForm();

		// Add additional fields
        form.AddField( "username", playerName );
		form.AddField( "password", password );
		form.AddField( "money", 0 );
		form.AddField( "kills", 0 );
		form.AddField( "time_played", 0 );
		form.AddField( "inventory", "" );
		form.AddField( "current_level", 1 );
		form.AddField( "xp", 1 );
		form.AddField( "unique_id", "");		// NOTE: Using "unique_id" because there is no "class" field

		SendScoreoidForm("createPlayer", form);
	}

	public void GetPlayer(string playerName, string password)
	{
		WWWForm form = StartScoreoidForm();

		// Add additional fields
        form.AddField( "username", playerName );
		form.AddField( "password", password );

		// Send the form
		SendScoreoidForm("getPlayer", form);
	}

	public void UpdatePlayer(string playerName, double money, double xp, int kills, int current_level, string weapon, string actorClassName)
	{
		WWWForm form = StartScoreoidForm();

		// Add additional fields
        form.AddField( "username", playerName );
		form.AddField( "money", money.ToString());
		form.AddField( "xp", xp.ToString());
		form.AddField( "kills", kills.ToString());
		form.AddField( "current_level", current_level.ToString());
		form.AddField( "inventory", weapon);
		form.AddField( "unique_id", actorClassName);

		// Send the form
		SendScoreoidForm("editPlayer", form);
	}

	/*
	public void UpdateChange(string playerName, double money)
	{
		WWWForm form = StartScoreoidForm();

		form.AddField( "username", playerName );
		form.AddField( "field", "money");
		form.AddField( "value", money.ToString());

		SendScoreoidForm("updatePlayerField", form);
	}

	public void UpdateKills(string playerName, int kills)
	{
		WWWForm form = StartScoreoidForm();

		form.AddField( "username", playerName );
		form.AddField( "field", "kills");
		form.AddField( "value", kills.ToString());

		SendScoreoidForm("updatePlayerField", form);
	}

	public void UpdateWeapon(string playerName, string weapon)
	{
		WWWForm form = StartScoreoidForm();

		form.AddField( "username", playerName );
		form.AddField( "field", "inventory");
		form.AddField( "value", weapon);

		SendScoreoidForm("updatePlayerField", form);
	}

	public void UpdateLevel(string playerName, int level)
	{
		WWWForm form = StartScoreoidForm();

		form.AddField( "username", playerName );
		form.AddField( "field", "current_level");
		form.AddField( "value", level.ToString());

		SendScoreoidForm("updatePlayerField", form);
	}

	public void UpdateXP(string playerName, int xp)
	{
		WWWForm form = StartScoreoidForm();

		form.AddField( "username", playerName );
		form.AddField( "field", "xp");
		form.AddField( "value", xp.ToString());

		SendScoreoidForm("updatePlayerField", form);
	}

	public void UpdateClass(string playerName, string actorClassName)
	{
		WWWForm form = StartScoreoidForm();

		form.AddField( "username", playerName );
		form.AddField( "field", "unique_id");		// NOTE: Using "unique_id" because there is no "class" field
		form.AddField( "value", actorClassName);

		SendScoreoidForm("updatePlayerField", form);
	}
	*/


	// -- Utils

	WWWForm StartScoreoidForm()
	{
		/* Unity WWW Class used for HTTP Request */
        WWWForm form = new WWWForm();

        form.AddField( "api_key", API_KEY );
        form.AddField( "game_id", GAME_ID );
        form.AddField( "response", RESPONSE_TYPE );

		return form;
	}

	void SendScoreoidForm(string request, WWWForm form)
	{
		// Setup the request url to be sent to Scoreoid
		WWW www = new WWW(API_URL_BASE + request, form);
		// Send the request in this coroutine so as not to wait busily
        StartCoroutine(WaitForRequest(www));
	}

	IEnumerator WaitForRequest(WWW www)
    {
        yield return www;

        /* Check for errors */
        if(www.error == null)
        {
			ParseScoreoidJSON(www.text, www.url);
        }
		else
		{
			Debug.Log("WWW Error: "+ www.error+" for URL: "+www.url);

			ProcessScoreoidError(www);
        }
    }

	void ProcessScoreoidError(WWW www)
	{
		Utilities().RequestFailed(www.error);
	}


	// -- Response Parsing

	void ParseScoreoidJSON(string theJSONString, string url)
	{
		var N = JSON.Parse(theJSONString);

		string errorString = N["error"];
		if(null != errorString)
		{
			Debug.Log(string.Format("Scoreoid Error: {0}", N["error"]));
			Utilities().RequestFailed(N["error"]);
		}
		else
		{
			string command = url.Replace(API_URL_BASE, "");

			switch(command)
			{
			case "editPlayer":
				Utilities().UpdatedPlayerSuccessfully();
				break;
			case "createPlayer":
				Utilities().PlayerRegistered();
				break;
			case "getPlayer":
				// Parse the player into the ScoreoidPlayer.
				ScoreoidPlayer player =
					new ScoreoidPlayer( N[0]["Player"]["username"],
									 	N[0]["Player"]["money"].AsDouble,
										N[0]["Player"]["kills"].AsInt,
										N[0]["Player"]["inventory"],
										N[0]["Player"]["current_level"].AsInt,
										N[0]["Player"]["xp"].AsDouble,
										N[0]["Player"]["unique_id"]);

				// Set the player
				Utilities().PlayerLoggedIn(player);
				break;
			default:
				Debug.Log(string.Format("Unhandled command: {0}", command));
				break;
			}
		}
	}

	// -- Utilities Script Access

	UtilitiesScript utilitiesScript;
	UtilitiesScript Utilities()
	{
		if(null == utilitiesScript)
			utilitiesScript = GameObject.Find("Utilities").GetComponent<UtilitiesScript>();

		return utilitiesScript;
	}
}
