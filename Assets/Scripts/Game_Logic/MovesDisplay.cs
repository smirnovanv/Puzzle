using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MovesDisplay : MonoBehaviour
{
    private TextMeshProUGUI movesText;
    private GameObject textObject;

    public void Initialize(GameObject textPrefab, int totalMoves)
    {
        // Находим или создаём Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("MainCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Создаём текст из префаба
        if (textPrefab != null)
        {
            textObject = Instantiate(textPrefab, canvas.transform);
            movesText = textObject.GetComponent<TextMeshProUGUI>();

            if (movesText != null)
            {
                // Настраиваем позицию
                RectTransform rect = textObject.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 1);
                rect.anchorMax = new Vector2(0.5f, 1);
                rect.pivot = new Vector2(0.5f, 1);
                rect.anchoredPosition = new Vector2(0, -20);
                rect.sizeDelta = new Vector2(400, 100);
            }
        }
        else
        {
            // Если префаб не назначен, создаём текст программно
            CreateDefaultText(canvas.transform);
        }
    }

    private void CreateDefaultText(Transform parent)
    {
        textObject = new GameObject("MovesText");
        textObject.transform.SetParent(parent);

        movesText = textObject.AddComponent<TextMeshProUGUI>();
        movesText.fontSize = 42;
        movesText.fontStyle = FontStyles.Bold;
        movesText.alignment = TextAlignmentOptions.Center;
        movesText.color = Color.white;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1);
        rect.anchorMax = new Vector2(0.5f, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = new Vector2(0, -50);
        rect.sizeDelta = new Vector2(400, 100);
    }

    public void UpdateMoves(int currentMoves, int maxMoves)
    {
        if (movesText != null)
        {
            movesText.text = $"Ходы: \n{currentMoves}/{maxMoves}";
        }
    }

    private void OnDestroy()
    {
        if (textObject != null)
        {
            Destroy(textObject);
        }
    }
}