using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    public class DoorDetector : MonoBehaviour {
        public Animation _anim;


        //门上的碰撞要细,而且带刚体
        void OnTriggerEnter(Collider other)
        {
            var chara = other.transform.parent.GetComponent<CharaBase>();
            if (chara != null)
            {
                if (chara.GetClassType() == CharaData.CharaClassType.Customer)
                {
                    if (!(chara as CharaCustomer).bIsLeaved)
                        _anim.Play("open");
                    else
                        _anim.Play("close");
                    DoozyUI.UIManager.PlaySound("2木门开启", transform.position);
                }
                else if (chara.GetClassType() == CharaData.CharaClassType.Chef)
                {
                    _anim.Play("anim_openCabin");
                }
            }
        }
    }
}
