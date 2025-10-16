using UnityEngine;

public static class Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Run()
    {
        var kernelGo = new GameObject("Kernel");
        Object.DontDestroyOnLoad(kernelGo);
        var kernel = kernelGo.AddComponent<Kernel>();
        kernel.Initialize();
    }
}
