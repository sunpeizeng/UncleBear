using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    public class CharaModel : MonoBehaviour
    {
        public Transform trsHandR;
        public Transform trsHandL;
        public Transform trsHead;
        public Transform trsMouth;

        public SkinnedMeshRenderer smrBody;

        public Material matBase;
        public Material matHappy;
        public Material matSad;

        CharaMoodEnum _eMood;
        public CharaMoodEnum CurMood
        { get { return _eMood; } }

        public void Awake()
        {
            ChangeCharaMood(CharaMoodEnum.Normal);
        }

        public void ChangeCharaMood(CharaMoodEnum mood)
        {
            if (smrBody == null)
                return;
            _eMood = mood;
            var mats = smrBody.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i].name.ToLower().Contains("cloth"))
                    continue;

                switch (mood)
                {
                    case CharaMoodEnum.Normal:
                        mats[i] = matBase;
                        break;
                    case CharaMoodEnum.Happy:
                        mats[i] = matHappy;
                        break;
                    case CharaMoodEnum.Sad:
                        mats[i] = matSad;
                        break;
                }
            }

            smrBody.materials = mats;
        }

        public void ResetCharaMood()
        {
            if(_eMood != CharaMoodEnum.Normal)
                ChangeCharaMood(CharaMoodEnum.Normal);
        }
    }
}