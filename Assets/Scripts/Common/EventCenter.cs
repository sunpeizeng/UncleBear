using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using DoozyUI;

public class EventCenter : DoozyUI.Singleton<EventCenter>
{
    private UITrigger _mButtonClickTrigger;
    private UITrigger _mGameEventTrigger;

    private Dictionary<string, UnityAction> _mButtonEventsRegistry = new Dictionary<string, UnityAction>();
    private Dictionary<string, UnityAction> _mGameEventsRegistry = new Dictionary<string, UnityAction>();

    void Awake()
    {
        //set active false to ensure UITrigger's OnEnable will be called after 'triggerOnButtonClick' and 'dispatchAll' been set already
        gameObject.SetActive(false);

        _mButtonClickTrigger = gameObject.AddComponent<UITrigger>();
        _mButtonClickTrigger.triggerOnButtonClick = true;
        _mButtonClickTrigger.dispatchAll = true;

        _mGameEventTrigger = gameObject.AddComponent<UITrigger>();
        _mGameEventTrigger.triggerOnGameEvent = true;
        _mGameEventTrigger.dispatchAll = true;

        gameObject.SetActive(true);
    }

    void OnEnable()
    {
        if (_mButtonClickTrigger != null)
        {
            //remove first, in case OnButtonEvent is added twice
            _mButtonClickTrigger.RemoveListener(OnButtonEvent);
            _mButtonClickTrigger.AddListener(OnButtonEvent);
        }
        if (_mGameEventTrigger != null)
        {
            //remove first, in case OnGameEvent is added twice
            _mGameEventTrigger.RemoveListener(OnGameEvent);
            _mGameEventTrigger.AddListener(OnGameEvent);
        }
    }

    void OnDisable()
    {
        if (_mButtonClickTrigger != null) _mButtonClickTrigger.RemoveListener(OnButtonEvent);
        if (_mGameEventTrigger != null) _mGameEventTrigger.RemoveListener(OnGameEvent);
    }

// 	// Use this for initialization
// 	void Start () {
// 		
// 	}
// 	
// 	// Update is called once per frame
// 	void Update () {
// 		
// 	}

    public void RegisterGameEvent(string gameEvent, UnityAction callback)
    {
        UnityAction callbacks = null;
        if (_mGameEventsRegistry.TryGetValue(gameEvent, out callbacks))
        {
            callbacks += callback;
            _mGameEventsRegistry[gameEvent] = callbacks;
        }
        else
        {
            _mGameEventsRegistry.Add(gameEvent, callback);
        }
    }

    public void UnregisterGameEvent(string gameEvent, UnityAction callback)
    {
        UnityAction callbacks = null;
        if (_mGameEventsRegistry.TryGetValue(gameEvent, out callbacks))
        {
            callbacks -= callback;
            _mGameEventsRegistry[gameEvent] = callbacks;
        }
    }

    public void RegisterButtonEvent(string buttonName, UnityAction callback)
    {
        UnityAction callbacks = null;
        if (_mButtonEventsRegistry.TryGetValue(buttonName, out callbacks))
        {
            callbacks += callback;
            _mButtonEventsRegistry[buttonName] = callbacks;
        }
        else
        {
            _mButtonEventsRegistry.Add(buttonName, callback);
        }
    }

    public void UnregisterButtonEvent(string buttonName, UnityAction callback)
    {
        UnityAction callbacks = null;
        if (_mButtonEventsRegistry.TryGetValue(buttonName, out callbacks))
        {
            callbacks -= callback;
            _mButtonEventsRegistry[buttonName] = callbacks;
        }
    }

    void OnGameEvent(string gameEvent)
    {
        LogUtil.Log("EventCenter", "receive game event: < {0} >", gameEvent);

        UnityAction callbacks = null;
        if (_mGameEventsRegistry.TryGetValue(gameEvent, out callbacks))
        {
            callbacks();
        }
    }

    void OnButtonEvent(string buttonName)
    {
        LogUtil.Log("EventCenter", "receive button click, button name: < {0} >", buttonName);

        UnityAction callbacks = null;
        if (_mButtonEventsRegistry.TryGetValue(buttonName, out callbacks))
        {
            callbacks();
        }
    }
}
