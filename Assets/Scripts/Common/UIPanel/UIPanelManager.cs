using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DoozyUI;

public class UIPanelManager : DoozyUI.Singleton<UIPanelManager>
{
    private int _mSortingOrderInterval = 10;
    private int _mZInsterval = -100;

    private Dictionary<string, UIPanel> _mPanelRegistry = new Dictionary<string, UIPanel>();
    private SimpleObservableList<UIPanel> _mActivePanels = new SimpleObservableList<UIPanel>();

    private bool _mIsDirty = false;
    private bool _mUpdateSibling = false;

    private Dictionary<string, Coroutine> _mShowCoroutines = new Dictionary<string, Coroutine>();
    private Dictionary<string, Coroutine> _mHideCoroutines = new Dictionary<string, Coroutine>();

    public SimpleObservableList<UIPanel> ActivePanelList
    {
        get
        {
            return _mActivePanels;
        }
    }

    void Awake()
    {
        _mActivePanels.ItemAdded += OnAddToActivePanels;
        _mActivePanels.ItemRemoved += OnRemoveFromActivePanels;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (_mIsDirty)
        {
            UpdateAllPanels();
            _mIsDirty = false;
        }

        if (_mUpdateSibling)
        {
            for (int i = 0; i < _mActivePanels.Count; ++i)
            {
                _mActivePanels[i].transform.SetAsLastSibling();
            }
            _mUpdateSibling = false;
        }
    }

    void OnAddToActivePanels(object sender, object item)
    {
        if (sender != _mActivePanels)
            return;

        UIPanel panelAdded = (UIPanel)item;

        //insert added panel to a proper position according to its layer
        int i = _mActivePanels.Count - 2;
        while (i >= 0)
        {
            if (panelAdded.mLayer > _mActivePanels[i].mLayer
                || (panelAdded.mLayer == _mActivePanels[i].mLayer && panelAdded.transform.GetSiblingIndex() > _mActivePanels[i].transform.GetSiblingIndex()))
            {
                if (i < _mActivePanels.Count - 2)
                {
                    _mActivePanels.RemoveAt(_mActivePanels.Count - 1);
                    _mActivePanels.Insert(i + 1, panelAdded);
                }
                break;
            }

            --i;
        }

        if (i < 0)
        {
            _mActivePanels.RemoveAt(_mActivePanels.Count - 1);
            _mActivePanels.Insert(0, panelAdded);
        }

        UpdatePanelsBeginAt(i + 1);

        if (panelAdded.UseOriginalSiblingIndex)
        {
            panelAdded.UseOriginalSiblingIndex = false;
            _mUpdateSibling = true;
        }
    }

    void OnRemoveFromActivePanels(object sender, object item, int oldIndex)
    {
        if (sender != _mActivePanels)
            return;

        UIPanel panelRemoved = (UIPanel)item;

        //if the removed panel was not the last element in the list
        if (oldIndex < _mActivePanels.Count)
        {
            panelRemoved.DoOnHideCompleted(
                (panel)=> {
                    UpdatePanelsBeginAt(oldIndex);
                });
        }

    }

    public void RegisterPanel(UIPanel panel)
    {
        string panelName = panel.GetComponent<UIElement>().elementName;
        if (_mPanelRegistry.ContainsKey(panelName))
        {
            //panel has already been registered, do nothing
            return;
        }

        _mPanelRegistry.Add(panelName, panel);
    }

    public void UnregisterPanel(UIPanel panel)
    {
        string panelName = panel.GetComponent<UIElement>().elementName;
        if (_mPanelRegistry.ContainsKey(panelName))
        {
            _mPanelRegistry.Remove(panelName);
        }
    }

    public UIPanel GetPanel(string panelName)
    {
        UIPanel panel = null;
        _mPanelRegistry.TryGetValue(panelName, out panel);
        return panel;
    }

    public UIPanel LoadPanel(string panelName)
    {
        GameObject prefab = Resources.Load<GameObject>("UI/Panels/" + panelName);
        UIPanel panel = null;
        if (prefab != null)
        {
            GameObject panelObj = Instantiate<GameObject>(prefab, UIManager.GetUiContainer, false);
            panel = panelObj.GetComponent<UIPanel>();
            if (panel != null)
            {
                panel.UseOriginalSiblingIndex = false;
                //rename cloned gameobject with elementName
                panelObj.name = panelName;

                UIElement[] elements = panelObj.GetComponentsInChildren<UIElement>();
                for (int i = 0; i < elements.Length; ++i)
                {
                    //if a panel is loaded on the fly, we force to set 'startHidden' to 'true' and 'animateAtStart' to 'false'
                    elements[i].startHidden = true;
                    elements[i].animateAtStart = false;

                    //do start after cloned immediately for every child uielement
                    elements[i].Start();
                }
            }
            else
            {
                LogUtil.LogError("UIPanelManager", "there is no UIPanel or a derived script attached to '{0}'", panelName);
            }

            Resources.UnloadUnusedAssets();
        }
        else
        {
            LogUtil.LogError("UIPanelManager", "no UIPanel prefab with name '{0}' found in 'Resources/UI/Panels'", panelName);
        }

        return panel;
    }

