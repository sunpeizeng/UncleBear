using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public static class ExtensionMethods
{

    /// <summary>
    /// Gets or add a component. Usage example:
    /// BoxCollider boxCollider = transform.GetOrAddComponent<BoxCollider>();
    /// </summary>
    public static T GetOrAddComponent<T>(this Component obj) where T : Component
    {
        T result = obj.GetComponent<T>();
        if (result == null)
        {
            result = obj.gameObject.AddComponent<T>();
        }
        return result;
    }

    public static Coroutine CallWithDelay(this MonoBehaviour obj, UnityAction call, float delay)
    {
        return obj.StartCoroutine(doCallWithDelay(call, delay));
    }

    static IEnumerator doCallWithDelay(UnityAction call, float delay)
    {
        //         float timer = delay;
        //         while (timer > 0)
        //         {
        //             timer -= Time.deltaTime;
        // 
        //             yield return null;
        //         }

        if (delay <= 0)
            yield return null;
        else
        {
            float start = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup < start + delay)
            {
                yield return null;
            }
        }

        if (call != null)
            call.Invoke();
    }
}

