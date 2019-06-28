using UnityEngine;
using UnityEngine.Events;
using DoozyUI;
using System.Collections.Generic;

//handler called when a panel begins to show, begins to hide, completed showing and completed hiding
public delegate void PanelHandler(UIPanel panel);

public interface IOnPanelShow
{
    IOnPanelShow DoOnShowBegin(PanelHandler handler);
    IOnPanelShow DoOnShowCompleted(PanelHandler handler);
}

public interface IOnPanelHide
{
    IOnPanelHide DoOnHideBegin(PanelHandler handler);
    IOnPanelHide DoOnHideCompleted(PanelHandler handler);
}

[RequireComponent(typeof(UIElement))]
public class UIPanel : MonoBehaviour, IOnPanelShow, IOnPanelHide
{
    [HideInInspector]
    public int mLayer = 0;

    private bool _mIsShowing = false;
    private bool _mIsHiding = false;
    private bool _mIsActive = false;
    private bool _mUseOriginalSiblingIndex = true;

    private PanelHandler _mOnShowBegin;
    private PanelHandler _mOnHideBegin;
    private PanelHandler _mOnShowCompleted;
    private PanelHandler _mOnHideCompleted;

    private UnityAction _mOnShowBeginListener;
    private UnityAction _mOnHideBeginListener;
    private UnityAction _mOnShowCompletedListener;
    private UnityAction _mOnHideCompletedListener;

    private string _mPanelName;
    private RectTransform _mRectTrans;
    private Canvas _mCanvas;
    private Canvas[] _mChildCanvases;
    private UIEffect[] _mEffects;

    private Dictionary<string, Coroutine> _mShowSubElementCallCoroutines = new Dictionary<string, Coroutine>();
    private Dictionary<string, Coroutine> _mHideSubElementCallCoroutines = new Dictionary<string, Coroutine>();

    private HashSet<string> _mSubElementsOnMoving = new HashSet<string>();

    private bool _mDirty = false;

#if UNITY_EDITOR
    [SerializeField]
    [HideInInspector]
    private int _mTestSortingOrder;
#endif

    public string PanelName
    {
        get
        {
            return _mPanelName;
        }
    }
    public bool IsShowing
    {
        get
        {
            return _mIsShowing;
        }
    }
    public bool IsHiding
    {
        get
        {
            return _mIsHiding;
        }
    }
    public bool IsActive
    {
        get
        {
            return _mIsActive;
        }
    }
    /// <summary>
    /// DO NOT use this property unless you know exactly what it means
    /// </summary>
    public bool UseOriginalSiblingIndex
    {
        get
        {
            return _mUseOriginalSiblingIndex;
        }
        set
        {
            _mUseOriginalSiblingIndex = value;
        }
    }

    protected virtual void Awake()
    {
        UIPanelManager.Instance.RegisterPanel(this);

        _mPanelName = GetComponent<UIElement>().elementName;
        _mRectTrans = GetComponent<RectTransform>();
        _mCanvas = GetComponent<Canvas>();
        _mCanvas.overrideSorting = true;
        _mCanvas.sortingLayerName = UIManager.GetUiContainer.GetComponent<Canvas>().sortingLayerName;
        _mChildCanvases = GetComponentsInChildren<Canvas>();
        _mEffects = GetComponentsInChildren<UIEffect>();
    }

    protected virtual void LateUpdate()
    {
        if (_mDirty)
        {
            OnPanelRepaint();
            _mDirty = false;
        }
    }

    void OnDestroy()
    {
        if (UIPanelManager.Instance != null)
            UIPanelManager.Instance.UnregisterPanel(this);
    }

    //derived class could override
    protected virtual void OnPanelShowBegin()
    {
        LogUtil.Log("UIPanel", "UIPanel< {0} > OnPanelShowBegin", _mPanelName);
    }
    protected virtual void OnPanelShowCompleted()
    {
        LogUtil.Log("UIPanel", "UIPanel< {0} > OnPanelShowCompleted", _mPanelName);
    }
    protected virtual void OnPanelHideBegin()
    {
        LogUtil.Log("UIPanel", "UIPanel< {0} > OnPanelHideBegin", _mPanelName);
    }
    protected virtual void OnPanelHideCompleted()
    {
        LogUtil.Log("UIPanel", "UIPanel< {0} > OnPanelHideCompleted", _mPanelName);
    }

    protected virtual void OnPanelRepaint()
    {

    }

    public void Repaint()
    {
        _mDirty = true;
    }

    public void OnShowBegin()
    {
        if (_mIsActive)
        {
            //LogUtil.Log("UIPanel", "Panel {0} is already active", _mPanelName);
            return;
        }
        //LogUtil.Log("UIPanel", "UIPanel< {0} > OnShowBegin", _mPanelName);
        _mIsShowing = true;

        if (!_mUseOriginalSiblingIndex)
            transform.SetAsLastSibling();

        _mIsActive = true;
        SimpleObservableList<UIPanel> activePanels = UIPanelManager.Instance.ActivePanelList;
        activePanels.Add(this);


        if (_mOnShowBeginListener != null)
        {
            _mOnShowBeginListener.Invoke();
        }

        if (_mOnShowBegin != null)
        {
            _mOnShowBegin.Invoke(this);
            //make sure run only once
            _mOnShowBegin = null;
        }

        OnPanelShowBegin();
    }

