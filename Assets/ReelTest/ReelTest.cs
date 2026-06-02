using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ReelTest : MonoBehaviour
{
    [Header("Top -> Bottom")]
    [SerializeField] private Image[] reelImages;

    [Header("Available Symbols")]
    [SerializeField] private Sprite[] symbols;

    [Header("Spin")]
    [SerializeField] private float spinDuration = 3f;
    [SerializeField] private float spinSpeed = 400f;

    [Header("Stop")]
    [SerializeField] private float alignSpeed = 300f;

    [Header("Layout")]
    [SerializeField] private float symbolSpacing = 170f;

    private bool isSpinning;

    private readonly float[] validPositions =
    {
        340f,
        170f,
        0f,
        -170f
    };

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isSpinning)
        {
            StartCoroutine(SpinRoutine());
        }
    }

    private IEnumerator SpinRoutine()
    {
        isSpinning = true;

        float timer = 0f;

        while (timer < spinDuration)
        {
            MoveReels();

            timer += Time.deltaTime;
            yield return null;
        }

        yield return StartCoroutine(AlignToGrid());

        isSpinning = false;
    }

    private void MoveReels()
    {
        foreach (Image image in reelImages)
        {
            image.rectTransform.anchoredPosition +=
                Vector2.down * spinSpeed * Time.deltaTime;
        }

        Image bottomImage = reelImages[reelImages.Length - 1];
        RectTransform bottomRect = bottomImage.rectTransform;

        if (bottomRect.anchoredPosition.y < -255f)
        {
            RectTransform topRect = reelImages[0].rectTransform;

            bottomRect.anchoredPosition = new Vector2(
                bottomRect.anchoredPosition.x,
                topRect.anchoredPosition.y + symbolSpacing);

            bottomImage.sprite = GetRandomSymbol();

            RotateArray();
        }
    }

    private IEnumerator AlignToGrid()
    {
        bool aligned = false;

        while (!aligned)
        {
            aligned = true;

            for (int i = 0; i < reelImages.Length; i++)
            {
                RectTransform rt = reelImages[i].rectTransform;

                float targetY = validPositions[i];

                float newY = Mathf.MoveTowards(
                    rt.anchoredPosition.y,
                    targetY,
                    alignSpeed * Time.deltaTime);

                rt.anchoredPosition = new Vector2(
                    rt.anchoredPosition.x,
                    newY);

                if (Mathf.Abs(newY - targetY) > 0.01f)
                {
                    aligned = false;
                }
            }

            yield return null;
        }
    }

    private void RotateArray()
    {
        Image last = reelImages[reelImages.Length - 1];

        for (int i = reelImages.Length - 1; i > 0; i--)
        {
            reelImages[i] = reelImages[i - 1];
        }

        reelImages[0] = last;
    }

    private Sprite GetRandomSymbol()
    {
        return symbols[Random.Range(0, symbols.Length)];
    }
}