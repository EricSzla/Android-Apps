using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using LitJson;
using System;
using Facebook.Unity;
using UnityEngine.Advertisements;



public class GameManager : MonoBehaviour {

	public class FriendsPic
	{
		public string id { get; set; }
		public Sprite Happy { get; set; }
		public Sprite Sad { get; set; }
		public Sprite Angry { get; set; }
	}
		
	public static GameManager Instance{ set; get; }

	private const float SLICE_FORCE_REQUIRED = 50;
	public GameObject vegetablePrefab;
	public Transform trail;

	public static List<Vegetable> veggies = new List<Vegetable> ();
	private float lastSpawn;
	private float deltaSpawn = 0.8f;
	private Vector3 lastMousePosition;
	private Collider2D[] veggiesCols;

	// ------------------- UI Part of the game -------------------
	private static int score;
	private static int highScore;
	private int lifepoint;
	public RawImage ProfilePic;
	public Text scoreText;
	public Text highScoreText;
	public Text loadingText;
	public Image[] lifepoints;
	public GameObject pauseMenu;
	public GameObject deathMenu;
	public GameObject deathMenuAd;
	public GameObject loadingMenu;
	private int deathCounter = 0;
	public List<FriendsPic> friendsPicObject;
	public static List<object> Friends;
	private bool loadStandard = false;
	private bool isPaused = false;
	public Sprite[] coinSprites;
	public GameObject coinImage;
	public Text coinText;
	private int coinCounter;
	private float coinTime;
	private bool callCoin = false;
	private float callCoinTime;
	private int coinAmmount;
	// Counter on start of the game
	public Sprite[] counterSprites;
	public GameObject counterImage;
	public GameObject counterMenu;
	private bool counterOn = false;
	float timer;
	int timerCounter = 0;

	public static int HighScore {
		get { return highScore; }
		set { highScore = value; }
	}
	public static int Score {
		get { return score; }
		set { score = value; }
	}
		
	// ------------------- PowerUps -------------------
	private bool isFreezed = false;
	private int freezedTime;
	private int freezePowerUpLeft = 0;
	private bool isShieldOn = false;
	private int shieldTime;
	private int shieldPowerUpLeft = 0;
	private int lifePowerUpLeft = 0;
	private int firePowerUpLeft = 0;
	// Active/Deactive icons
	public GameObject fireBombActive;
	public GameObject fireBombDeactive;
	public GameObject freezeBombActive;
	public GameObject freezeBombDeactive;
	public GameObject shieldBombActive;
	public GameObject shieldBombDeactive;
	public GameObject lifeActive;
	public GameObject lifeDeactive;
	//Backgrounds
	public GameObject freezedBackground;
	public GameObject shieldBackground;
	public GameObject extraLifeBackground;

	//Sounds
	private float soundTime;
	private bool allowSliceSound = true;

	//Ads
	bool afterAd = false;

	#region Game
	public void NewGame(){
		if (!afterAd) {
			foreach (Image i in lifepoints)
				i.enabled = true;
			score = 0;
			lifepoint = 3;
			ResetValues ();
		} else {
			deathMenu.SetActive (false);
			deathMenuAd.SetActive (false);
			ResetValues ();
			lifepoint = 1;
			lifepoints[0].enabled = true;
			lifepoints [1].enabled = false;
			lifepoints [2].enabled = false;
			afterAd = false;
		}

		foreach (Vegetable v in veggies)
			Destroy (v.gameObject);

		veggies.Clear ();
	}

	public void ResetValues(){
		foreach (Image i in lifepoints)
			i.enabled = true;

		foreach (Vegetable v in veggies)
			Destroy (v.gameObject);

		veggies.Clear ();

		firePowerUpLeft = PlayerPrefs.GetInt ("firePowerUpLeft");
		deathCounter = PlayerPrefs.GetInt ("deathCount");
		freezePowerUpLeft = PlayerPrefs.GetInt ("freezePowerUpLeft");
		shieldPowerUpLeft = PlayerPrefs.GetInt ("shieldPowerUpLeft");
		lifePowerUpLeft = PlayerPrefs.GetInt ("lifePowerUpLeft");
		coinAmmount = PlayerPrefs.GetInt ("CoinAmmount");
		coinText.text = "x " + coinAmmount.ToString ();
		CheckActivePowerUps ();
		pauseMenu.SetActive (false);
		deathMenu.SetActive (false);
		deathMenuAd.SetActive (false);
		loadingMenu.SetActive (false);
		scoreText.text = score.ToString ();
		highScore = PlayerPrefs.GetInt ("Score");
		highScoreText.text = "Best Score: " + highScore.ToString ();
		Time.timeScale = 1f;
		timer = Time.time;
		timerCounter = 0;
		counterOn = true;
		counterMenu.SetActive (true);
		isPaused = true;
		coinCounter = 0;
		coinTime = Time.time;
		callCoinTime = Time.time;
		soundTime = Time.time;
		counterImage.GetComponent<Image> ().overrideSprite = counterSprites [0];
	}