    public void OnHideBegin()
    {
        if (!_mIsActive)
        {
            //LogUtil.Log("UIPanel", "Panel< {0} > has already been hidden", _mPanelName);
            return;
        }
        //LogUtil.Log("UIPanel", "UIPanel< {0} > OnHideBegin", _mPanelName);
        _mIsHiding = true;

        _mIsActive = false;
        SimpleObservableList<UIPanel> activePanels = UIPanelManager.Instance.ActivePanelList;
        activePanels.Remove(this);

        if (_mOnHideBeginListener != null)
        {
            _mOnHideBeginListener.Invoke();
        }

        if (_mOnHideBegin != null)
        {
            _mOnHideBegin.Invoke(this);
            //make sure run only once
            _mOnHideBegin = null;
        }

        OnPanelHideBegin();

    }

    public void OnShowCompleted()
    {
        if (!_mIsShowing)
            return;

        //LogUtil.Log("UIPanel", "UIPanel< {0} >OnShowCompleted", _mPanelName);

        _mIsShowing = false;

        if (_mOnShowCompletedListener != null)
        {
            _mOnShowCompletedListener.Invoke();
        }

        if (_mOnShowCompleted != null)
        {
            _mOnShowCompleted.Invoke(this);
            //make sure run only once
            _mOnShowCompleted = null;
        }

        OnPanelShowCompleted();

    }

    public void OnHideCompleted()
    {
        if (!_mIsHiding)
            return;

        //LogUtil.Log("UIPanel", "UIPanel< {0} > OnHideCompleted", _mPanelName);

        _mIsHiding = false;

        if (_mOnHideCompletedListener != null)
        {
            _mOnHideCompletedListener();
        }

        if (_mOnHideCompleted != null)
        {
            _mOnHideCompleted.Invoke(this);
            //make sure run only once
            _mOnHideCompleted = null;
        }

        OnPanelHideCompleted();

    }

    public IOnPanelShow DoOnShowBegin(PanelHandler handler)
    {
        _mOnShowBegin += handler;
        return this;
    }

    public IOnPanelShow DoOnShowCompleted(PanelHandler handler)
    {
        _mOnShowCompleted += handler;
        return this;
    }

    public void ShowSubElements(string elementName, UnityAction callOnComplete = null)
    {

        if (elementName == _mPanelName)
        {
            _mSubElementsOnMoving.Add(elementName);

            UIPanelManager.Instance.ShowPanel(_mPanelName)
                .DoOnShowCompleted((panel) =>
                {
                    if (callOnComplete != null)
                        callOnComplete.Invoke();

                    _mSubElementsOnMoving.Remove(elementName);
                });

            return;
        }

        StopOngoingCoroutines(elementName);

        UIElement[] elements = GetComponentsInChildren<UIElement>();
        float finishTime = 0;
        for (int i = 0; i < elements.Length; ++i)
        {
            if (elements[i] == this || elements[i].elementName != elementName || elements[i].isVisible)
                continue;

            if (!_mSubElementsOnMoving.Contains(elementName))
                _mSubElementsOnMoving.Add(elementName);

            finishTime = Mathf.Max(finishTime, elements[i].GetInAnimationsFinishTime());
        }

        if (_mSubElementsOnMoving.Contains(elementName))
        {
            //use UIPanelManager.CallWithDelay to make sure coroutine will be triggered anyway
            _mShowSubElementCallCoroutines[elementName] = UIPanelManager.Instance.CallWithDelay(() =>
            {
                if (callOnComplete != null)
                    callOnComplete.Invoke();

                _mSubElementsOnMoving.Remove(elementName);
                _mShowSubElementCallCoroutines[elementName] = null;
            }, finishTime);
        }

        UIManager.ShowUiElement(elementName);
    }

    public IOnPanelHide DoOnHideBegin(PanelHandler handler)
    {
        _mOnHideBegin += handler;
        return this;
    }

    public IOnPanelHide DoOnHideCompleted(PanelHandler handler)
    {
        _mOnHideCompleted += handler;
        return this;
    }

