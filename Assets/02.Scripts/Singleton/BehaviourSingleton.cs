using UnityEngine;
using System.Collections;

public abstract class BehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T i = null;

    public static T Instance
    {
        get
        {
            if (i == null)
            {
                i = FindFirstObjectByType(typeof(T)) as T;
                if (i == null)
                {

                }
            }
            return i;
        }
        set
        {
            i = value;
        }
    }
}