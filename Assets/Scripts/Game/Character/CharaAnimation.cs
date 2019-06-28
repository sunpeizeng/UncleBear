using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    public class CharaAnimation
    {
        public enum AnimPlayType
        {
            Normal,
            Crossfade,
        }

        CharaBase _owner;
        Animation _anim;
        string _strCurAnimState;
        string _strCurAnimClipName;
        float _fCurAnimSpeed;
        float _fFadeTime;

        Dictionary<string, string[]> _dictAnimMatches;


        public CharaAnimation(CharaBase owner, Animation animCompo)
        {
            _owner = owner;
            _anim = animCompo;
            _dictAnimMatches = new Dictionary<string, string[]>();
        }

        //path为带有角色名路径
        public void LoadAnimConf(string path)
        {
            var items = SerializationManager.LoadFromCSV<CharaAnimItem>(path);
            if (items != null)
            {
                items.ForEach(p =>
                {
                    _dictAnimMatches.Add(p.strState, p.strClips.Split('|'));
                });
            }
            _fFadeTime = 0;
        }

        public bool IsAnimFading()
        {
            return _fFadeTime > 0;
        }

        public void TickAnimation(float deltaTime)
        {
            if (_fFadeTime > 0)
            {
                _fFadeTime -= deltaTime;
            }
        }

        public void SetCurAnimSpeed(float val)
        {
            if (!string.IsNullOrEmpty(_strCurAnimClipName))
            {
                _anim[_strCurAnimClipName].speed = val;
            }
        }

        //设置一个动画状态的层级和混合模式
        public void SetAnimBlend(string state, int layer = 1, float weight = 1f, AnimationBlendMode mode = AnimationBlendMode.Additive)
        {
            _anim[state].layer = layer;
            _anim[state].blendMode = mode;
            _anim[state].weight = weight;
        }

        //播放某状态动画
        public float AnimPlay(string stateName, bool loop = false, float speed = 1f, AnimPlayType type = AnimPlayType.Crossfade, float fadeLen = 0.2f)
        {
            float animLen = 0;
            if (_dictAnimMatches.ContainsKey(stateName))
            {
                var ranAnimId = Random.Range(0, _dictAnimMatches[stateName].Length);
                ////!Hack
                //if (_dictAnimMatches[stateName][ranAnimId] == _strCurAnimClipName)
                //    type = AnimPlayType.Normal;
                _strCurAnimClipName = _dictAnimMatches[stateName][ranAnimId];
                _fCurAnimSpeed = speed;

                switch (type)
                {
                    case AnimPlayType.Normal:
                        animLen = AnimNormalPlay(_strCurAnimClipName, loop, speed);
                        break;
                    case AnimPlayType.Crossfade:
                        animLen = AnimCrossFade(_strCurAnimClipName, loop, speed, fadeLen);
                        break;
                }
            }
            return animLen;
        }
        //强切动画clip
        float AnimNormalPlay(string name, bool loop, float speed = 1f)
        {
            _anim[name].speed = speed;
            _anim[name].wrapMode = loop ? WrapMode.Loop : WrapMode.ClampForever;
            _anim.Rewind(name);
            _anim.Play(name, PlayMode.StopAll);
            return _anim[name].length;
        }
        //过渡动画clip
        float AnimCrossFade(string name, bool loop, float speed = 1f, float fadeLength = 0.2f)
        {
            _anim[name].speed = speed;
            _anim[name].wrapMode = loop ? WrapMode.Loop : WrapMode.ClampForever;
            _anim.CrossFade(name, fadeLength);
            _fFadeTime = fadeLength;
            return _anim[name].length;
        }
        //停止动画
        public void AnimStop()
        {
            _anim.Stop();
        }
        //动画暂停
        public void AnimPause()
        {
            _anim[_strCurAnimClipName].speed = 0;
        }
        public void AnimUnPause()
        {
            _anim[_strCurAnimClipName].speed = _fCurAnimSpeed;
        }

    }

    public class CharaAnimItem : ICSVDeserializable
    {
        public string strState;
        public string strClips;

        public void CSVDeserialize(Dictionary<string, string[]> data, int index)
        {
            strState = data["State"][index];
            strClips = data["Clips"][index];
        }
    }
}
