using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanAnim : MonoBehaviour {

    public Sprite[] spOils;
    SpriteRenderer _renderer;
    float _fTime;
    public float _fDelta = 0.1f;

	// Use this for initialization
	void Start () {
        _renderer = GetComponent<SpriteRenderer>();
        _fTime = _fDelta;
        _renderer.sprite = spOils[0];
    }


    int _index;
    // Update is called once per frame
    void Update () {
       
        if (_fTime > 0)
        {
            _fTime -= Time.deltaTime;
            if (_fTime < 0)
            {
                _index += 1;
                if (_index > spOils.Length - 1)
                    _index = 0;
                _renderer.sprite = spOils[_index];
                _fTime = _fDelta;
            }
        }

    }
}