	private void Awake(){
		Instance = this;
		Screen.orientation = ScreenOrientation.LandscapeRight;
		Debug.Log ("Awake");
	}

	private void Start(){
		Screen.orientation = ScreenOrientation.LandscapeRight;
		ProfilePic.texture = GameStateManager.UserTexture;
		veggiesCols = new Collider2D[0];
		loadingMenu.SetActive (false);
		counterMenu.SetActive (false);
		Debug.Log ("Start");
		if (friendsPicObject == null || friendsPicObject.Count <= 0) {
			LoadingPause ();
			Debug.Log ("Loading menu loaded");
			if (Friends != null) {
				if (Friends.Count > 0) {
					Debug.Log (Friends.Count.ToString ());
					friendsPicObject = new List<FriendsPic> (Friends.Count);
					for (int i = 0; i < Friends.Count; i++) {

						var friend = GameManager.Friends [i] as Dictionary<string,object>;
						string friendId = friend ["id"] as string;

						Debug.Log ("Loading friends images to texture");
						if (loadingText.text.Length == 9) {
							loadingText.text = "Loading";
						} else if (loadingText.text.Length < 9) {
							loadingText.text += ".";
						}
						Debug.Log ("Friends id: " + friendId);
						StartCoroutine (GetFriendsPics (friendId, i));
					}
				} else {
					//TODO: Load standard images ! 
					LoadingPause ();
					loadStandard = true;
					NewGame ();
				}
			} else{
				LoadingPause ();
				loadStandard = true;
				NewGame ();
			}
		} else {
			loadStandard = true;
			NewGame ();
		}
	}

	private void Update(){

		foreach (Vegetable v in veggies) {
			if (v.isSliced) {
				v.setGravity ();
				veggies.Remove (v);
			}
		}

		if (counterOn) {
			if (Time.time - timer > 1.2f) {
				if (counterImage.GetComponent<Animation> ().enabled) {
					counterImage.GetComponent<Animation> ().enabled = false;
				}

				if (timerCounter == 3) {
					isPaused = false;
					counterOn = false;
					counterMenu.SetActive (false);
				}

				if (timerCounter < 3) {
					counterImage.GetComponent<Image> ().overrideSprite = counterSprites [timerCounter];
					counterImage.GetComponent<Animation> ().enabled = true;
					counterImage.GetComponent<Animation> ().Play ();
					timer = Time.time;
					timerCounter++;
				}
			}


		}
		if (isPaused)
			return;
	
		CheckFreezePowerUp (); // Checks if freez powerup is on, if so freezes the objects
		CheckShieldPowerUp ();
			
		if (Time.time - coinTime > 0.1f) {
			if (coinCounter > 5) {
				coinCounter = 0;
			}
			coinImage.GetComponent<Image> ().sprite = coinSprites [coinCounter];
			//coinImage.GetComponent<SpriteRenderer> ().sprite = coinSprites [coinCounter];
			coinCounter++;
			coinTime = Time.time;
		}

		if ((Time.time - callCoinTime > 35.0f) && !callCoin) {
			callCoin = true;
			callCoinTime = Time.time;
			CreateVeggie ();
		} else {
			callCoin = false;
		}

		if (isFreezed) {
			if (Time.time - lastSpawn > (deltaSpawn * 2)) {
				CreateVeggie ();
			}
		} else {
			if (Time.time - lastSpawn > deltaSpawn) {
				CreateVeggie ();
			}
		}

		if(Input.GetMouseButtonUp(0)){
			allowSliceSound = true;
		}
			
		if (Input.GetMouseButton (0)) {
			Vector3 position = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			position.z = -1;
			trail.position = position;


			if (Time.time - soundTime > 0.3f && allowSliceSound) {
				SoundManager.Instance.PlaySound (1);
				soundTime = Time.time;
			}

			Collider2D[] thisFramesVeggie = Physics2D.OverlapPointAll (new Vector2(position.x, position.y),LayerMask.GetMask("Vegetable"));
			if ((Input.mousePosition - lastMousePosition).sqrMagnitude > SLICE_FORCE_REQUIRED) {
				foreach (Collider2D c2 in thisFramesVeggie) {
					for (int i = 0; i < veggiesCols.Length; i++) {
						if (c2 == veggiesCols [i]) {
							c2.GetComponent<Vegetable>().Slice (false);
							c2.GetComponent<Vegetable> ().Animate ();
						}
					}
				}
			}

			veggiesCols = thisFramesVeggie;
			lastMousePosition = Input.mousePosition;
			allowSliceSound = false;
		}
	}

