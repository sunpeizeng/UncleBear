using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Events;

public interface IIAPBehaviourBase
{
	
}

public interface IStandardIAPBehaviour : IIAPBehaviourBase
{
	Dictionary<string, string> StoreInfo { get; }
	IAPType Type { get; }
	string CurrencyCode { get; }
	decimal LocalizedPrice { get; }
	void SetCurrencyCode (string currencyCode);
	void SetLocalizedPrice (decimal localizedPrice);
	void Create (Dictionary<string, string> storeInfo, IAPType type);
}

public interface IOtherIAPBehaviour : IIAPBehaviourBase
{
	string Title { get; }
	string Description { get; }
	int RMBPrice { get; }
	void Create (string title, string description, int RMBPrice);
}

public class StoreName
{
	public const string GooglePlay = "GooglePlay";
	public const string AppStore = "AppleAppStore";
}

public enum IAPType
{
	Consumable,
	NonConsumable,
	Subscription
}

public class IAPItem
{
	class StandardIAPItem : IStandardIAPBehaviour
	{
		private Dictionary<string, string> _mStoreInfo;
		private ProductType _mType = ProductType.Consumable;
		private string _mCurrencyCode;
		private decimal _mLocalizedPrice;

		#region IStandardIAPBehaviour implementation
		public Dictionary<string, string> StoreInfo 
		{
			get
			{
				return _mStoreInfo;
			}
		}

		public IAPType Type
		{
			get
			{
				switch (_mType) 
				{
				case ProductType.Consumable:
					return IAPType.Consumable;
				case ProductType.NonConsumable:
					return IAPType.NonConsumable;
				case ProductType.Subscription:
					return IAPType.Subscription;
				default:
					return IAPType.Consumable;
				}
			}
		}

		public string CurrencyCode
		{
			get
			{
				return _mCurrencyCode;
			}
		}

		public decimal LocalizedPrice
		{
			get
			{
				return _mLocalizedPrice;
			}
		}

		public void SetLocalizedPrice(decimal localizedPrice)
		{
			_mLocalizedPrice = localizedPrice;
		}

		public void SetCurrencyCode(string currencyCode)
		{
			_mCurrencyCode = currencyCode;
		}

		public void Create(Dictionary<string, string> storeInfo, IAPType type)
		{
			_mStoreInfo = storeInfo;
			switch (type) 
			{
			case IAPType.Consumable:
				_mType = ProductType.Consumable;
				break;
			case IAPType.NonConsumable:
				_mType = ProductType.NonConsumable;
				break;
			case IAPType.Subscription:
				_mType = ProductType.Subscription;
				break;
			}
		}
		#endregion
	}

	class OtherIAPItem : IOtherIAPBehaviour
	{
		private string _mTitle;//计费点title，用于例如光辉支付等第三方sdk传递参数
		private string _mDescription;//计费点描述，用于例如光辉支付等第三方sdk传递参数
		private int _mRMBPrice;//以分为单位

		#region IOtherIAPBehaviour implementation
		public void Create (string title, string description, int RMBPrice)
		{
			_mTitle = title;
			_mDescription = description;
			_mRMBPrice = RMBPrice;
		}

		public string Title 
		{
			get
			{
				return _mTitle;
			}
		}

		public string Description 
		{
			get 
			{
				return _mDescription;
			}
		}

		public int RMBPrice {
			get 
			{
				return _mRMBPrice;
			}
		}
		#endregion
	}

	private string _mInternalId;
	private StandardIAPItem _mStandardIAPItem = new StandardIAPItem();
	private OtherIAPItem _mOtherIAPItem = new OtherIAPItem();

	public string InternalIAPId
	{
		get
		{
			return _mInternalId;
		}
	}

	public IAPItem(string internalId)
	{
		_mInternalId = internalId;	
	}

	public T GetItemExtension <T>() where T : IIAPBehaviourBase
	{
		if (_mStandardIAPItem is T)
			return (T)(IIAPBehaviourBase)_mStandardIAPItem;

		if (_mOtherIAPItem is T)
			return (T)(IIAPBehaviourBase)_mOtherIAPItem;

		return default(T);
	}
		
}

public class IAPHelper : IStoreListener 
{
	const string GooglePlayPubKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAsFozQe2KSdLFGx5IXr94XUxuQH3oYsVMv8OeS5VUC1+Y7HU7LF+5F76qTtkDUkIf3ZEyDf4N9AEwlMpOO9MrbxlU3CEJmvZHYXXeCtDuHNYaS1IZ1MGbZEH4jW9u1mVuIw9o15/Fo16nqI4zk2G0+adwLsbueGp9k67Qjwkjtt7p9Op4fKYFAvCbIxwybdo6FazeB6xr3Aav4kjLngCrft0XTYKVom8aOKezghF+UpHMRUtnA/Hs1kJSW+V2ECq9j+qiAQo6TBQpioxoL4fuY2++la9Dg7TRzXPEYPooegnD2Lfbf3VbCQkhXFpuYeICB2xDvUP+MXG0yzCXL1mCWwIDAQAB";

	private static Dictionary<string, IAPItem> IAPItems = new Dictionary<string, IAPItem>();

	private static IStoreController IAPController;
	private static IAppleExtensions AppleExtensions;

	private static UnityAction<string> purchaseSucceededCb = null;
	private static UnityAction<string, string> purchaseFailedCb = null;
	private static bool purchaseOngoing = false;

