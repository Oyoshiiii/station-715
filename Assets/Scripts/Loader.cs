using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader  
{
    public enum Scene
    {
        MainMenuScene,
        StartRoomScene,
        LoadingScene,
        MainCoridorScene,
        PantryScene
    }

    private static Scene targetScene;

    public static void Load(Scene targetScene)
    {
        Loader.targetScene = targetScene;
        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }

    public static void LoaderCallback()
    {
        Debug.Log("Loading scene: " + targetScene.ToString());
        SceneManager.LoadScene(targetScene.ToString());
    }
}
