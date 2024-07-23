using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hexagon : MonoBehaviour
{
    [SerializeField] Renderer renderer;
    [SerializeField] Collider collider;

    public HexagonStack HexagonStack;

    public Color Color
    {
        get => renderer.material.color;
        set => renderer.material.color = value;
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void DisableCollider() => collider.enabled = false;

    public void MoveToLocal(Vector3 targetLocalPos, float delay)
    {
        LeanTween.cancel(gameObject);

        LeanTween.moveLocal(gameObject, targetLocalPos, 0.3f)
            .setEase(LeanTweenType.easeInOutSine)
            .setDelay(delay);

        Vector3 direction = (targetLocalPos - transform.localPosition).With(y: 0).normalized;
        Vector3 rotationAxis = Vector3.Cross(Vector3.up, direction);
        
        LeanTween.rotateAround(gameObject, rotationAxis, 180, 0.3f)
            .setEase(LeanTweenType.easeInOutSine)
            .setDelay(delay);
    }

    public void Vanish(float delay)
    {
        LeanTween.cancel(gameObject);

        LeanTween.scale(gameObject, Vector3.zero, 0.2f)
            .setEase(LeanTweenType.easeInBack)
            .setDelay(delay)
            .setOnComplete(() => {
                transform.localScale = Vector3.one;
                this.collider.enabled = true;
                ObjectPooler.EnqueueObject(KeySave.hexagon, gameObject.GetComponent<Hexagon>());
            });
    }
}
