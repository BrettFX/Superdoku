using System.Collections.Generic;
using System.Threading;
using System;
using UnityEngine;

namespace Superdoku
{
    /**
     * To circumvent Unity errors that occur when UnityEngine calls are made outside the main thread,
     * this dispatcher makes it so UnityEngine calls from async threads are redirected (dispatched)
     * to the main thread.
     */
    public class Dispatcher : MonoBehaviour
    {
        public static void RunAsync(Action action)
        {
            ThreadPool.QueueUserWorkItem(o => action());
        }

        public static void RunAsync(Action<object> action, object state)
        {
            ThreadPool.QueueUserWorkItem(o => action(o), state);
        }

        public static void RunOnMainThread(Action action)
        {
            lock (_backlog)
            {
                _backlog.Add(action);
                _queued = true;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (_instance == null)
            {
                _instance = new GameObject("Dispatcher").AddComponent<Dispatcher>();
                DontDestroyOnLoad(_instance.gameObject);
            }
        }

        private void Update()
        {
            if (_queued)
            {
                lock (_backlog)
                {
                    var tmp = _actions;
                    _actions = _backlog;
                    _backlog = tmp;
                    _queued = false;
                }

                foreach (var action in _actions)
                    action();

                _actions.Clear();
            }
        }

        static Dispatcher _instance;
        static volatile bool _queued = false;
        static List<Action> _backlog = new List<Action>(8);
        static List<Action> _actions = new List<Action>(8);
    }

    //public class Dispatcher : MonoBehaviour
    //{
    //    private static Dispatcher instance;

    //    private List<Action> pending = new List<Action>();

    //    public static Dispatcher Instance
    //    {
    //        get
    //        {
    //            return instance;
    //        }
    //    }

    //    public void Invoke(Action fn)
    //    {
    //        lock (this.pending)
    //        {
    //            this.pending.Add(fn);
    //        }
    //    }

    //    private void InvokePending()
    //    {
    //        lock (this.pending)
    //        {
    //            foreach (Action action in this.pending)
    //            {
    //                action();
    //            }

    //            this.pending.Clear();
    //        }
    //    }

    //    private void Awake()
    //    {
    //        if (instance != null && instance != this)
    //        {
    //            Destroy(this.gameObject);
    //        }
    //        else
    //        {
    //            instance = this;
    //        }
    //    }

    //    private void Update()
    //    {
    //        this.InvokePending();
    //    }
    //}
}
