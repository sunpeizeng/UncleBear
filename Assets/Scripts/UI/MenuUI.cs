using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using Lean.Touch;
using DoozyUI;
using UncleBear;
using UnityEngine.EventSystems;

public class MenuUI : MonoBehaviour
{
    public float mTipCountdown = 3f;
    public float mTipDurtation = 3f;

    public float mPageTurningDuration = 0.5f;
    public Vector3 mEulerAngleLeft = new Vector3(0, -20.5f, 0);
    public Vector3 mEulerAngleRight = new Vector3(0, -179.5f, 0);

    public float mOpenDuration = 0.7f;
    public Vector3 mPosToCamera = new Vector3(0, 0, 140f);
    public Vector3 mRotationToCamera = new Vector3(0, 0, 0);
    public GameObject mPage;
    public GameObject mScreenCover;
    //public Camera mMenuCam;
    public Transform mIndicatorTrans;
    public Transform mTipTrans;
    public Transform mMenuObjTrans;//a father node includes all visible gameobjects
    Animation _animMenu;

    private Vector3 _mPosToParent;
    private Vector3 _mRotationToParent;

    private bool _mOpened = false;
    private bool _mAnimating = false;
    private bool _mLockSwipe = false;
    private Transform _mParent;
    private Transform _mLeftPageTrans;
    private Transform _mRightPageTrans;
    private Transform _mFreePageTrans;

    private int _mTotalPageCount = 0;
    private int _mPageIndex = 0;
    private Image[] _mIndicators;
    private float _mTipCountdownTimer = 0f;
    private bool _mTipShowing = false;
    private bool _mDirty = false;
    private List<FoodItem> _mFoodItemList = new List<FoodItem>();

    private static MenuUI instance = null;

	public int buyType = 0;

    public static MenuUI Instance
    {
        get
        {
            return instance;
        }
    }

    public bool isOpened
    {
        get
        {
            return _mOpened;
        }
    }

    public bool isAnimating
    {
        get
        {
            return _mAnimating;
        }
    }

    void Awake()
    {
        instance = this;

        _mParent = transform.parent;
        //GameUtilities.SetCamFOVByScreen(mMenuCam);
        mScreenCover.SetActive(false);
        Button btn = mScreenCover.transform.GetOrAddComponent<Button>();
        btn.onClick.AddListener(CloseMenu);

        _mPosToParent = transform.localPosition;
        _mRotationToParent = transform.localEulerAngles;

        InitPage();
        _mFreePageTrans = mPage.transform;

        _mLeftPageTrans = Instantiate(_mFreePageTrans, mMenuObjTrans);
        _mLeftPageTrans.gameObject.name = mPage.name;
        _mLeftPageTrans.transform.localEulerAngles = mEulerAngleLeft;

        _mRightPageTrans = Instantiate(_mFreePageTrans, mMenuObjTrans);
        _mRightPageTrans.gameObject.name = mPage.name;
        _mRightPageTrans.transform.localEulerAngles = mEulerAngleRight;

        mPage.SetActive(false);
        //set initial rotation make the pages looks like closed
        //_mRightPageTrans.localEulerAngles = _mLeftPageTrans.localEulerAngles = (mEulerAngleLeft + mEulerAngleRight) / 2;

        mIndicatorTrans.gameObject.SetActive(false);

        mTipTrans.gameObject.SetActive(false);

        LeanTouch.OnFingerSwipe += OnSwipe;
        _animMenu = mMenuObjTrans.GetComponent<Animation>();
    }

