using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Za redoslijed pokretanja datoteka
public class MainThreadDispatcher : MonoBehaviour
{
    private static Queue<System.Action> actions = new Queue<System.Action>();

    public void Update()
    {
        lock (actions)
        {
            while (actions.Count > 0)
            {
                actions.Dequeue().Invoke();
            }
        }
    }

    public static void Enqueue(System.Action action)
    {
        lock (actions)
        {
            actions.Enqueue(action);
        }
    }
}
