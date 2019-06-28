using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUncleBear : MonoBehaviour {

    public GameObject objApple;
    public GameObject objPineapple;

    bool _bHello = true;
    Animation _anim;
	// Use this for initialization
	void Start () {
        _anim = gameObject.GetComponent<Animation>();
        _anim.Play("Take 001");
    }
	
	// Update is called once per frame
	void Update () {
        if (_anim["Take 001"].normalizedTime >= 1)
        {
            _anim.CrossFade("Take 002", 0.2f);
            objApple.SetActive(true);
            objPineapple.SetActive(true);
        }
        else if (_anim["Take 002"].normalizedTime >= 1)
        {
            _anim.CrossFade("Take 001", 0.2f);
            objApple.SetActive(false);
            objPineapple.SetActive(false);
        }
    }
}
