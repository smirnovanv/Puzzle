using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// [SerializeField] private float baseFallDuration = 0.1f;
// [SerializeField] private float durationPerCell = 0.08f;
//[SerializeField] private float maxFallDuration = 0.6f;
//[SerializeField] private float bounceHeight = 0.1f;
//[SerializeField] private float bounceDuration = 0.1f;
//[SerializeField] private float maxRandomDelay = 0.07f;
//[SerializeField] private float swingAmount = 0.03f;


public class ShiftAnimationHandler : MonoBehaviour
{

    public void AnimateShift(VirtualBoard board, List<BallShiftData> ballMovements, int waitTime, Action onComplete)
    {
        if (ballMovements == null || ballMovements.Count == 0)
        {
            Debug.Log("No movements to animate");
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(AnimateShiftCoroutine(board, ballMovements, waitTime, onComplete));
    }

    private IEnumerator AnimateShiftCoroutine(VirtualBoard board, List<BallShiftData> ballMovements, int waitTime, Action onComplete)
    {
        Debug.Log($"=== АНИМАЦИЯ СДВИГА: {ballMovements.Count} шариков ===");
        yield return new WaitForSeconds(waitTime * 0.2f);
        DG.Tweening.Sequence swapSequence = DOTween.Sequence();

        foreach (BallShiftData movement in ballMovements)
        {
            GameObject visualObject = board._gameBoard[movement.targetX, movement.targetY].ball.visualObject;

            Vector3 targetPosition = new Vector3(movement.targetX, movement.targetY, 0);

            float duration = CalculateFallDuration(movement.distance);
            swapSequence.Join(visualObject.transform.DOMove(targetPosition, duration).SetEase(Ease.InOutQuad));

        }

            // Ждём завершения всех анимаций
            yield return swapSequence.WaitForCompletion();

        Debug.Log($"=== СДВИГ ЗАВЕРШЕН ===");
        onComplete?.Invoke();

    }

    private float CalculateFallDuration(int distance)
    {
        // Базовое время + дополнительное время за каждую клетку
        return 0.1f + (distance * 0.08f);
    }
}