	static public UnityAction<string> PurchaseSucceededEvent
	{
		get
		{
			return purchaseSucceededCb;
		}
		set
		{
			purchaseSucceededCb = value;
		}
	}

	static public UnityAction<string, string> PurchaseFailedEvent
	{
		get
		{
			return purchaseFailedCb;
		}
		set
		{
			purchaseFailedCb = value;
		}
	}

	public static bool IsPurchaseOngoing
	{
		get
		{
			return purchaseOngoing;
		}
	}

	private IAPHelper()
	{
		
	}

	static public void InitUnityIAP(List<IAPItem> iapItems)
	{
		LogUtil.Log ("IAPHelper", "Init IAP, channel: {0}", SDKBridge.GetChannel());

		ConfigurationBuilder builder = null;
		if (SDKBridge.GetChannel () == "App Store" || SDKBridge.GetChannel () == "google")
		{
			builder = ConfigurationBuilder.Instance (StandardPurchasingModule.Instance ());

			if (SDKBridge.GetChannel () == "google")
				builder.Configure<IGooglePlayConfiguration> ().SetPublicKey (GooglePlayPubKey);
		}

		for (int i = 0; i < iapItems.Count; ++i) 
		{
			IAPItem item = iapItems [i];
			if (builder != null)
			{
				IDs ids = null;
				IStandardIAPBehaviour standardItem = item.GetItemExtension<IStandardIAPBehaviour> ();
				if (standardItem.StoreInfo != null) 
				{
					ids = new IDs ();
					foreach (KeyValuePair<string, string> pair in standardItem.StoreInfo) 
					{
						ids.Add (pair.Key, pair.Value);
					}
				}

				if (ids != null)
					builder.AddProduct (item.InternalIAPId, (ProductType)(int)standardItem.Type, ids);
			}

			IAPItems.Add (item.InternalIAPId, item);
		}
			
		if (builder != null)
		{
			LogUtil.Log ("IAPHelper", "Initialize through UnityPurchasing");
			UnityPurchasing.Initialize (new IAPHelper (), builder);
		}

		IAPCallbackReceiver.GetOrCreateInstance ();
	}

	static public void Pay(string internalId)
	{
		if (SDKBridge.GetChannel() == "App Store" || SDKBridge.GetChannel() == "google")
		{
			if (IAPController != null) 
			{
				IAPController.InitiatePurchase (IAPController.products.WithID (internalId));
				purchaseOngoing = true;
			}
		} 
		else 
		{
			IAPItem item = GetIAPItem (internalId);
			IOtherIAPBehaviour otherIAP = item.GetItemExtension<IOtherIAPBehaviour> ();
			Dictionary<string, object> dic = new Dictionary<string, object> ();
			dic.Add ("internalId", internalId);
			dic.Add ("title", otherIAP.Title);
			dic.Add ("description", otherIAP.Description);
			dic.Add ("price", otherIAP.RMBPrice);

			SDKBridge.AndroidActivity.Call ("pay", MiniJSONV.Json.Serialize (dic));
			purchaseOngoing = true;
		}
	}

	static public void Restore(UnityAction<bool> callback)
	{
		if (AppleExtensions != null) 
		{
			purchaseOngoing = true;
			AppleExtensions.RestoreTransactions ((success) => {

				purchaseOngoing = false;

				if (callback != null)
					callback(success);
			});
		}
	}

	static public IAPItem GetIAPItem(string internalId)
	{
		IAPItem item = null;
		IAPItems.TryGetValue (internalId, out item);
		return item;
	}

	static public void FinishPurchasing()
	{
		purchaseOngoing = false;
	}
	#region IStoreListener implementation

	public void OnInitialized (IStoreController controller, IExtensionProvider extensions)
	{
		LogUtil.Log ("IAPHelper", "IAP initialized");

		IAPController = controller;

		AppleExtensions = extensions.GetExtension<IAppleExtensions> ();

		foreach (KeyValuePair<string, IAPItem> pair in IAPItems) 
		{
			Product product = controller.products.WithID (pair.Key);
			if (product != null)
			{
				LogUtil.Log ("IAPHelper", "item localized price: {0}, currency code: {1}", product.metadata.localizedPrice, product.metadata.isoCurrencyCode);

				pair.Value.GetItemExtension<IStandardIAPBehaviour> ().SetLocalizedPrice (product.metadata.localizedPrice);
				pair.Value.GetItemExtension<IStandardIAPBehaviour> ().SetCurrencyCode (product.metadata.isoCurrencyCode);
			}
		}
	}

	public void OnInitializeFailed (InitializationFailureReason error)
	{
		LogUtil.LogError ("IAPHelper", "IAP initialization failed: {0}", error.ToString ());
	}

	public PurchaseProcessingResult ProcessPurchase (PurchaseEventArgs e)
	{
		LogUtil.Log ("IAPHelper", "Process purchase: {0}", e.purchasedProduct.definition.id);

		if (purchaseSucceededCb != null)
		{
			purchaseSucceededCb.Invoke (e.purchasedProduct.definition.id);
		}

		FinishPurchasing ();
		return PurchaseProcessingResult.Complete;
	}

	public void OnPurchaseFailed (Product product, PurchaseFailureReason p)
	{
		if (purchaseFailedCb != null)
		{
			purchaseFailedCb.Invoke (product.definition.id, p.ToString());
		}

		FinishPurchasing ();
	}

	#endregion
}