	private Vegetable GetVegetable(){
		Vegetable v = veggies.Find (x => !x.isActive);

		if (v == null) {
			v = Instantiate (vegetablePrefab).GetComponent<Vegetable> ();
			veggies.Add (v);
		}

		return v;
	}

	private void CreateVeggie(){
		Vegetable v = GetVegetable();
		if (friendsPicObject != null) {
			if (friendsPicObject.Count <= 0) {
				loadStandard = true;
			} else {
				loadStandard = false;
			}
		} else {
			loadStandard = true;
		}

		float randomX = UnityEngine.Random.Range (-1.65f, 2.65f);
		float velocity = UnityEngine.Random.Range (1.85f, 2.75f);
		//float vegSpeed = UnityEngine.Random.Range (-1.65f, 2.65f);
		float freezeSpeed;
		float gravity;

		if (isFreezed) {
			gravity = 0.3f;
			freezeSpeed = 0.1f;
		} else {
			gravity = 2.0f;
			freezeSpeed = 1.0f;
		}

		if (callCoin) {
			callCoin = false;
			v.LaunchCoin (velocity, randomX, -randomX, freezeSpeed, gravity);
		}else if (loadStandard) {
			v.LaunchVegetable (velocity, randomX, -randomX,freezeSpeed,gravity);
		} else {
			int randomPic = UnityEngine.Random.Range (0, friendsPicObject.Count);
			v.LaunchVegetable (velocity, randomX, -randomX, friendsPicObject [randomPic].Happy, friendsPicObject [randomPic].Sad, friendsPicObject [randomPic].Angry,freezeSpeed,gravity);
		}

		lastSpawn = Time.time;
	}
		
	public void IncrementScore(int scoreAmmount){
		score += scoreAmmount;
		scoreText.text = score.ToString ();
		if (score > highScore) {
			highScore = score;
			highScoreText.text = "Best Score: " + highScore.ToString ();
			PlayerPrefs.SetInt ("Score", highScore);
		}
	}

	public void IncrementCoin(int incrementAmmount){
		//TODO: Increment score
		coinAmmount += incrementAmmount;
		PlayerPrefs.SetInt ("CoinAmmount", coinAmmount);
		coinText.text = "x " + coinAmmount;
	}

	public void LoseLP(){

		if (!isShieldOn) {
			if (lifepoint == 0) {
				lifepoint--;
				Death ();
				return;
			} else if (lifepoint > 0) {
				lifepoint--;
				lifepoints [lifepoint].enabled = false;

				if (lifepoint == 0) {
					lifepoint--;
					Death ();
				}
			}
		}
	}

	public void Death(){
		isPaused = true;
		deathCounter++;
		if (deathCounter == 2) {
			deathCounter = 0;
		}
	
		PlayerPrefs.SetInt ("deathCount", deathCounter);

		Debug.Log ("iterating");

		if (deathCounter % 2 == 0) {
			deathMenuAd.SetActive (false);
			deathMenu.SetActive (true);
		} else {
			deathMenu.SetActive (false);
			deathMenuAd.SetActive (true);
		}
	}
	#endregion Game
	#region Menu
	public void PauseGame(){
		pauseMenu.SetActive (!pauseMenu.activeSelf);
		isPaused = pauseMenu.activeSelf;
		Time.timeScale = (Time.timeScale == 0) ? 1 : 0;
	}

	public void LoadingPause(){
		loadingMenu.SetActive (!loadingMenu.activeSelf);
		isPaused = loadingMenu.activeSelf;
		Time.timeScale = (Time.timeScale == 0) ? 1 : 0;
	}

	public void ToMenuPause(){
		Time.timeScale = (Time.timeScale == 0) ? 1 : 0;
		SoundManager.Instance.PlaySound (0);
		FBGraph.SetScores ();
		SceneManager.LoadScene ("MainMenu");
	}

	public void ToMenu(){
		SoundManager.Instance.PlaySound (0);
		FBGraph.SetScores ();
		SceneManager.LoadScene ("MainMenu");
	}

	public void RestartPause(){
		Time.timeScale = (Time.timeScale == 0) ? 1 : 0;
		SoundManager.Instance.PlaySound (0);
		FBGraph.SetScores ();
		SceneManager.LoadScene ("Game");
	}
		