    public IOnPanelShow ShowPanel(string panelName, float delay = 0f)
    {
        UIPanel panel = GetPanel(panelName);
        if (panel == null)
        {
            panel = LoadPanel(panelName);        
        }

        if (panel != null)
        {
            StopOngoingCoroutines(panel);

            if (!panel.IsActive)
            {
                panel.ClearOnPanelShow();
                _mShowCoroutines[panelName] = this.CallWithDelay(() =>
                {
                    UIManager.ShowUiElement(panel.PanelName);
                    _mShowCoroutines[panelName] = null;
                }, delay);               
            }
        }

        return panel;
    }

    public IOnPanelShow ShowPanel(UIPanel panel, float delay = 0f)
    {
        return ShowPanel(panel.PanelName, delay);
    }

    public IOnPanelHide HidePanel(string panelName, float delay = 0f)
    {
        UIPanel panel = GetPanel(panelName);
        if (panel != null)
        {
            StopOngoingCoroutines(panel);

            if (panel.IsActive)
            {
                panel.ClearOnPanelHide();

                _mHideCoroutines[panelName] = this.CallWithDelay(() =>
                {
                    UIManager.HideUiElement(panel.PanelName);
                    _mHideCoroutines[panelName] = null;
                }, delay);
            }
        }

        return panel;
    }

    public IOnPanelHide HidePanel(UIPanel panel, float delay = 0f)
    {
        return HidePanel(panel.PanelName, delay);
    }

    public void DestroyPanel(string panelName)
    {
        UIPanel panel = GetPanel(panelName);
        if (panel != null)
        {
            if (panel.IsActive && !panel.IsHiding)
            {
                LogUtil.LogError("UIPanelManager", "tring to destroy UIPanel< {0} > which is still active or hiding", panelName);
                return;
            }

            LogUtil.Log("UIPanelManager", "destroy UIPanel< {0} >", panelName);
            GameObject.Destroy(panel.gameObject);
        }
    }

    public void DestroyPanel(UIPanel panel)
    {
        DestroyPanel(panel.PanelName);
    }

    private void UpdateAllPanels()
    {
        LogUtil.Log("UIPanelManager", "UpdatePanels");

        //update from list beginning, update every panel in active
        UpdatePanelsBeginAt(0);
    }

    private void UpdatePanelsBeginAt(int startIndex)
    {
        //this is an optimization, choose the side with less elements to update
        bool forward = startIndex + 1 > _mActivePanels.Count - startIndex;
        UIPanel panelPrev = null;
        if (forward)
        {
            panelPrev = startIndex - 1 < 0 ? null : _mActivePanels[startIndex - 1];
            for (int i = startIndex; i < _mActivePanels.Count; ++i)
            {
                UIPanel panel = _mActivePanels[i];

                //update sorting order according to the previous UIPanel
                panel.UpdateSortingOrder(panelPrev == null ? 0 : panelPrev.GetComponent<Canvas>().sortingOrder + _mSortingOrderInterval);
                //update z position according to the previous UIPanel
                panel.UpdateDepth(panelPrev == null ? 0 : panelPrev.GetComponent<RectTransform>().anchoredPosition3D.z + _mZInsterval);

                panelPrev = panel;
            }
        }
        else
        {
            panelPrev = startIndex + 1 > _mActivePanels.Count - 1 ? null : _mActivePanels[startIndex + 1]; 
            for (int i = startIndex; i >= 0; --i)
            {
                UIPanel panel = _mActivePanels[i];

                //update sorting order according to the previous UIPanel
                panel.UpdateSortingOrder(panelPrev == null ? 0 : panelPrev.GetComponent<Canvas>().sortingOrder - _mSortingOrderInterval);
                //update z position according to the previous UIPanel
                panel.UpdateDepth(panelPrev == null ? 0 : panelPrev.GetComponent<RectTransform>().anchoredPosition3D.z - _mZInsterval);

                panelPrev = panel;
            }
        }
    }

    public void SetSortingOrderInterval(int sortingOrderInterval)
    {
        if (_mSortingOrderInterval == sortingOrderInterval)
            return;

        _mSortingOrderInterval = sortingOrderInterval;
        _mIsDirty = true;
    }
    
    public void SetZInterval(int zInterval)
    {
        if (_mZInsterval == zInterval)
            return;

        _mZInsterval = zInterval;
        _mIsDirty = true;
    }

    private void StopOngoingCoroutines(UIPanel panel)
    {
        Coroutine hideCoroutine = null;
        _mHideCoroutines.TryGetValue(panel.PanelName, out hideCoroutine);
        if (hideCoroutine != null)
        {
            panel.ClearOnPanelHide();
            StopCoroutine(hideCoroutine);
            _mHideCoroutines[panel.PanelName] = null;
        }

        Coroutine showCoroutine = null;
        _mShowCoroutines.TryGetValue(panel.PanelName, out showCoroutine);
        if (showCoroutine != null)
        {
            panel.ClearOnPanelShow();
            StopCoroutine(showCoroutine);
            _mShowCoroutines[panel.PanelName] = null;
        }
    }
}
