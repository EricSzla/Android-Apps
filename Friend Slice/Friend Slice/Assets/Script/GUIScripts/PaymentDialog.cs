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
using System.Collections.Generic;
using UnityEngine.Advertisements;

public class PaymentDialog : MonoBehaviour
{
    //   UI References (Set in Unity Editor)   //
	private const int firePowerUpPrice = 50;
	private const int freezePowerUpPrice = 75;
	private const int shieldPowerUpPrice = 100;
	private const int lifePowerUpPrice = 30;

    public Text CoinText;
	public Text FireText;
	public Text FreezeText;
	public Text ShieldText;
	public Text	LifeText;
	private int coin;
	public GameObject PaymentPanel;
	public static bool canButtonBePressed;

    //   Built-in   //
    void OnEnable ()
    {
        // When PaymentDialog is enabled, update the User Interface
        UpdateUI();
		canButtonBePressed = true;
    }

    //   UI   //
    public void UpdateUI ()
    {
        //Set Coin and Bomb counters
		coin = PlayerPrefs.GetInt("CoinAmmount");
		int firepowerup = PlayerPrefs.GetInt ("firePowerUpLeft");
		int freezepowerup = PlayerPrefs.GetInt ("freezePowerUpLeft");
		int shieldpowerup = PlayerPrefs.GetInt ("shieldPowerUpLeft");
		int lifepowerup = PlayerPrefs.GetInt ("lifePowerUpLeft");

		CoinText.text = coin.ToString ();
		FireText.text = firepowerup.ToString ();
		FreezeText.text = freezepowerup.ToString ();
		ShieldText.text = shieldpowerup.ToString ();
		LifeText.text = lifepowerup.ToString ();
    }

    //   Buttons   //
    public void CloseDialog ()
	{
		gameObject.SetActive (false);
	}

	#region PowerUps
	public void GetFirePowerUp(){
		if (canButtonBePressed) {
			coin = PlayerPrefs.GetInt ("CoinAmmount");
			int firepowerup = PlayerPrefs.GetInt ("firePowerUpLeft");
			if (coin >= firePowerUpPrice) {
				firepowerup++;
				coin -= firePowerUpPrice;
				CoinText.text = coin.ToString ();
				FireText.text = firepowerup.ToString ();
				PlayerPrefs.SetInt ("firePowerUpLeft", firepowerup);
				PlayerPrefs.SetInt ("CoinAmmount", coin);
			} else {
				PopupScript.SetPopup ("Not enough coins", 1f);
				canButtonBePressed = false;
			}
		}
	}

	public void GetFreezePowerUp(){
		if (canButtonBePressed) {
			coin = PlayerPrefs.GetInt ("CoinAmmount");
			int freezepowerup = PlayerPrefs.GetInt ("freezePowerUpLeft");
			if (coin >= freezePowerUpPrice) {
				freezepowerup++;
				coin -= freezePowerUpPrice;
				CoinText.text = coin.ToString ();
				FreezeText.text = freezepowerup.ToString ();
				PlayerPrefs.SetInt ("freezePowerUpLeft", freezepowerup);
				PlayerPrefs.SetInt ("CoinAmmount", coin);
			} else {
				PopupScript.SetPopup ("Not enough coin", 1f);
				canButtonBePressed = false;
			}
		}
	}

	public void GetShieldPowerUp(){
		if (canButtonBePressed) {
			coin = PlayerPrefs.GetInt ("CoinAmmount");
			int shieldpowerup = PlayerPrefs.GetInt ("shieldPowerUpLeft");
			if (coin >= shieldPowerUpPrice) {
				shieldpowerup++;
				coin -= shieldPowerUpPrice;
				CoinText.text = coin.ToString ();
				ShieldText.text = shieldpowerup.ToString ();
				PlayerPrefs.SetInt ("shieldPowerUpLeft", shieldpowerup);
				PlayerPrefs.SetInt ("CoinAmmount", coin);
			} else {
				PopupScript.SetPopup ("Not enough coins", 1f);
				canButtonBePressed = false;
			}
		}

	}

	public void GetLifePowerUp(){
		if (canButtonBePressed) {
			coin = PlayerPrefs.GetInt ("CoinAmmount");
			int lifepowerup = PlayerPrefs.GetInt ("lifePowerUpLeft");
			if (coin >= lifePowerUpPrice) {
				lifepowerup++;
				coin -= lifePowerUpPrice;
				CoinText.text = coin.ToString ();
				LifeText.text = lifepowerup.ToString ();
				PlayerPrefs.SetInt ("lifePowerUpLeft", lifepowerup);
				PlayerPrefs.SetInt ("CoinAmmount", coin);
			} else {
				PopupScript.SetPopup ("Not enough coins", 1f);
				canButtonBePressed = false;
			}
		}
	}
	#endregion 

	public static void setButtonToTrue(){
		canButtonBePressed = true;
	}


	#region Ads
	public void LaunchAd(){
		//TODO: Launch the ad ! 
		SoundManager.Instance.PlaySound (0);
		Debug.Log ("Showing Ad");
		if (Advertisement.IsReady("rewarderVideoCoin"))
		{
			Debug.Log ("Ad Ready");
			var options = new ShowOptions { resultCallback = HandleShowResult };
			Advertisement.Show("rewarderVideoCoin", options);
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
			int coin = PlayerPrefs.GetInt ("CoinAmmount");
			coin++;
			PlayerPrefs.SetInt ("CoinAmmount", coin);
			CoinText.text = coin.ToString ();
			Debug.Log ("Finished watching");
			break;
		case ShowResult.Skipped:
			Debug.Log ("The ad was skipped before reaching the end.");
			break;
		case ShowResult.Failed:
			Debug.LogError ("The ad failed to be shown.");
			break;
		}


		Debug.Log ("End of callback");
	}
	#endregion Ads
		
}
