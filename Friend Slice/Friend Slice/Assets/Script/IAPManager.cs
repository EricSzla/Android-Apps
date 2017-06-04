using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
public class IAPManager : MonoBehaviour, IStoreListener
{
	public static IAPManager Instance{ set; get; }


	private static IStoreController m_StoreController;          // The Unity Purchasing system.
	private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.


	public static string PRODUCT_100_COINS =    "coin_100";   
	public static string PRODUCT_250_COINS = "coin_250";
	public static string PRODUCT_700_COINS =  "coin_700"; 
	public static string PRODUCT_1500_COINS =  "coin_1500"; 
	public Text CoinText;

	private void Awake(){
		Instance = this;
	}
		
	private void Start()
	{
		// If we haven't set up the Unity Purchasing reference
		if (m_StoreController == null)
		{
			// Begin to configure our connection to Purchasing
			InitializePurchasing();
		}
	}

	public void InitializePurchasing() 
	{
		// If we have already connected to Purchasing ...
		if (IsInitialized())
		{
			// ... we are done here.
			return;
		}

		// Create a builder, first passing in a suite of Unity provided stores.
		var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

		builder.AddProduct(PRODUCT_100_COINS, ProductType.Consumable);
		builder.AddProduct(PRODUCT_250_COINS, ProductType.Consumable);
		builder.AddProduct (PRODUCT_700_COINS, ProductType.Consumable);
		builder.AddProduct (PRODUCT_1500_COINS, ProductType.Consumable);

		UnityPurchasing.Initialize(this, builder);
	}


	private bool IsInitialized()
	{
		// Only say we are initialized if both the Purchasing references are set.
		return m_StoreController != null && m_StoreExtensionProvider != null;
	}


	public void Buy100Coins()
	{
		// Buy the consumable product using its general identifier. Expect a response either 
		// through ProcessPurchase or OnPurchaseFailed asynchronously.
		BuyProductID(PRODUCT_100_COINS);
		int coin = PlayerPrefs.GetInt ("CoinAmmount");
		CoinText.text = coin.ToString();
	}


	public void Buy250Coins()
	{
		// Buy the consumable product using its general identifier. Expect a response either 
		// through ProcessPurchase or OnPurchaseFailed asynchronously.
		BuyProductID(PRODUCT_250_COINS);
		int coin = PlayerPrefs.GetInt ("CoinAmmount");
		CoinText.text = coin.ToString();

	}

	public void Buy700Coins()
	{
		// Buy the consumable product using its general identifier. Expect a response either 
		// through ProcessPurchase or OnPurchaseFailed asynchronously.
		BuyProductID(PRODUCT_700_COINS);
		int coin = PlayerPrefs.GetInt ("CoinAmmount");
		CoinText.text = coin.ToString();
	}

	public void Buy1500Coins()
	{
		// Buy the consumable product using its general identifier. Expect a response either 
		// through ProcessPurchase or OnPurchaseFailed asynchronously.
		BuyProductID(PRODUCT_1500_COINS);
		int coin = PlayerPrefs.GetInt ("CoinAmmount");
		CoinText.text = coin.ToString();
	}
		

	void BuyProductID(string productId)
	{
		// If Purchasing has been initialized ...
		if (IsInitialized())
		{
			// ... look up the Product reference with the general product identifier and the Purchasing 
			// system's products collection.
			Product product = m_StoreController.products.WithID(productId);

			// If the look up found a product for this device's store and that product is ready to be sold ... 
			if (product != null && product.availableToPurchase)
			{
				Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
				// ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
				// asynchronously.
				m_StoreController.InitiatePurchase(product);
			}
			// Otherwise ...
			else
			{
				// ... report the product look-up failure situation  
				Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
			}
		}
		// Otherwise ...
		else
		{
			// ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
			// retrying initiailization.
			Debug.Log("BuyProductID FAIL. Not initialized.");
		}
	}


	// Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
	// Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
	public void RestorePurchases()
	{
		// If Purchasing has not yet been set up ...
		if (!IsInitialized())
		{
			// ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
			Debug.Log("RestorePurchases FAIL. Not initialized.");
			return;
		}

		// If we are running on an Apple device ... 
		if (Application.platform == RuntimePlatform.IPhonePlayer || 
			Application.platform == RuntimePlatform.OSXPlayer)
		{
			// ... begin restoring purchases
			Debug.Log("RestorePurchases started ...");

			// Fetch the Apple store-specific subsystem.
			var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
			// Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
			// the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
			apple.RestoreTransactions((result) => {
				// The first phase of restoration. If no more responses are received on ProcessPurchase then 
				// no purchases are available to be restored.
				Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
			});
		}
		// Otherwise ...
		else
		{
			// We are not running on an Apple device. No work is necessary to restore purchases.
			Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
		}
	}


	//  
	// --- IStoreListener
	//

	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
	{
		// Purchasing has succeeded initializing. Collect our Purchasing references.
		Debug.Log("OnInitialized: PASS");

		// Overall Purchasing system, configured with products for this application.
		m_StoreController = controller;
		// Store specific subsystem, for accessing device-specific store features.
		m_StoreExtensionProvider = extensions;
	}


	public void OnInitializeFailed(InitializationFailureReason error)
	{
		// Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
		Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
	}


	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args) 
	{
		int coin = PlayerPrefs.GetInt ("CoinAmmount");
		// A consumable product has been purchased by this user.
		if (String.Equals(args.purchasedProduct.definition.id, PRODUCT_100_COINS, StringComparison.Ordinal))
		{
			Debug.Log("You have just bought 100 gold! Good times");
			coin += 100;
			PlayerPrefs.SetInt ("CoinAmmount", coin);
			// The consumable item has been successfully purchased, add 100 coins to the player's in-game score.
		}
		// Or ... a non-consumable product has been purchased by this user.
		else if (String.Equals(args.purchasedProduct.definition.id, PRODUCT_250_COINS, StringComparison.Ordinal))
		{
			Debug.Log("You have just bought 250 gold! Good times");
			coin += 250;
			PlayerPrefs.SetInt ("CoinAmmount", coin);
			// TODO: The non-consumable item has been successfully purchased, grant this item to the player.
		}
		// Or ... a subscription product has been purchased by this user.
		else if (String.Equals(args.purchasedProduct.definition.id, PRODUCT_700_COINS, StringComparison.Ordinal))
		{
			Debug.Log("You have just bought 700 gold! Good times");
			coin += 700;
			PlayerPrefs.SetInt ("CoinAmmount", coin);
			// TODO: The subscription item has been successfully purchased, grant this to the player.
		}// Or ... a subscription product has been purchased by this user.
		else if (String.Equals(args.purchasedProduct.definition.id, PRODUCT_1500_COINS, StringComparison.Ordinal))
		{
			Debug.Log("You have just bought 1500 gold! Good times");
			coin += 1500;
			PlayerPrefs.SetInt ("CoinAmmount", coin);
			// TODO: The subscription item has been successfully purchased, grant this to the player.
		}
		// Or ... an unknown product has been purchased by this user. Fill in additional products here....
		else 
		{
			Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
		}
			
		return PurchaseProcessingResult.Complete;
	}


	public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
	{
		// A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
		// this reason with the user to guide their troubleshooting actions.
		Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
	}
}