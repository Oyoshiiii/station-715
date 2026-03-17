using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private float fadeSpeed = 1f;

    private Color color;

    private void Start()
    {
        color = background.color;
        StartCoroutine(FadeOut());
    }

    public IEnumerator FadeIn()
    {
        while (color.a < 1f)
        {
            color.a += fadeSpeed * Time.deltaTime;
            background.color = color;

            yield return null;
        }
    }

    public IEnumerator FadeOut()
    {
        while (color.a > 0f)
        {
            color.a -= fadeSpeed * Time.deltaTime;
            background.color = color;

            yield return null;
        }
    }
}