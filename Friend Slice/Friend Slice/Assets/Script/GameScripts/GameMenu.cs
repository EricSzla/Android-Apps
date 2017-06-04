/**
 * Copyright (c) 2014-present, Facebook, Inc. All rights reserved.
 *
 * You are hereby granted a non-exclusive, worldwide, royalty-free license to use,
 * copy, modify, and distribute this software in source code or binary form for use
 * in connection with the web services and APIs provided by Facebook.
 *
 * As with any software that integrates with the Facebook platform, your use of
 * this software is subject to the Facebook Developer Principles and Policies
 * [http://developers.facebook.com/policy/]. This copyright notice shall be
 * included in all copies or substantial portions of the software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;


using System;

public class GameMenu : MonoBehaviour
{
    // UI Element References (Set in Unity Editor)
    //  Header
    public Text WelcomeText;
    //  Leaderboard
    public GameObject LeaderboardPanel;
	public GameObject LeaderboardContent;
    public GameObject LeaderboardItemPrefab;
    public ScrollRect LeaderboardScrollRect;
    //  Payment Dialog
    public GameObject PaymentPanel;
    // Game Resources (Set in Unity Editor)
    public GameResources gResources;

	public GameObject normalPlayButton;
	public Button facebookLoginButton;
	public bool isFbLogged;
	public GameObject DialogLoggedIn;
	public GameObject DialogLoggedOut;
	public GameObject DialogUsername;
	public RawImage DialogProfilePic;

	//Selfies
	public GameObject selfiesPanel; 
	public GameObject storeButton;
	public GameObject playButton;
	private float playedTime;


    #region Built-In
    void Awake()
    {
		if (selfiesPanel != null) {
			selfiesPanel.SetActive (false);
		}

		playedTime = Time.time;
        // Initialize FB SDK
		Screen.orientation = ScreenOrientation.LandscapeRight;
		if (!FB.IsInitialized) {
			FB.Init (InitCallback);
		} else {
			int checkSelfiesLater = PlayerPrefs.GetInt ("checkSelfiesLater");
			if (checkSelfiesLater == 1) {
				if (selfiesPanel != null) {
					selfiesPanel.SetActive (false);
				}
			} else {
				StartCoroutine (CheckSelfies ());
			}

			RedrawUI ();
		}
		//PlayerPrefs.DeleteAll ();
    }

	void Update(){
		if (selfiesPanel != null) {
			if (selfiesPanel.activeSelf) {
				if (storeButton != null) {
					storeButton.GetComponent<Animation> ().enabled = false;
				}
				if (playButton != null) {
					playButton.GetComponent<Animation> ().enabled = false;
				}
			} else {
				if (storeButton != null) {
					storeButton.GetComponent<Animation> ().enabled = true;
				}

				if (playButton != null) {
					playButton.GetComponent<Animation> ().enabled = true;
					if (Time.time - playedTime > 5f) {
						playButton.GetComponent<Animation> ().Play ();
						playedTime = Time.time;
					}
				}
			}
		}
	}

    // OnApplicationPause(false) is called when app is resumed from the background
    void OnApplicationPause (bool pauseStatus)
    {
        // Do not do anything in the Unity Editor
        if (Application.isEditor) {
            return;
        }

        // Check the pauseStatus for an app resume
        if (!pauseStatus)
        {
            if (FB.IsInitialized)
            {
                // App Launch events should be logged on app launch & app resume
                // See more: https://developers.facebook.com/docs/app-events/unity#quickstart
                FBAppEvents.LaunchEvent();
            }
            else
            {
                FB.Init(InitCallback);
            }
        }
    }

    // OnLevelWasLoaded is called when we return to the main menu
    void OnLevelWasLoaded(int level)
    {
        Debug.Log("OnLevelWasLoaded");
        if (level == 0 && FB.IsInitialized)
        {
            Debug.Log("Returned to main menu");

            // We've returned to the main menu so let's complete any pending score activity
            if (FB.IsLoggedIn)
            {
                RedrawUI();

                // Post any pending High Score
                if (GameStateManager.highScorePending)
                {
                    GameStateManager.highScorePending = false;
                    FBShare.PostScore(GameStateManager.HighScore);
                }
            }
        }
    }
    #endregion

    #region FB Init
    private void InitCallback()
    {
        Debug.Log("InitCallback");

        // App Launch events should be logged on app launch & app resume
        // See more: https://developers.facebook.com/docs/app-events/unity#quickstart
        FBAppEvents.LaunchEvent();

        if (FB.IsLoggedIn) 
        {
            Debug.Log("Already logged in");
            OnLoginComplete();
        }
		RedrawUI ();
    }
    #endregion

    #region Login
    public void OnLoginClick ()
    {
		SoundManager.Instance.PlaySound (0);
        Debug.Log("OnLoginClick");

        // Disable the Login Button
		facebookLoginButton.interactable = false;

        // Call Facebook Login for Read permissions of 'public_profile', 'user_friends', and 'email'
        FBLogin.PromptForLogin(OnLoginComplete);
    }

	private void OnLoginComplete()
    {
        Debug.Log("OnLoginComplete");

        if (!FB.IsLoggedIn)
        {
            // Reenable the Login Button
			facebookLoginButton.interactable = true;
            return;
        }
        // Begin querying the Graph API for Facebook data
        FBGraph.GetPlayerInfo();
        FBGraph.GetFriends();
        //FBGraph.GetInvitableFriends();
        FBGraph.GetScores();
		PlayerPrefs.SetString ("fbId", AccessToken.CurrentAccessToken.UserId);
		Debug.Log ("Checking Selfies");
		StartCoroutine(CheckSelfies());
    }
    #endregion

    #region GUI
    // Method to update the Game Menu User Interface
    public void RedrawUI ()
    {
		if (FB.IsLoggedIn) {
			// Swap GUI Header for a player after login
			DialogLoggedIn.SetActive (true);
			DialogLoggedOut.SetActive (false);
		} else {
			DialogLoggedIn.SetActive (false);
			DialogLoggedOut.SetActive (true);
		}

		if (GameStateManager.UserTexture != null && !string.IsNullOrEmpty(GameStateManager.Username))
		{
			// Update Profile Picture
			DialogProfilePic.enabled = true;
			DialogProfilePic.texture = GameStateManager.UserTexture;

			// Update Welcome Text
			WelcomeText.text = "Welcome " + GameStateManager.Username + "!";
		}

        // Update PaymentPanel UI
        PaymentPanel.GetComponent<PaymentDialog>().UpdateUI();

		// Update PaymentPanel UI
		LeaderboardPanel.GetComponent<LeaderBoardDialog>().UpdateUI();

		Update ();
    }
    #endregion

    #region Menu Buttons
    public void OnPlayClicked()
    {
        Debug.Log("OnPlayClicked");
        // Start the main game
		SoundManager.Instance.PlaySound (0);
		SceneManager.LoadScene ("Game");
        //GameStateManager.Instance.StartGame();

    }

    public void OnBragClicked()
    {
		SoundManager.Instance.PlaySound (0);
        Debug.Log("OnBragClicked");
        FBShare.ShareBrag();
    }

    public void OnChallengeClicked()
    {
		SoundManager.Instance.PlaySound (0);
        Debug.Log("OnChallengeClicked");
        FBRequest.RequestChallenge();
    }

    public void OnStoreClicked()
    {
		SoundManager.Instance.PlaySound (0);
        Debug.Log("OnStoreClicked");
        PaymentPanel.SetActive(true);
    }

	public void OnSelfieClicked(){
		SceneManager.LoadScene("Selfies");
	}

	public void showLeaderboard(){
		SoundManager.Instance.PlaySound (0);
		FBGraph.GetScores ();
		LeaderboardPanel.SetActive (true);
	}
    #endregion
	

	private IEnumerator CheckSelfies(){
		Debug.Log ("Inside CheckSelfies()");
		var form = new WWWForm(); //here you create a new form connection
			var URL = "http://erpamdevelopment.com/Apps/FriendSlice/AppScripts/GetSelfie.php";
		form.AddField( "fbId", AccessToken.CurrentAccessToken.UserId ); //add your hash code to the field myform_hash, check that this variable name is the same as in PHP file
		form.AddField( "check" , "1");
		form.AddField ("selfieString", "happy");
		var w = new WWW(URL, form); //here we create a var called 'w' and we sync with our URL and the form
		yield return w; //we wait for the form to check the PHP file, so our game dont just hang
		if (w.error != null) {
			print(w.error); //if there is an error, tell us
		} else {
			print("Test ok");
			string userFound = w.text; //here we return the data our PHP told us
			w.Dispose(); //clear our form in game
			Debug.Log(userFound);

			if(userFound.StartsWith("1")){
				Debug.Log("Found Selfies");
				if (storeButton != null) {
					storeButton.GetComponent<Animation> ().enabled = true;
				}

				if (selfiesPanel != null) {
					selfiesPanel.SetActive (false);
				}
			}else{
				Debug.Log("Selfies not found, showing selfie notification");
				if (selfiesPanel != null) {
					selfiesPanel.SetActive (true);
					if (storeButton != null) {
						storeButton.GetComponent<Animation> ().enabled = false;
					}
				}
			}
		}
		Debug.Log ("End of CheckSelfies()");
	}

	public void GotoSelfiesScene(){
		if (storeButton != null) {
			storeButton.GetComponent<Animation> ().enabled = true;
		}
		SceneManager.LoadScene ("Selfies");
	}

	public void LaterButton(){
		if (storeButton != null) {
			storeButton.GetComponent<Animation> ().enabled = true;
		}

		if (playButton != null) {
			playButton.GetComponent<Animation> ().enabled = true;
			playButton.GetComponent<Animation> ().Play();
		}

		PlayerPrefs.SetInt ("checkSelfiesLater",1);
		if (selfiesPanel != null) {
			selfiesPanel.SetActive (false);
		}
	}
}

/*
 * // Start the main game
SceneManager.LoadScene ("Game");
GameStateManager.Instance.StartGame();*/