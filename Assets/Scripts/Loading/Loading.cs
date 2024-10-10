using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    [Header("Scrolling Background-----------------------------------")]
    [SerializeField] RawImage background;
    [SerializeField] Vector2 offset;
    [Header("Elements-----------------------------------")]
    [SerializeField] Image fill;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] float duration = 3f;


    void Start()
    {
        LeanTween.moveLocal(text.gameObject, Vector3.zero, 1f).setEase(LeanTweenType.easeOutBack).setDelay(0.5f);
        LeanTween.scale(text.gameObject, Vector3.zero, 1f).setEase(LeanTweenType.easeOutBounce).setDelay(1.5f);

        FakeLoading();
    }

    private async void FakeLoading()
    {
        fill.fillAmount = 0f;

        AsyncOperation loading = SceneManager.LoadSceneAsync(1);
        loading.allowSceneActivation = false;

        float timer = 0f;
        while(timer < duration)
        {
            timer += Time.deltaTime;
            fill.fillAmount = timer / duration;
            await Task.Yield();
        }

        await Task.Delay(100);
        fill.fillAmount = 1;

        loading.allowSceneActivation = true;

    }

    void Update()
    {
        background.uvRect = new Rect(background.uvRect.position + offset, background.uvRect.size);
    }
}
