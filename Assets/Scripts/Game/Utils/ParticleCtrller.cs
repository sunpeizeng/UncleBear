using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCtrller : MonoBehaviour {
    public float m_Time = 0;
    public float m_maxTime = 0;

    ParticleSystem[] particle_systems;
    List<ParticleSystem> listLoopPs = new List<ParticleSystem>();
    Animation[] particle_animations;

    private Dictionary<ParticleSystem, float> systemTimeList = new Dictionary<ParticleSystem, float>();

    public void Awake()
    {
        particle_systems = GetComponentsInChildren<ParticleSystem>();
        particle_animations = GetComponentsInChildren<Animation>();
    }

    public void CalcEffMaxTime()
    {
        m_maxTime = 0;
        if (particle_systems != null && particle_animations.Length > 0)
        {
            float length = 0;
            foreach (Animation anim in particle_animations)
            {
                if (anim.clip != null)
                {
                    length = anim.clip.length > length ? anim.clip.length : length;
                }
            }

            m_maxTime = length;
        }

        if (particle_animations != null && particle_systems.Length > 0)
        {
            float durLen = 0;
            float lifeLen = 0;
            float delayLen = 0;
            foreach (ParticleSystem system in particle_systems)
            {
                delayLen = system.startDelay > delayLen ? system.startDelay : delayLen;
                lifeLen = system.startLifetime > lifeLen ? system.startLifetime : lifeLen;
                durLen = system.duration > durLen ? system.duration : durLen;
                if (system.loop)
                    listLoopPs.Add(system);
            }

            float newTime = lifeLen + durLen + delayLen;
            m_maxTime = newTime > m_maxTime ? newTime : m_maxTime;
        }
    }

    public void OnEnable()
    {
        m_Time = 0;
        CalcEffMaxTime();
        foreach (ParticleSystem system in particle_systems)
        {
            if (listLoopPs.Contains(system))
                system.loop = true;
            system.enableEmission = true;
        }
    }

    public void Update()
    {

        m_Time += Time.deltaTime;

        if (m_maxTime >= 0 && m_Time >= m_maxTime)
        {
            DestroyEffectGradually();
        }

    }

    //! 销毁特效
    public void DestroyEffect()
    {
        GameObject.Destroy(gameObject);
    }

    //! 使MaxTime控制删除失效
    public void SetMaxTimeUseless()
    {
        m_maxTime = -1;
    }

    public void ResetMaxTimeUseful(float newTime = -1)
    {
        if (newTime == -1)
            m_maxTime = m_Time;
        else
            m_maxTime = newTime;
    }


    public void DestroyEffectGradually()
    {
        //Debug.Log(gameObject.name);
        if (particle_systems.Length > 0)
        {
            bool stop = true;
            foreach (ParticleSystem system in particle_systems)
            {
                if (system.main.loop == true)
                {
                    system.loop = false;
                }
                system.enableEmission = false;

                if (system.isPlaying)
                {
                    stop = false;
                }
            }

            if (stop || m_Time > m_maxTime + 5)//一般duration不会超过5
            {
                Lean.LeanPool.Despawn(gameObject);
            }
        }
        else
        {
            Lean.LeanPool.Despawn(gameObject);
        }
    }
}
