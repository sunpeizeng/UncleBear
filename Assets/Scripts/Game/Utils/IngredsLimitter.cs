using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{

    public class IngredsLimitter : MonoBehaviour
    {
        Rigidbody[] _bodies;

        void Awake()
        {
            _bodies = gameObject.GetComponentsInChildren<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            //for (int i = 0; i < _bodies.Length; i++)
            //{
            //    var newPos = new Vector3(transform.position.x, _bodies[i].transform.position.y, transform.position.z);
            //    var disVec = _bodies[i].transform.position - newPos;
            //    if (disVec.magnitude > 1f)
            //    {
            //        _bodies[i].transform.position = newPos + disVec.normalized * 1f;
            //    }
            //}
        }

        //防止材料爆开
        void FixedUpdate()
        {
            if (_bodies == null)
                return;
            for (int i = 0; i < _bodies.Length; i++)
            {
                if (_bodies[i] != null)
                {
                    var newX = Mathf.Clamp(_bodies[i].velocity.x, -1, 1);
                    var newZ = Mathf.Clamp(_bodies[i].velocity.z, -1, 1);
                    _bodies[i].velocity = new Vector3(newX, _bodies[i].velocity.y, newZ);
                }
            }
        }

    }
}
