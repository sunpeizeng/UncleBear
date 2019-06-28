using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Lean.Touch;

public class UIParentVerification : UIPanel {

	public UILocalize mTextTip;
	public Vector2 mTolerable = new Vector2(200f, 200f);

	const int SWIPE_UP = 0;
	const int SWIPE_DOWN = 1;
	const int SWIPE_LEFT = 2;
	const int SWIPE_RIGHT = 3;

	private bool _mPanelReady = false;
	private int _mTipRandom = 0;
	private bool _mVerificationSuccess = false;
	private float _mScaleFactor = 1f;

	private UnityAction _mCallback;

	protected override void Awake ()
	{
		base.Awake ();

		_mScaleFactor = Screen.width / DoozyUI.UIManager.GetUiContainer.GetComponent<CanvasScaler> ().referenceResolution.x;
	}

	void OnEnable()
	{
		LeanTouch.OnFingerSwipe += OnSwipe;
	}

	void OnDisable()
	{
		LeanTouch.OnFingerSwipe -= OnSwipe;
	}

	public void SetSuccessCallback(UnityAction callback)
	{
		_mCallback = callback;
	}

	void OnSwipe(LeanFinger finger)
	{
		if (!_mPanelReady)
			return;

		_mVerificationSuccess = false;
		Vector2 delta = finger.SwipeScreenDelta;
		if (delta.x > 150 * _mScaleFactor && Mathf.Abs (delta.y) < mTolerable.y * _mScaleFactor) {
			//swipe right
			if (_mTipRandom == SWIPE_RIGHT) 
			{
				_mVerificationSuccess = true;
			}
		} else if (delta.x < -150 * _mScaleFactor && Mathf.Abs (delta.y) < mTolerable.y * _mScaleFactor) {
			//swipe left
			if (_mTipRandom == SWIPE_LEFT) 
			{
				_mVerificationSuccess = true;
			}
		} else if (delta.y < -150 * _mScaleFactor && Mathf.Abs (delta.x) < mTolerable.x * _mScaleFactor) {
			//swipe down
			if (_mTipRandom == SWIPE_DOWN) 
			{
				_mVerificationSuccess = true;
			}
		} else if (delta.y > 150 * _mScaleFactor && Mathf.Abs (delta.x) < mTolerable.x * _mScaleFactor) {
			//swipe up
			if (_mTipRandom == SWIPE_UP) 
			{
				_mVerificationSuccess = true;
			}
		}

		if (_mVerificationSuccess)
			DoozyUI.UIManager.PlaySound ("1按键");
		else
			DoozyUI.UIManager.PlaySound ("52错误音效");

		UIPanelManager.Instance.HidePanel (this);
	}

	protected override void OnPanelShowBegin()
	{
		base.OnPanelShowBegin();

		_mTipRandom = Random.Range (0, 3);
		if (_mTipRandom == SWIPE_UP) 
		{
			mTextTip.Key = "Finger_Slide_Up";
		}
		else if (_mTipRandom == SWIPE_DOWN) 
		{
			mTextTip.Key = "Finger_Slide_Down";
		}
		else if (_mTipRandom == SWIPE_LEFT) 
		{
			mTextTip.Key = "Finger_Slide_Left";
		}
		else if (_mTipRandom == SWIPE_RIGHT) 
		{
			mTextTip.Key = "Finger_Slide_Right";
		}
	}

	protected override void OnPanelHideBegin()
	{
		base.OnPanelHideBegin();

		_mPanelReady = false;
	}

	protected override void OnPanelShowCompleted()
	{
		base.OnPanelShowCompleted();

		_mPanelReady = true;
	}

	protected override void OnPanelHideCompleted()
	{
		base.OnPanelHideCompleted();

		if (_mVerificationSuccess) 
		{
			if (_mCallback != null) 
			{
				_mCallback.Invoke ();
			}
		}
		_mCallback = null;
	}
}
