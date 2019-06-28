using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    public class Singleton<T> where T : class, new()
    {
        private static T _instance = null;

        public static T CreateInstance()
        {
            _instance = new T();
            return _instance;
        }

        public static void DestroyInstance()
        {
            _instance = null;
        }

        public static T Instance
        {
            get
            {
                return _instance;
            }
        }
    }
}
