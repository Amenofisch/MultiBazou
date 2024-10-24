using System;
using System.Collections.Generic;

namespace MultiBazou
{
    public static class ThreadManager
    {
        private static readonly List<Action> ToBeExecutedOnMainThread = new();
        private static readonly List<Action> ExecuteCopiedOnMainThread = new();
        private static bool _actionToExecuteOnMainThread;

        /// <summary>Sets an action to be executed on the main thread.</summary>
        /// <param name="action">The action to be executed on the main thread.</param>
        /// <param name="exception"></param>
        public static void ExecuteOnMainThread<T>(Action<T> action, T exception)
        {
            if (action == null)
            {
                Plugin.log.LogError("No action was given to execute on the main thread!");
                return;
            }

            lock (ToBeExecutedOnMainThread)
            {
                ToBeExecutedOnMainThread.Add(() =>
                {
                    try
                    {
                        action(exception);
                    }
                    catch (Exception e)
                    {
                        Plugin.log.LogError("Exception occured on MainThread: " + e);
                    }
                });
                _actionToExecuteOnMainThread = true;
            }
        }

        /// <summary>Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.</summary>
        public static void UpdateThread()
        {
            if (!_actionToExecuteOnMainThread) return;
            
            ExecuteCopiedOnMainThread.Clear();
            lock (ToBeExecutedOnMainThread)
            {
                ExecuteCopiedOnMainThread.AddRange(ToBeExecutedOnMainThread);
                ToBeExecutedOnMainThread.Clear();
                _actionToExecuteOnMainThread = false;
            }

            foreach (var t in ExecuteCopiedOnMainThread)
            {
                t();
            }
        }
    }
}