using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace RedsUtils.LoadingSystem
{

public class SceneLoadingManager : MonoBehaviour
{
    private const string LoadingSceneName = "SCN_LoadingScreen";
    private static List<ILoadable> loadables = new();
    private static bool isRunning = false;

    public static async void SwitchScene(string sceneName)
    {

        if (isRunning)
            return;

        await HandleSwitchScene(sceneName);

    }

    static async Task HandleSwitchScene(string sceneName)
    {
        var previousScene = SceneManager.GetActiveScene();
        
        var loadingScene = SceneManager.GetSceneByName(LoadingSceneName);

        if (!loadingScene.IsValid() || !loadingScene.isLoaded)
        {
            var loadLoadingOp = SceneManager.LoadSceneAsync(LoadingSceneName, LoadSceneMode.Additive);
            if (loadLoadingOp == null)
                throw new ArgumentException(
                    $"Could not start loading scene '{LoadingSceneName}'. Is it added to Build Settings and spelled correctly?"
                );

            await loadLoadingOp;
            
            do
            {
                await Task.Yield();
                loadingScene = SceneManager.GetSceneByName(LoadingSceneName);
            }
            while (!loadingScene.IsValid() || !loadingScene.isLoaded);
        }
        
        isRunning = true;
        
        SceneManager.SetActiveScene(loadingScene);

        if (previousScene.IsValid() && previousScene.isLoaded && previousScene.name != LoadingSceneName)
            await SceneManager.UnloadSceneAsync(previousScene);

        await LoadSceneAsync(sceneName);
    }


    private static void FinishLoading()
    {
        LoadingEvents.RaiseProgressChanged(1f);
        LoadingEvents.RaiseSceneLoadComplete();
        isRunning = false;
    }

    private static async Task LoadSceneAsync(string sceneName)
    {
        LoadingEvents.RaiseSceneLoadStarted();

        await Task.Delay(2000);

        // --- Step 1: Load Scene ---
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (op == null)
            throw new ArgumentException($"Could not start loading scene '{sceneName}'. Is it added to Build Settings?");

        op.allowSceneActivation = true;

        while (!op.isDone)
        {
            float sceneProgress = Mathf.Clamp01(op.progress / 0.9f);
            float blended = sceneProgress * 0.5f; // 0–50%
            LoadingEvents.RaiseProgressChanged(blended);
            await Task.Yield();
        }

        // Ensure SceneManager reports it as loaded before setting active
        var newScene = SceneManager.GetSceneByName(sceneName);
        while (!newScene.IsValid() || !newScene.isLoaded)
        {
            await Task.Yield();
            newScene = SceneManager.GetSceneByName(sceneName);
        }

        SceneManager.SetActiveScene(newScene);

        SceneManager.SetActiveScene(newScene);

        // --- Step 2: Collect loadables (recommended: only from the new scene) ---
        loadables = FindObjectsOfType<MonoBehaviour>(true)
            .OfType<ILoadable>()
            .Where(l => (l as MonoBehaviour).gameObject.scene == newScene) // avoids picking up loadables in loading screen
            .ToList();

        var loadTasks = loadables.Select(l => l.LoadAsync()).ToList();

        // --- Step 3: Track progress while tasks run ---
        while (loadTasks.Any(t => !t.IsCompleted))
        {
            float avgProgress = loadables.Count > 0 ? loadables.Average(l => l.Progress) : 1f;
            float blended = 0.5f + avgProgress * 0.5f; // 50–100%
            LoadingEvents.RaiseProgressChanged(blended);

            await Task.Yield();
        }

        await Task.WhenAll(loadTasks);

        LoadingEvents.RaiseAllLoadablesComplete();

        await SceneManager.UnloadSceneAsync(LoadingSceneName);

        await Task.Delay(2000);

        FinishLoading();
    }
}

    
}