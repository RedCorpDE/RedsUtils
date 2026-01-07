using System;

namespace RedsUtils.LoadingSystem
{
    public static class LoadingEvents
    {
        public static event Action OnSceneLoadStarted;
        public static event Action OnAllLoadablesComplete;
        public static event Action OnSceneLoadComplete;
        public static event Action<ILoadable> OnLoadableStarted;
        public static event Action<ILoadable> OnLoadableCompleted;
        public static event Action<ILoadable, float> OnLoadableProgress;
        public static event Action<float> OnProgressChanged; // global blended

        public static void RaiseSceneLoadStarted() => OnSceneLoadStarted?.Invoke();
        public static void RaiseAllLoadablesComplete() => OnAllLoadablesComplete?.Invoke();
        public static void RaiseSceneLoadComplete() => OnSceneLoadComplete?.Invoke();
        public static void RaiseLoadableStarted(ILoadable loadable) => OnLoadableStarted?.Invoke(loadable);
        public static void RaiseLoadableCompleted(ILoadable loadable) => OnLoadableCompleted?.Invoke(loadable);
        public static void RaiseLoadableProgress(ILoadable loadable, float progress) => OnLoadableProgress?.Invoke(loadable, progress);
        public static void RaiseProgressChanged(float progress) => OnProgressChanged?.Invoke(progress);
    }
}