using System.Threading.Tasks;
using UnityEngine;

namespace RedsUtils.LoadingSystem
{
    public abstract class LoadableBehavior : MonoBehaviour, ILoadable
    {
        public float Progress { get; protected set; } = 0f;
        public bool IsDone { get; protected set; } = false;

        public async Task LoadAsync()
        {
            LoadingEvents.RaiseLoadableStarted(this);
            await DoLoadAsync();
            Progress = 1f;
            IsDone = true;
            LoadingEvents.RaiseLoadableCompleted(this);
        }

        protected abstract Task DoLoadAsync();

        protected void ReportProgress(float progress)
        {
            Progress = Mathf.Clamp01(progress);
            LoadingEvents.RaiseLoadableProgress(this, Progress);
        }
    }
}