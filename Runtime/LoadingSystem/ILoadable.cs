using System.Threading.Tasks;

namespace RedsUtils.LoadingSystem
{
    public interface ILoadable
    {
        Task LoadAsync();   // Starts loading and resolves when complete
        float Progress { get; }   // Current progress 0–1
        bool IsDone { get; }      // Whether finished
    }
}