	public void Restart(){
		SoundManager.Instance.PlaySound (0);
		FBGraph.SetScores ();
		SceneManager.LoadScene ("Game");
	}
	#endregion Menu
	#region PowerUps

	private void CheckActivePowerUps(){
		if (firePowerUpLeft <= 0) {
			fireBombActive.SetActive (false);
			fireBombDeactive.SetActive (true);
		} else {
			fireBombActive.SetActive (true);
			fireBombDeactive.SetActive (false);
		}

		if (freezePowerUpLeft <= 0) {
			freezeBombActive.SetActive (false);
			freezeBombDeactive.SetActive (true);
		} else {
			freezeBombActive.SetActive (true);
			freezeBombDeactive.SetActive (false);
		}

		if (shieldPowerUpLeft <= 0) {
			shieldBombActive.SetActive (false);
			shieldBombDeactive.SetActive (true);
		} else {
			shieldBombActive.SetActive (true);
			shieldBombDeactive.SetActive (false);
		}

		if (lifePowerUpLeft <= 0) {
			lifeActive.SetActive (false);
			lifeDeactive.SetActive (true);
		} else {
			lifeActive.SetActive (true);
			lifeDeactive.SetActive (false);
		}
	}

	public void FirePowerUp(){
		if (firePowerUpLeft > 0) {
			SoundManager.Instance.PlaySound (4);
			foreach (Vegetable v in veggies) {
				v.Slice (true);
			}
			firePowerUpLeft--;
			PlayerPrefs.SetInt ("firePowerUpLeft", firePowerUpLeft);
			CheckActivePowerUps ();
		} else {
			SoundManager.Instance.PlaySound (8);
		}
	}


	public void FreezePowerUp(){
		if (freezePowerUpLeft > 0) {
			SoundManager.Instance.PlaySound (5);
			freezedTime = Time.frameCount;
			isFreezed = true;
			freezePowerUpLeft--;
			PlayerPrefs.SetInt ("freezePowerUpLeft",freezePowerUpLeft);
			freezedBackground.SetActive (!freezedBackground.activeSelf);
			CheckActivePowerUps ();
		} else {
			SoundManager.Instance.PlaySound (8);
		}
	}

	private void CheckFreezePowerUp(){
		foreach (Vegetable v in veggies) {
			if (v.isSliced) {
				v.setGravity ();
			} else {
				if (isFreezed) {
					if (Time.frameCount - freezedTime < 350) {
						v.setFreezeGravity ();
					} else {
						v.setGravity ();
					}
				}
			}
		}
		if (Time.frameCount - freezedTime > 350) {
			isFreezed = false;
			freezedBackground.SetActive (false);
		}
	}

	public void ShieldPowerUp(){
		if (shieldPowerUpLeft > 0) {
			SoundManager.Instance.PlaySound (6);
			isShieldOn = true;
			shieldTime = Time.frameCount;
			shieldPowerUpLeft--;
			PlayerPrefs.SetInt ("shieldPowerUpLeft", shieldPowerUpLeft);
			shieldBackground.SetActive (true);
			CheckActivePowerUps ();
		} else {
			SoundManager.Instance.PlaySound (8);
		}
	}

	private void CheckShieldPowerUp(){
		if (Time.frameCount - shieldTime > 350) {
			isShieldOn = false;
			shieldBackground.SetActive (false);

		}
	}

	public void LifePowerUp(){
		if (lifePowerUpLeft > 0) {
			if (lifepoint < 3) {
				SoundManager.Instance.PlaySound (7);
				lifepoints [lifepoint].enabled = true;
				lifepoint = (lifepoint + 1);
				lifePowerUpLeft--;	

				PlayerPrefs.SetInt ("lifePowerUpLeft",lifePowerUpLeft);

				CheckActivePowerUps ();
			}
		} else {
			SoundManager.Instance.PlaySound (8);
		}
	}

	#endregion PowerUps
	#region Pictures
	public static Texture2D LoadPNG(byte[] fileData) {

		Texture2D tex = null;	
		tex = new Texture2D(2, 2);
		tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
		return tex;
	}

