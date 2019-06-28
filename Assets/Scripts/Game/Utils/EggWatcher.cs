using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class EggWatcher : MonoBehaviour {

    public Action<GameObject> onEnterBowl;

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null && 
            other.transform.parent.name.Contains("Spoon"))
        {
            gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
        else if (other.transform.parent != null && 
            other.transform.parent.name.Contains("EggBowlSmall"))
        {
            if (onEnterBowl != null)
                onEnterBowl.Invoke(gameObject);
        }
    }
}