    // Use this for initialization
    void Start()
    {
        SortFoodItemList();
        for(int i = 0; i < GameData.FoodItemList.Count; ++i)
        {
            if (GameData.FoodItemList[i].ShowOnMenu)
            {
                _mFoodItemList.Add(GameData.FoodItemList[i]);
            }
        }
        _mTotalPageCount = (_mFoodItemList.Count % 2) > 0 ? (_mFoodItemList.Count / 2 + 1) : (_mFoodItemList.Count / 2);
        InitIndicators();
        //fill contents on left
        FillPageDoubleSide(_mLeftPageTrans, new int[] { _mPageIndex * 2 - 1, _mPageIndex * 2 });
        ShowPage(_mLeftPageTrans, false, true);
        ShowPageMesh(_mLeftPageTrans, false);
        //fill contents on right
        FillPageDoubleSide(_mRightPageTrans, new int[] { _mPageIndex * 2 + 1, _mPageIndex * 2 + 2 });
        ShowPage(_mRightPageTrans, true, false);
        ShowPageMesh(_mRightPageTrans, false);
    }

	private bool _mTouchBlock = false;
    void Update()
    {
#if UNITY_EDITOR
        if (!_mOpened && !_mAnimating && Input.GetMouseButtonUp(0))
#else
		if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
		{
			_mTouchBlock = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
		}

		if (!_mTouchBlock && !_mOpened && !_mAnimating && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
#endif
        {
			RaycastHit hitInfo;

#if UNITY_EDITOR
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);//从摄像机发出到点击坐标的射线
			if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out hitInfo))
#else
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);//从摄像机发出到点击坐标的射线
			if (Physics.Raycast(ray, out hitInfo))
