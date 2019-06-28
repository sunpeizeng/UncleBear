using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UncleBear
{
    public enum CharaAnimStateEnum
    {
        Idle,
        IdleSitting,
        Move,
        Greet,
        Wait,
        SitDown,
        StandUp,
        Eat,
        Happy,
        Sad,
        TakeNotes,
        TransDish,
        Pay,
    }

    public enum CharaMoodEnum
    {
        Normal,
        Happy,
        Sad,
    }


    public class CharaBase : MonoBehaviour
    {
        protected CharaEnum _eName;

        protected CharaAnimation _anim;
        public CharaAnimation AnimCtrller { get { return _anim; } }
        protected CharaData _data;
        protected CharaModel _model;
        protected StateMachine<CharaBase> _fsmChara;

        protected bool _bMoving;

        void Awake()
        {
            _model = gameObject.AddMissingComponent<CharaModel>();
            _data = gameObject.AddMissingComponent<CharaData>();
            _anim = new CharaAnimation(this, gameObject.GetComponent<Animation>());
            _fsmChara = new StateMachine<CharaBase>(this);
        }

        // Use this for initialization
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            _fsmChara.TickState(Time.deltaTime);
            _anim.TickAnimation(Time.deltaTime);
        }

        public virtual void Initialize(CharaEnum type, string animId)
        {
            _eName = type;
            _anim.LoadAnimConf("Data/Characters/" + animId);

            //_fsmChara.AddState(new CharaStateIdle((int)CharaActionType.Idle));
            //_fsmChara.AddState(new CharaStateMove((int)CharaActionType.Move));
        }

        public void SetTransform(Vector3 pos, Vector3 angle, float scale = -1)
        {
            transform.DOKill();
            transform.position = pos;
            transform.localEulerAngles = angle;

            if (scale >= 0)
                transform.localScale = Vector3.one * scale;
        }

        public void PathMove(List<Vector3> pathPoints, bool isForward = true, System.Action callback = null)
        {
            //TODO::看情况是不是包含原位置,目前是不包含
            var points = new List<Vector3>();
            points.AddRange(pathPoints);
            if (points.Count < 1)
                return;
            float totalDis = 0;
            for (int i = 0; i < points.Count; i++)
            {
                totalDis += i == 0 ? Vector3.Distance(points[i], transform.position) : Vector3.Distance(points[i], points[i - 1]);
            }
            float moveTime = totalDis / _data.fMovespeed;
            //默认线性正向
            if (!isForward)
                points.Reverse();
            transform.DOKill();
            transform.DOPath(points.ToArray(), moveTime, PathType.CatmullRom).SetEase(Ease.Linear).SetLookAt(0.01f).OnComplete(() =>
            {
                if (callback != null) callback.Invoke();
            }).SetAutoKill(true);
            //_fsmChara.ChangeState((int)CharaActionType.Move);
            _bMoving = true;
        }
        public void StopMove()
        {
            if (_bMoving)
            {
                transform.DOKill();
                _bMoving = false;
            }
            _anim.AnimStop();
        }

        public void ChangeCharaState(CharaStateEnum action, object param = null)
        {
            _fsmChara.ChangeState((int)action, param);
        }
        public CharaStateEnum GetCurState()
        {
            return (CharaStateEnum)_fsmChara.CurState.StateEnum;
        }
        public CharaData.CharaClassType GetClassType()
        {
            return _data.eType;
        }
        public CharaModel GetCharaModel()
        {
            return _model;
        }
    }
}