	private IEnumerator GetFriendsPics(String friendsId, int i){
		Debug.Log ("Getting friends from database");
		var form = new WWWForm(); //here you create a new form connection
		var URL = "http://erpamdevelopment.com/Apps/FriendSlice/AppScripts/GetAllSelfies.php"; //change for your URL
		form.AddField( "fbId", AccessToken.CurrentAccessToken.UserId ); //add your hash code to the field myform_hash, check that this variable name is the same as in PHP file
		form.AddField( "friendsFbId",friendsId);
		var w = new WWW(URL, form); //here we create a var called 'w' and we sync with our URL and the form
		yield return w; //we wait for the form to check the PHP file, so our game dont just hang
		if (w.error != null) {
			print(w.error); //if there is an error, tell us
		} else {
			print("Test ok");
			string dataFound = w.text; //here we return the data our PHP told us
			w.Dispose(); //clear our form in game
			Debug.Log(dataFound);
			JsonData pics = JsonMapper.ToObject (dataFound);
			Debug.Log (pics [0]);
			Debug.Log (pics [1]);
			string happyPic = pics [0].ToString ();
			string sadPic = pics [1].ToString ();
			string angryPic = pics [2].ToString ();
			Sprite happySprite = null;
			Sprite sadSprite = null;
			Sprite angrySprite = null;
			if (!happyPic.StartsWith ("ERROR") && !happyPic.Equals ("ERROR")) {
				byte[] happyPicData = Convert.FromBase64String (happyPic);
				Texture2D tex = LoadPNG (happyPicData);
				happySprite = ConvertToSprite (tex);
			} else {
				happySprite = LoadStandardSprite (0);
			}
			if (!sadPic .StartsWith ("ERROR") && !sadPic .Equals ("ERROR")) {
				byte[] happyPicData = Convert.FromBase64String (sadPic);
				Texture2D tex = LoadPNG (happyPicData);
				sadSprite = ConvertToSprite (tex);
			}else {
				sadSprite = LoadStandardSprite (1);
			}

			if (!angryPic .StartsWith ("ERROR") && !angryPic .Equals ("ERROR")) {
				byte[] happyPicData = Convert.FromBase64String (angryPic);
				Texture2D tex = LoadPNG (happyPicData);
				angrySprite = ConvertToSprite (tex);
			}else {
				angrySprite = LoadStandardSprite (2);
			}

			Debug.Log ("Before assigning sprites");
			//friendsPicObject[i] = new FriendsPic { id = friendsId, Happy = happySprite, Sad = sadSprite, Angry = angrySprite };
			FriendsPic friendsPic = new FriendsPic();
			friendsPic.id = friendsId;
			friendsPic.Happy = happySprite;
			friendsPic.Sad = sadSprite;
			friendsPic.Angry = angrySprite;
			friendsPicObject.Add (friendsPic);
			Debug.Log ("After assigning sprites");
		}

		if (i >= Friends.Count-1) {
			LoadingPause ();
			NewGame ();
		}
		Debug.Log ("End of CheckSelfies()");
	}

	public Sprite ConvertToSprite(Texture2D pic1){
		//Rect rec = new Rect(28, 0, 80 , 128);
		Rect rec = new Rect(0,0,128,128);
		Sprite sprite = Sprite.Create(pic1,rec,new Vector2(0.5f,0.5f),771);
		return sprite;
	}

	private Sprite LoadStandardSprite(int what){
		Sprite face = null;
		if (what == 0) {
			face = Resources.Load <Sprite> ("Sprites/head");
		} else if (what == 1) {
			face = Resources.Load <Sprite> ("Sprites/head2");
		} else if (what == 2) {
			face = Resources.Load <Sprite> ("Sprites/head3");
		} else {
			face = Resources.Load <Sprite> ("Sprites/head");
		}
		return face;
	}
	#endregion Pictures
	#region Ads
	public void ShowRewardedAd()
	{
		SoundManager.Instance.PlaySound (0);
		Debug.Log ("Showing Ad");
		if (Advertisement.IsReady("rewardedVideo"))
		{
			Debug.Log ("Ad Ready");
			var options = new ShowOptions { resultCallback = HandleShowResult };
			Advertisement.Show("rewardedVideo", options);
		}
	}

	private void HandleShowResult(ShowResult result)
	{
		Debug.Log ("In callback");
		switch (result)
		{
		case ShowResult.Finished:
			Debug.Log ("The ad was successfully shown.");
			//
			// YOUR CODE TO REWARD THE GAMER
			// Give coins etc.

			counterImage.GetComponent<Image> ().overrideSprite = counterSprites [0];
			afterAd = true;
			NewGame ();
			//ResetValues ();
			Debug.Log ("Finished watching");
			break;
		case ShowResult.Skipped:
			Debug.Log ("The ad was skipped before reaching the end.");
			afterAd = false;
			NewGame ();
			break;
		case ShowResult.Failed:
			Debug.LogError ("The ad failed to be shown.");
			afterAd = false;
			NewGame ();
			break;
		}


		Debug.Log ("End of callback");
	}
	#endregion Ads
}
