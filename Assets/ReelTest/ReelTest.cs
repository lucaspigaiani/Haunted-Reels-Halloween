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
    [SerializeField] private float startSpeed = 400f;
    [SerializeField] private float stopSpeed = 20f;

    private bool isSpinning;

    private const float TOP_POSITION = 340f;
    private const float BOTTOM_POSITION = -340f;
    private const float SPACING = 170f;

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
            MoveReels(startSpeed);

            timer += Time.deltaTime;
            yield return null;
        }

        float currentSpeed = startSpeed;

        while (currentSpeed > stopSpeed)
        {
            currentSpeed = Mathf.Lerp(
                currentSpeed,
                0f,
                Time.deltaTime * 2f);

            MoveReels(currentSpeed);

            yield return null;
        }

        yield return StartCoroutine(AlignRoutine());

        isSpinning = false;
    }

    private IEnumerator AlignRoutine()
    {
        while (true)
        {
            MoveReels(stopSpeed);

            float topY = reelImages[0].rectTransform.anchoredPosition.y;

            float remainder =
                Mathf.Abs((topY - TOP_POSITION) % SPACING);

            if (remainder < 3f || remainder > SPACING - 3f)
            {
                AlignPerfectly();
                yield break;
            }

            yield return null;
        }
    }

    private void MoveReels(float speed)
    {
        foreach (Image image in reelImages)
        {
            RectTransform rt = image.rectTransform;

            rt.anchoredPosition += Vector2.down * speed * Time.deltaTime;

            if (rt.anchoredPosition.y < BOTTOM_POSITION)
            {
                rt.anchoredPosition += Vector2.up * (SPACING * reelImages.Length);

                image.sprite = GetRandomSymbol();
            }
        }
    }

    private void AlignPerfectly()
    {
        float offset =
            reelImages[0].rectTransform.anchoredPosition.y - TOP_POSITION;

        foreach (Image image in reelImages)
        {
            RectTransform rt = image.rectTransform;

            rt.anchoredPosition = new Vector2(
                rt.anchoredPosition.x,
                rt.anchoredPosition.y - offset);
        }
    }

    private Sprite GetRandomSymbol()
    {
        return symbols[Random.Range(0, symbols.Length)];
    }
}