    public void HideSubElements(string elementName, UnityAction callOnComplete = null)
    {
        if (elementName == _mPanelName)
        {
            _mSubElementsOnMoving.Add(elementName);

            UIPanelManager.Instance.HidePanel(_mPanelName)
                .DoOnHideCompleted((panel) =>
                {
                    if (callOnComplete != null)
                        callOnComplete.Invoke();

                    _mSubElementsOnMoving.Remove(elementName);
                });
            return;
        }

        StopOngoingCoroutines(elementName);

        UIElement[] elements = GetComponentsInChildren<UIElement>();
        float finishTime = 0;
        for (int i = 0; i < elements.Length; ++i)
        {
            if (elements[i] == this || elements[i].elementName != elementName || !elements[i].isVisible)
                continue;

            if (!_mSubElementsOnMoving.Contains(elementName))
                _mSubElementsOnMoving.Add(elementName);
            finishTime = Mathf.Max(finishTime, elements[i].GetOutAnimationsFinishTime());
        }

        if (_mSubElementsOnMoving.Contains(elementName))
        {
            //use UIPanelManager.CallWithDelay to make sure coroutine will be triggered anyway
            _mHideSubElementCallCoroutines[elementName] = UIPanelManager.Instance.CallWithDelay(() =>
            {
                if (callOnComplete != null)
                    callOnComplete.Invoke();

                _mSubElementsOnMoving.Remove(elementName);

                _mHideSubElementCallCoroutines[elementName] = null;
            }, finishTime);
        }
        UIManager.HideUiElement(elementName);
    }

    public bool IsSubElementMoving(string elementName)
    {
        return _mSubElementsOnMoving.Contains(elementName);
    }

    /// <summary>
    /// DO NOT call, this is an internal method within UI system
    /// </summary>
    public void ClearOnPanelShow()
    {
        _mOnShowBegin = null;
        _mOnShowCompleted = null;
    }

    /// <summary>
    /// DO NOT call, this is an internal method within UI system
    /// </summary>
    public void ClearOnPanelHide()
    {
        _mOnHideBegin = null;
        _mOnHideCompleted = null;
    }

    public void AddListenerOnShowBegin(UnityAction call)
    {
        _mOnShowBeginListener += call;
    }

    public void RemoveListenerOnShowBegin(UnityAction call)
    {
        _mOnShowBeginListener -= call;
    }

    public void ClearListenersOnShowBegin()
    {
        _mOnShowBeginListener = null;
    }

    public void AddListenerOnShowCompleted(UnityAction call)
    {
        _mOnShowCompletedListener += call;
    }

    public void RemoveListenerOnShowCompleted(UnityAction call)
    {
        _mOnShowCompletedListener -= call;
    }

    public void ClearListenersOnShowCompleted()
    {
        _mOnShowCompletedListener = null;
    }

    public void AddListenerOnHideBegin(UnityAction call)
    {
        _mOnHideBeginListener += call;
    }

    public void RemoveListenerOnHideBegin(UnityAction call)
    {
        _mOnHideBeginListener -= call;
    }

    public void ClearListenersOnHideBegin()
    {
        _mOnHideBeginListener = null;
    }

    public void AddListenerOnHideCompleted(UnityAction call)
    {
        _mOnHideCompletedListener += call;
    }

    public void RemoveListenerOnHideCompleted(UnityAction call)
    {
        _mOnHideCompletedListener -= call;
    }

    public void ClearListenersOnHideCompleted()
    {
        _mOnHideCompletedListener = null;
    }

    /// <summary>
    /// DO NOT call this method unless you know what it exactly does
    /// </summary>
    /// <param name="sortingOrder"></param>
    public void UpdateSortingOrder(int sortingOrder)
    {
#if UNITY_EDITOR
        _mCanvas = GetComponent<Canvas>();
        _mChildCanvases = GetComponentsInChildren<Canvas>();
        _mEffects = GetComponentsInChildren<UIEffect>();
#endif
        int oldSortingOrder = _mCanvas.sortingOrder;
        _mCanvas.sortingOrder = sortingOrder;
        for (int i = 0; i < _mChildCanvases.Length; ++i)
        {
            if (_mChildCanvases[i] == _mCanvas || !_mChildCanvases[i].overrideSorting)
                continue;

            //children canvases' sorting order are based on their panel's sorting order
            _mChildCanvases[i].sortingOrder += _mCanvas.sortingOrder - oldSortingOrder;
        }

        for (int i = 0; i < _mEffects.Length; ++i)
        {
            _mEffects[i].UpdateEffectSortingOrder();
        }
    }
    /// <summary>
    /// DO NOT call this method unless you know what it exactly does
    /// </summary>
    /// <param name="depth"></param>
    public void UpdateDepth(float depth)
    {
        Vector3 anchoredPos = _mRectTrans.anchoredPosition3D;
        anchoredPos.z = depth;
        _mRectTrans.anchoredPosition3D = anchoredPos;
    }

    private void StopOngoingCoroutines(string elementName)
    {
        Coroutine showSubCor;
        _mShowSubElementCallCoroutines.TryGetValue(elementName, out showSubCor);
        if (showSubCor != null)
        {
            UIPanelManager.Instance.StopCoroutine(showSubCor);
            _mShowSubElementCallCoroutines[elementName] = null;
            UIManager.HideUiElement(elementName);
        }

        Coroutine hideSubCor;
        _mHideSubElementCallCoroutines.TryGetValue(elementName, out hideSubCor);
        if (hideSubCor != null)
        {
            UIPanelManager.Instance.StopCoroutine(hideSubCor);
            _mHideSubElementCallCoroutines[elementName] = null;
            UIManager.ShowUiElement(elementName);
        }
    }
}
