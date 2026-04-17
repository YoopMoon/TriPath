using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CharacterSelectedCardBlink : MonoBehaviour
{
    [Header("Blink Settings")]
    [SerializeField] private float blinkInterval = 0.5f;
    [SerializeField] private float visibleAlpha = 1f;
    [SerializeField] private float hiddenAlpha = 0.25f;

    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        blinkCoroutine = StartCoroutine(BlinkCoroutine());
    }

    private void OnDisable()
    {
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        SetAlpha(visibleAlpha);
    }

    private IEnumerator BlinkCoroutine()
    {
        while (true)
        {
            SetAlpha(visibleAlpha);
            yield return new WaitForSeconds(blinkInterval);

            SetAlpha(hiddenAlpha);
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private void SetAlpha(float alpha)
    {
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}