using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    Image image;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    public IEnumerator FadeIn(float time)
    {
        yield return image.DOFade(1f, time).WaitForCompletion();
    }

    public IEnumerator FadeOut(float time)
    {
        yield return image.DOFade(0f, time).WaitForCompletion();
    }
}
