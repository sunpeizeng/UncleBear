using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    //不用配置表了,直接在prefab上设置
    public class CharaData : MonoBehaviour
    {
        public enum CharaClassType
        {
            None,
            Chef,
            Waiter,
            Customer,
        }
        public CharaClassType eType = CharaClassType.None;
        public float fMovespeed = 10;
    }
}
