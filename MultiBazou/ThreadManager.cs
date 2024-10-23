using System;
using System.Collections.Generic;
using UnityEngine;

namespace MultiBazou
{
    public class ThreadManager
    {
        private static readonly List<Action> executeOnMainThread = new List<Action>();
        private static readonly List<Action> executeCopiedOnMainThread = new List<Action>();
        private static bool actionToExecuteOnMainThread = false;

        public static void UpdateThread()
        {
            UpdateMain();
        }

        /// <summary>Sets an action to be executed on the main thread.</summary>
        /// <param name="_action">The action to be executed on the main thread.</param>
        public static void ExecuteOnMainThread<T>(Action<T> _action, T exception)
        {
            if (_action == null)
            {
                Debug.Log("No action was given to execute on the main thread!");
                return;
            }

            lock (executeOnMainThread)
            {
                executeOnMainThread.Add(() =>
                {
                    try
                    {
                        _action(exception);
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Exception occured on MainThread: " + e);
                    }
                });
                actionToExecuteOnMainThread = true;
            }
        }

        /// <summary>Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.</summary>
        public static void UpdateMain()
        {
            if (actionToExecuteOnMainThread)
            {
                executeCopiedOnMainThread.Clear();
                lock (executeOnMainThread)
                {
                    executeCopiedOnMainThread.AddRange(executeOnMainThread);
                    executeOnMainThread.Clear();
                    actionToExecuteOnMainThread = false;
                }

                for (int i = 0; i < executeCopiedOnMainThread.Count; i++)
                {
                    executeCopiedOnMainThread[i]();
                }
            }
        }
    }
}