using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    //自由切割计数器
    public class CutterCounter : MonoBehaviour
    {
        public int _nCount;
        public int _nLimit = 10;

        void Awake()
        {
            ResetCount();
        }

        public void ResetCount()
        {
            _nCount = 0;
        }

        public int nCutCount
        {
            set { _nCount = value; }
            get { return _nCount; }
        }

        public int nCutLimit {
            set { _nLimit = value; }
            get { return _nLimit; }
        }

        public bool bIsLimitCount
        {
            get
            {
                return _nCount >= _nLimit;
            }
        }
    }
}
