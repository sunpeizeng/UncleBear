using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    //自由切割时间间隔,防止手指抖动也切出一堆细小碎片
    public class CutterTimer : MonoBehaviour
    {
        bool _bLimitTimerActive;
        float _fCountTime;
        float _fLimitTime = 0.3f;


        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (_bLimitTimerActive)
            {
                _fCountTime += Time.deltaTime;
                if (_fCountTime > _fLimitTime)
                    _bLimitTimerActive = false;
            }
        }

        public void ActiveTimer()
        {
            _bLimitTimerActive = true;
            _fCountTime = 0;
        }

        public bool bCutLimiting
        {
            get { return _bLimitTimerActive; }
        }
    }
}
