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

    public static void Initialize()
    {
        if(Player.Instance != null)
        {
            Player.Instance.OnDoorOpened += Player_OnDoorPassed;
        }
    }

    private static void Player_OnDoorPassed(object sender, System.EventArgs e)
    {
        Door door = (sender as Player)?.GetSelectedDoor();
        if (door != null)
        {
            Load(door.GetScene());
        }
    }

    public static void Load(Scene targetScene)
    {
        Loader.targetScene = targetScene;
        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }

    public static void LoaderCallback()
    {
        Debug.Log("loading scene: " + targetScene.ToString());
        SceneManager.LoadScene(targetScene.ToString());
    }
}
