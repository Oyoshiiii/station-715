using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playBtn;
    [SerializeField] private Button quitBtn;

    [SerializeField] private Fader fader;

    private void Awake()
    {
        playBtn.onClick.AddListener(() =>
        {
            StartCoroutine(StartGame());
        });

        quitBtn.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }

    private System.Collections.IEnumerator StartGame()
    {
        if (fader != null)
        {
            yield return StartCoroutine(fader.FadeIn());
        }

        Loader.Load(Loader.Scene.StartRoomScene);
    }
}
