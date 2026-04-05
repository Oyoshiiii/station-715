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

    private static Scene targetScene = Scene.StartRoomScene;
    private static Scene lastScene;

    public static void Load(Scene targetScene)
    {
        Loader.lastScene = Loader.targetScene;
        Loader.targetScene = targetScene;

        SceneManager.LoadScene(Scene.LoadingScene.ToString());

        Debug.Log(lastScene.ToString());
        Debug.Log(targetScene.ToString());
    }

    public static void LoaderCallback()
    {
        Debug.Log("Loading scene: " + targetScene.ToString());
        SceneManager.LoadScene(targetScene.ToString());
    }

    public static Door.DoorName GetPlayerPointDoor()
    {
        Door.DoorName doorIndexName = Door.DoorName.Default;

        if(lastScene is Scene.StartRoomScene && targetScene is Scene.MainCoridorScene)
        {
            doorIndexName = Door.DoorName.FromStartToCoridor;
        }

        else if(lastScene is Scene.MainCoridorScene && targetScene is Scene.StartRoomScene)
        {
            doorIndexName = Door.DoorName.FromCoridorToStart;
        }

        else if(lastScene is Scene.MainCoridorScene && targetScene is Scene.PantryScene)
        {
            doorIndexName = Door.DoorName.FromCoridorToPantry;
        }

        else if(lastScene is Scene.PantryScene && targetScene is Scene.MainCoridorScene)
        {
            doorIndexName = Door.DoorName.FromPantryToCoridor;
        }

        return doorIndexName;
    }
}
