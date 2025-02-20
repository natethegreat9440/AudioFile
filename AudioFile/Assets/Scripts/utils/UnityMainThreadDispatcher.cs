﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace AudioFile.Utilities
{
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static readonly Queue<Action> _executionQueue = new Queue<Action>();
        private static UnityMainThreadDispatcher _instance;

        public static UnityMainThreadDispatcher Instance()
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("UnityMainThreadDispatcher");
                _instance = obj.AddComponent<UnityMainThreadDispatcher>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }

        public void Enqueue(Action action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }

        void Update()
        {
            while (_executionQueue.Count > 0)
            {
                Action action;
                lock (_executionQueue)
                {
                    action = _executionQueue.Dequeue();
                }
                action?.Invoke();
            }
        }
    }
}

