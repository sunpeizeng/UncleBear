using UnityEngine;

public class IAPCallbackReceiver: DoozyUI.Singleton<IAPCallbackReceiver>
{
	public void OnPurchaseSucceeded(string internalId)
	{
		if (IAPHelper.PurchaseSucceededEvent != null) 
		{
			IAPHelper.PurchaseSucceededEvent (internalId);
		}

		IAPHelper.FinishPurchasing ();
	}

	public void OnPurchaseFailed(string internalId)
	{
		if (IAPHelper.PurchaseFailedEvent != null) 
		{
			IAPHelper.PurchaseFailedEvent (internalId, "");
		}
		IAPHelper.FinishPurchasing ();
	}

	public void OnPurchaseCancelled(string internalId)
	{
		IAPHelper.FinishPurchasing ();
	}
}


