using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SpriteFrameAnim : MonoBehaviour {

    public Sprite[] mSprites;
    public float mFrameRate = 30f;
    public bool mAutoPlay = true;
    public bool mLoop = true;

    private bool _mStartPlay = false;
    private int _mFrameIndex = 0;
    private float _mTimer = 0f;
    private Image _mImg;

    void Awake()
    {
        _mImg = GetComponent<Image>();
    }

	// Use this for initialization
	void Start ()
    {
        if (mAutoPlay)
            _mStartPlay = true;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (_mStartPlay)
        {
            if (_mTimer > 1 / mFrameRate)
            {
                ++_mFrameIndex;
                if (mLoop)
                    _mFrameIndex %= mSprites.Length;
                else if (_mFrameIndex >= mSprites.Length)
                {
                    _mTimer = 0f;
                    _mStartPlay = false;
                    return;
                }
                _mImg.sprite = mSprites[_mFrameIndex];
                _mTimer = 0f;
            }
            else
            {
                _mTimer += Time.deltaTime;
            }
        }
	}

    public void Play()
    {
        _mStartPlay = true;
    }

    public void Reset()
    {
        _mFrameIndex = 0;
        _mImg.sprite = mSprites[0];
        _mStartPlay = false;
    }
}
