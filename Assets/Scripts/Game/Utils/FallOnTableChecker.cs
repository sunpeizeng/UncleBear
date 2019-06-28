using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallOnTableChecker : MonoBehaviour {

    System.Action<GameObject> OnCollideCallback;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetCollisionCallback(System.Action<GameObject> cb)
    {
        OnCollideCallback = cb;
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.name == "TableSurface")
        {
            if (OnCollideCallback != null)
                OnCollideCallback.Invoke(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "TableSurface")
        {
            if (OnCollideCallback != null)
                OnCollideCallback.Invoke(gameObject);
        }
    }
}