#endif
            {
                if (hitInfo.collider.gameObject == gameObject)
                {
                    OpenMenu();
                }
            }
        }

        //show tip
        if (_mOpened && !_mAnimating)
        {
            if (!_mTipShowing && _mTipCountdownTimer < 0)
            {
                ShowTip();
                this.CallWithDelay(HideTip, mTipDurtation);
                ResetTipCountDown();
            }
            else if (!_mTipShowing)
            {
                _mTipCountdownTimer -= Time.deltaTime;
            }
        }

        if (_mDirty)
        {
            if (_mLeftPageTrans != null)
            {
                FillPageDoubleSide(_mLeftPageTrans, new int[] { _mPageIndex * 2 - 1, _mPageIndex * 2 });
                ShowPage(_mLeftPageTrans, false, true);
                ShowPageMesh(_mLeftPageTrans, false);
            }
            if (_mRightPageTrans != null)
            {
                FillPageDoubleSide(_mRightPageTrans, new int[] { _mPageIndex * 2 + 1, _mPageIndex * 2 + 2 });
                ShowPage(_mRightPageTrans, true, false);
                ShowPageMesh(_mRightPageTrans, false);
            }

            _mDirty = false;
        }
    }

    void OnDestroy()
    {
        instance = null;
        LeanTouch.OnFingerSwipe -= OnSwipe;
    }

    void OnFoodButtonClicked(FoodItem foodItem)
    {
		UIManager.PlaySound ("1按键");
		
        if (!_mOpened || _mAnimating)
            return;

        if (foodItem.Locked)
        {
            OnUnlockButtonClicked(foodItem);
            return;
        }

        CloseMenu();
        UIManager.PlaySound("6选择料理");
        //test
        //foodItem.SetCompleted();
        //foodItem.Stars = UnityEngine.Random.Range(0, 4);
        foodItem.SetPlayedOnce();
        (UIPanelManager.Instance.HidePanel("UIGameHUD") as UIPanel).HideSubElements("UIHomeButton");
        UIPanelManager.Instance.ShowPanel("UILoading").DoOnShowCompleted((panel) =>
        {
            switch (foodItem.ID)
            {
                case "item_finished_pizza":
                    LevelManager.Instance.ChangeLevel(LevelEnum.Pizza);
                    break;
                case "item_finished_sushi":
                    LevelManager.Instance.ChangeLevel(LevelEnum.Sushi);
                    break;
                case "item_finished_cupcake":
                    LevelManager.Instance.ChangeLevel(LevelEnum.Cupcake);
                    break;
                case "item_finished_farfalle":
                    LevelManager.Instance.ChangeLevel(LevelEnum.Farfalle);
                    break;
                case "item_finished_hamburger":
                    LevelManager.Instance.ChangeLevel(LevelEnum.Burger);
                    break;
                case "item_finished_salad":
                    LevelManager.Instance.ChangeLevel(LevelEnum.Salad);
                    break;
                case "item_finished_icecream":
                    LevelManager.Instance.ChangeLevel(LevelEnum.IceCream);
                    break;
            }
        });
    }

	private FoodItem _mUnlockFoodSelected;

    void OnUnlockButtonClicked(FoodItem foodItem)
    {
		UIManager.PlaySound ("1按键");

        if (!_mOpened || _mAnimating)
            return;

		_mUnlockFoodSelected = foodItem;

		LockSwipe (true);

//		if (UncleBear.GameUtilities.GetParam ("isParentControl", "open") == "open") 
//		{
//			UIParentVerification verificationPanel = (UIParentVerification)UIPanelManager.Instance.ShowPanel ("UIParentVerification");
//			verificationPanel.SetSuccessCallback (() => {
//				ShowUnlockPanel();
//			});
//		}
//		else
			ShowUnlockPanel ();
    }

	public void ShowUnlockPanel()
	{
		buyType = Consts.BUY_ITEM_TYPE_FOOD;
		HideMenu(() =>
        {
			//礼包加单个食物，总体UI
//            UIPanelManager.Instance.ShowPanel("UIBuy");
			//单独加载食物购买
			UIUnlockFood unlockPanel = UIPanelManager.Instance.ShowPanel("UIUnlockFood") as UIUnlockFood;

			unlockPanel.SetFoodInfo(_mUnlockFoodSelected);
			return;
        });
	}

    void InitPage()
    {
        Transform sideATrans = mPage.transform.FindChild("SideA");

        Transform sideBTrans = Instantiate(sideATrans, sideATrans.parent);
        sideBTrans.gameObject.name = "SideB";

        Vector3 sideBPos = sideATrans.localPosition;
        sideBPos.z = -sideBPos.z;
        sideBTrans.localPosition = sideBPos;

        Vector3 sideBEulerAngle = sideATrans.localEulerAngles;
        sideBEulerAngle.y = sideBEulerAngle.y + 180f;
        sideBTrans.localEulerAngles = sideBEulerAngle;
    }

    void InitIndicators()
    {
        Transform dotTrans = mIndicatorTrans.FindChild("dot");
        _mIndicators = new Image[_mTotalPageCount];
        for (int i = 0; i < _mTotalPageCount; ++i)
        {
            if (i == 0)
                _mIndicators[i] = dotTrans.GetComponent<Image>();
            else
            {
                Transform trans = Instantiate(dotTrans, mIndicatorTrans);
                _mIndicators[i] = trans.GetComponent<Image>();
                _mIndicators[i].sprite = AtlasManager.Instance.GetSprite("UIAtlas/indicator_dot_unselected");
            }
        }
    }

    void UpdateIndicators()
    {
        for (int i = 0; i < _mIndicators.Length; ++i)
        {
            _mIndicators[i].sprite = AtlasManager.Instance.GetSprite("UIAtlas/indicator_dot_unselected");
        }

        _mIndicators[_mPageIndex].sprite = AtlasManager.Instance.GetSprite("UIAtlas/indicator_dot_selected");
    }

    void OnSwipe(LeanFinger finger)
    {
        if (_mLockSwipe || !_mOpened || _mAnimating)
            return;

        HideTip();
        //reset tip count down every time you swipe
        ResetTipCountDown();

        Vector2 delta = finger.SwipeScreenDelta;
        if (delta.x > 100 && delta.x > Mathf.Abs(delta.y))
        {
            //Debug.Log("swipe right");
            if (_mPageIndex > 0)
            {
                UIManager.PlaySound("5菜谱翻页");

                TurnPage(false);
                UpdateIndicators();
            }
        }
        else if (delta.x < -100 && -delta.x > Mathf.Abs(delta.y))
        {
            //Debug.Log("swipe left");
            if (_mPageIndex + 1 < _mTotalPageCount)
            {
                UIManager.PlaySound("5菜谱翻页");

                TurnPage(true);
                UpdateIndicators();
            }
        }
    }

    void TurnPage(bool turnNext)
    {
        _mAnimating = true;
        Transform temp = null;
        if (turnNext)
        {
            ++_mPageIndex;
            temp = _mLeftPageTrans;
            _mFreePageTrans.localPosition = _mRightPageTrans.localPosition;
            _mFreePageTrans.localRotation = _mRightPageTrans.localRotation;
            //fill next page
            FillPageDoubleSide(_mFreePageTrans, new int[] { _mPageIndex * 2 + 1, _mPageIndex * 2 + 2 });


            this.CallWithDelay(() =>
            {
                if (_mPageIndex * 2 + 1 < _mFoodItemList.Count)
                {
                    _mFreePageTrans.gameObject.SetActive(true);
                    ShowPage(_mFreePageTrans, true, false);
                    ShowPageMesh(_mFreePageTrans, false);
                }
            }, mPageTurningDuration * 0.1f);

            this.CallWithDelay(() =>
            {
                _mLeftPageTrans.gameObject.SetActive(false);
                ShowPage(_mRightPageTrans, false, true);
                ShowPageMesh(_mRightPageTrans, false);

            }, mPageTurningDuration * 0.8f);


            _mRightPageTrans.DOLocalRotate(mEulerAngleLeft, mPageTurningDuration)
                .OnStart(() => {
                    ShowPage(_mRightPageTrans, true, true);
                    ShowPageMesh(_mRightPageTrans, true);
                })
                .OnComplete(
                () => {
                    //mRightPageTrans.DOKill(true);
                    //_mLeftPageTrans.gameObject.SetActive(false);

                    _mLeftPageTrans = _mRightPageTrans;
                    _mRightPageTrans = _mFreePageTrans;
                    _mFreePageTrans = temp;

                    _mAnimating = false;
                });
        }
        else
        {
            --_mPageIndex;

            temp = _mRightPageTrans;
            _mFreePageTrans.localPosition = _mLeftPageTrans.localPosition;
            _mFreePageTrans.localRotation = _mLeftPageTrans.localRotation;
            //fill previous page
            FillPageDoubleSide(_mFreePageTrans, new int[] { _mPageIndex * 2 - 1, _mPageIndex * 2 });

            this.CallWithDelay(() =>
            {
                _mFreePageTrans.gameObject.SetActive(true);
                ShowPage(_mFreePageTrans, false, true);
                ShowPageMesh(_mFreePageTrans, false);

            }, mPageTurningDuration * 0.1f);

            this.CallWithDelay(() =>
            {
                _mRightPageTrans.gameObject.SetActive(false);
                ShowPage(_mLeftPageTrans, true, false);
                ShowPageMesh(_mLeftPageTrans, false);

            }, mPageTurningDuration * 0.8f);

            _mLeftPageTrans.DOLocalRotate(mEulerAngleRight, mPageTurningDuration)
                .OnStart(() =>
                {
                    ShowPage(_mLeftPageTrans, true, true);
                    ShowPageMesh(_mLeftPageTrans, true);
                })
                .OnComplete(
                () => {
                    //mLeftPageTrans.DOKill(true);
                    //_mRightPageTrans.gameObject.SetActive(false);

                    _mRightPageTrans = _mLeftPageTrans;
                    _mLeftPageTrans = _mFreePageTrans;
                    _mFreePageTrans = temp;

                    _mAnimating = false;
                });
        }
    }

    void ShowPage(Transform pageTrans, bool showSideA, bool showSideB)
    {
        Transform sideA = pageTrans.Find("SideA");
        sideA.gameObject.SetActive(showSideA);

        Transform sideB = pageTrans.Find("SideB");
        sideB.gameObject.SetActive(showSideB);

    }

    void ShowPageMesh(Transform pageTrans, bool show)
    {
        pageTrans.Find("PageMesh").gameObject.SetActive(show);
    }

    void FillPageDoubleSide(Transform pageTrans, int[] indice)
    {
        Transform sideA = pageTrans.FindChild("SideA");
        FillPageOneSide(sideA, indice[0]);

        Transform sideB = pageTrans.FindChild("SideB");
        FillPageOneSide(sideB, indice[1]);
    }

    void FillPageOneSide(Transform sideTrans, int index)
    {
        if (index < 0 || index > _mFoodItemList.Count - 1)
        {
            sideTrans.gameObject.SetActive(false);
        }
        else
        {
            FoodItem foodItem = _mFoodItemList[index];

            Item baseItem = ItemManager.Instance.GetItem(foodItem.ID);

            Button foodBtn = sideTrans.FindChild("FoodBtn").GetComponent<Button>();
            foodBtn.onClick.RemoveAllListeners();
            foodBtn.onClick.AddListener(() =>
            {
                OnFoodButtonClicked(foodItem);
            });

            sideTrans.gameObject.SetActive(true);
            Image foodIcon = sideTrans.FindChild("FoodBtn").GetComponent<Image>();
            foodIcon.sprite = baseItem.Icon;
            foodIcon.SetNativeSize();

            if (baseItem.DescKey == "Pizza")
            {
                sideTrans.FindChild("Strip").gameObject.SetActive(true);
            }
            else
            {
                sideTrans.FindChild("Strip").gameObject.SetActive(false);
            }

			UILocalize nameLocalize = sideTrans.FindChild("Name").GetComponent<UILocalize>();
			nameLocalize.Key = foodItem.LocalizedKey;

            //update lock status
            Transform lockTrans = sideTrans.FindChild("LockTag");
            lockTrans.gameObject.SetActive(foodItem.Locked);
            Button lockBtn = lockTrans.GetComponentInChildren<Button>();
            lockBtn.onClick.RemoveAllListeners();
            lockBtn.onClick.AddListener(() =>
            {
                OnUnlockButtonClicked(foodItem);
            });

            //update stars
            Transform starsTrans = sideTrans.FindChild("Stars");
            //if (foodItem.PlayedOnce)
            //{
                starsTrans.gameObject.SetActive(true);
                Image[] starImgs = starsTrans.GetComponentsInChildren<Image>();
                int i = 0;
                for (; i < foodItem.Stars; ++i)
                {
                    starImgs[i].sprite = AtlasManager.Instance.GetSprite("UIAtlas/star");
                }

                for (int j = i; j < starImgs.Length; ++j)
                {
                    starImgs[j].sprite = AtlasManager.Instance.GetSprite("UIAtlas/star_grey");
                }
            //}
//            else
//            {
//                starsTrans.gameObject.SetActive(false);
//            }

            //update new tag
            Transform newTagTrans = sideTrans.FindChild("NewTag");
            newTagTrans.gameObject.SetActive(!foodItem.PlayedOnce);
        }
    }

	void OnEnable()
	{
		EventCenter.Instance.RegisterGameEvent("buySummerPack", buySummerPack);
	}

	void OnDisable()
	{
		if (EventCenter.Instance != null)
		{
			EventCenter.Instance.UnregisterGameEvent("buySummerPack", buySummerPack);
		}
	}


	void buySummerPack()
	{
		buyType = Consts.BUY_ITEM_TYPE_PACK;
		mMenuObjTrans.Find ("summerPack/New UIButton").gameObject.GetComponent<Animation>().Stop();

		HideMenu(() =>
			{
			UIUnlockPackage unlockpackPanel = UIPanelManager.Instance.ShowPanel("UIUnlockPackage") as UIUnlockPackage;

			for (int i = 0; i < GameData.FoodItemList.Count; ++i)
			{
				if (GameData.FoodItemList[i].ID == "item_foodpackage")
				{
					unlockpackPanel.SetFoodInfo(GameData.FoodItemList[i]);
					return;
				}
			}
				return;
			});
	}

	bool isShowSummerPack()
	{
		for (int i = 0; i < GameData.FoodItemList.Count; ++i)
		{
			if ( (GameData.FoodItemList[i].ID == "item_finished_pizza" && GameData.FoodItemList[i].Locked)
				|| (GameData.FoodItemList[i].ID == "item_finished_icecream" && GameData.FoodItemList[i].Locked)
				|| (GameData.FoodItemList[i].ID == "item_finished_farfalle" && GameData.FoodItemList[i].Locked))
			{
				return true;
			}
		}
		return false;
	}

    void OpenMenu()
    {
		#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
		AdHelper.HideBanner();
		#endif

		Repaint ();
        ShowMenuAnim(false);
        UIManager.PlaySound("4打开菜谱", transform);

        UIPanelManager.Instance.GetPanel("UIGameHUD").HideSubElements("UIHomeButton");

        _mLockSwipe = false;
        ResetTipCountDown();

        transform.SetParent(Camera.main.transform, true);

        _mAnimating = true;
        //make sure rotation is finished before move
        transform.DOLocalRotate(mRotationToCamera, mOpenDuration - 0.1f)
            .SetEase(Ease.OutQuad);

        transform.DOLocalMove(mPosToCamera, mOpenDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(
            () =>
            {
                EnterDinning.Instance.ActiveMenuCam(true);
                mScreenCover.SetActive(true);
                mIndicatorTrans.gameObject.SetActive(true);

                transform.SetParent(_mParent, true);
                _mOpened = true;
                _mAnimating = false;

				if (isShowSummerPack ()) {
					mMenuObjTrans.Find ("summerPack").gameObject.SetActive(true);
					mMenuObjTrans.Find ("summerPack/New UIButton").gameObject.GetComponent<Animation>().Play();
				}
            });


//         _mLeftPageTrans.gameObject.SetActive(true);
//         _mRightPageTrans.gameObject.SetActive(true);
//         _mLeftPageTrans.DOLocalRotate(mEulerAngleLeft, mOpenDuration);
//         _mRightPageTrans.DOLocalRotate(mEulerAngleRight, mOpenDuration);


    }

    void CloseMenu()
    {
        if (_mAnimating)
            return;
        
		HideTip ();
        UIPanelManager.Instance.GetPanel("UIGameHUD").ShowSubElements("UIHomeButton");

        _mAnimating = true;
        mIndicatorTrans.gameObject.SetActive(false);
        mScreenCover.SetActive(false);
        EnterDinning.Instance.ActiveMenuCam(false);

        transform.DOLocalRotate(_mRotationToParent, mOpenDuration - 0.1f)
                        .SetEase(Ease.OutQuad);
		//隐藏礼包
		mMenuObjTrans.Find ("summerPack/New UIButton").gameObject.GetComponent<Animation>().Stop();
		mMenuObjTrans.Find ("summerPack").gameObject.SetActive(false);


        transform.DOLocalMove(_mPosToParent, mOpenDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(
            () =>
            {
                mScreenCover.SetActive(false);
                _mOpened = false;
                _mAnimating = false;

				#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
				if (!GameData.AdsRemoved && UncleBear.GameUtilities.GetParam("isBannerOpened", "close") == "open")
				{
					UIPanel loadingPanel = UIPanelManager.Instance.GetPanel("UILoading");
					if (loadingPanel != null && loadingPanel.IsActive)
					{}
					else
					{
						AdHelper.ShowBanner();	
					}
				}
				#endif

                ShowMenuAnim(true);
            });

        //         _mLeftPageTrans.DOLocalRotate((mEulerAngleLeft + mEulerAngleRight) / 2, mOpenDuration)
        //             .OnComplete(() => {
        //                 _mLeftPageTrans.gameObject.SetActive(false);
        //             });

        //         _mRightPageTrans.DOLocalRotate((mEulerAngleLeft + mEulerAngleRight) / 2, mOpenDuration)
        //             .OnComplete(() =>
        //             {
        //                 _mRightPageTrans.gameObject.SetActive(false);
        //             });

//         StartCoroutine(CallWithDelay(() =>
//         {
//             _mRightPageTrans.gameObject.SetActive(false);
//         }, mOpenDuration * 0.8f));
// 
//         _mLeftPageTrans.DOLocalRotate(mEulerAngleRight, mOpenDuration)
//             .OnComplete(() =>
//             {
//                 _mLeftPageTrans.gameObject.SetActive(false);
//             });
    }

    void SortFoodItemList()
    {
        List<FoodItem> foodItemList = new List<FoodItem>();
        for (int i = 0; i < GameData.FoodItemList.Count; ++i)
        {
            foodItemList.Add(GameData.FoodItemList[i]);
        }
        GameData.FoodItemList.Clear();

        List<FoodItem> lockedItems = new List<FoodItem>();

        //sort by lock status
        for (int i = 0; i < foodItemList.Count; ++i)
        {
            FoodItem foodItem = foodItemList[i];

            if (foodItem.Locked)
            {
                lockedItems.Add(foodItem);
                continue;
            }
            GameData.FoodItemList.Add(foodItem);
        }
        GameData.FoodItemList.AddRange(lockedItems);
    }

    void ShowTip()
    {
        mTipTrans.gameObject.SetActive(true);
        _mTipShowing = true;
    }

    void HideTip()
    {
        mTipTrans.gameObject.SetActive(false);
        _mTipShowing = false;
    }

    void ResetTipCountDown()
    {
        _mTipCountdownTimer = mTipCountdown;
    }

    public void LockSwipe(bool lockSwipe)
    {
        _mLockSwipe = lockSwipe;
    }

    public void HideMenu(UnityAction call)
    {
        if (_mAnimating)
            return;

		_animMenu.Stop ();
        mMenuObjTrans.DOLocalMoveY(3000f, 0.5f)
            .SetRelative(true)
            .SetEase(Ease.InBack)
            .OnStart(() =>
            {
                LockSwipe(true);
                _mAnimating = true;
                mScreenCover.GetComponent<Button>().enabled = false;
            })
            .OnComplete(() =>
            {
                LockSwipe(false);
                _mAnimating = false;
                EnterDinning.Instance.ActiveMenuCam(false);
                if (call != null)
                    call.Invoke();
            });
    }

    public void ShowMenu()
    {
        if (_mAnimating)
            return;

        EnterDinning.Instance.ActiveMenuCam(true);
        Repaint();
        mMenuObjTrans.DOLocalMoveY(-3000f, 0.5f)
            .SetRelative(true)
            .SetEase(Ease.OutBack)
            .OnStart(() =>
            {
                LockSwipe(true);
                _mAnimating = true;
            })
            .OnComplete(() =>
            {
                LockSwipe(false);
                _mAnimating = false;
                mScreenCover.GetComponent<Button>().enabled = true;
				if (isShowSummerPack ()) {
					mMenuObjTrans.Find ("summerPack").gameObject.SetActive(true);
					
					mMenuObjTrans.Find ("summerPack/New UIButton").gameObject.GetComponent<Animation>().Play();
				}
				else{
					mMenuObjTrans.Find ("summerPack").gameObject.SetActive(false);
				}
            });
    }

    public void Repaint()
    {
        _mDirty = true;
    }

    public void ShowMenuAnim(bool state)
    {
        if (state)
        {
            _animMenu.Play();
			_animMenu ["anim_menu"].speed = 1;
        }
        else
        {
			_animMenu.SampleAnim ("anim_menu", 0);
        }
    }
}
