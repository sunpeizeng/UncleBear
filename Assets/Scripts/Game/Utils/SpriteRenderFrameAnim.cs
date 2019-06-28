using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteRenderFrameAnim : MonoBehaviour {
    public Sprite[] mSprites;
    public float mFrameRate = 30f;
    public bool mAutoPlay = true;
    public bool mLoop = false;
    public bool mReverse = false;

    private bool _mStartPlay = false;
    private int _mFrameIndex = 0;
    private float _mTimer = 0f;
    private SpriteRenderer _mImg;

    public System.Action<bool> cbOnSinglePlayFinished;

    void Awake()
    {
        _mImg = GetComponent<SpriteRenderer>();
    }

    // Use this for initialization
    void Start()
    {
        if (mAutoPlay)
            _mStartPlay = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_mStartPlay)
        {
            if (_mTimer > 1 / mFrameRate)
            {
                _mFrameIndex += mReverse ? -1 : 1;
                if (mLoop)
                {
                    if (_mFrameIndex < 0)
                        _mFrameIndex += mSprites.Length;
                    _mFrameIndex %= mSprites.Length;
                }
                else
                {
                    if (!mReverse && _mFrameIndex >= mSprites.Length)
                    {
                        _mTimer = 0f;
                        _mStartPlay = false;
                        if (cbOnSinglePlayFinished != null)
                            cbOnSinglePlayFinished.Invoke(false);
                        return;
                    }
                    else if (mReverse && _mFrameIndex < 0)
                    {
                        _mTimer = 0f;
                        _mStartPlay = false;
                        if (cbOnSinglePlayFinished != null)
                            cbOnSinglePlayFinished.Invoke(true);
                        return;
                    }
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

    public void Play(bool rev = false)
    {
        _mStartPlay = true;
        mReverse = rev;
    }

    public void ResetAndStop()
    {
        _mFrameIndex = 0;
        _mImg.sprite = mSprites[0];
        _mStartPlay = false;
    }
}
