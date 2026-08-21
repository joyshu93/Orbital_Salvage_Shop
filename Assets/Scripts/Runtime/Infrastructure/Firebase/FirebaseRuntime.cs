using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Extensions;

namespace CurioClerk.Infrastructure.Firebase
{
    public static class FirebaseRuntime
    {
        private static readonly object SyncRoot = new object();
        private static Task<bool> _dependencyTask;

        public static Task<bool> DependencyTask
        {
            get
            {
                lock (SyncRoot)
                {
                    if (_dependencyTask == null)
                    {
                        _dependencyTask = CheckDependenciesOnce();
                    }

                    return _dependencyTask;
                }
            }
        }

        private static Task<bool> CheckDependenciesOnce()
        {
            try
            {
                return FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsCanceled || task.IsFaulted)
                    {
                        return false;
                    }

                    return task.Result == DependencyStatus.Available;
                });
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }
    }
}
