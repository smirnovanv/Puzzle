using DG.Tweening;
using UnityEngine;
using System;
using System.Collections;

public class SwipeAnimationHandler : MonoBehaviour
{
    private bool _isAnimating = false;

    public event Action OnMoveComplete;

    public void AnimateSwap(BallData2 ball1, BallData2 ball2, Action onComplete)
    {
        if (_isAnimating)
        {
            Debug.Log("Already animating, ignoring...");
            return;
        }

        StartCoroutine(AnimateSwapCoroutine(ball1, ball2, onComplete));
    }

    private IEnumerator AnimateSwapCoroutine(BallData2 ball1, BallData2 ball2, Action onComplete)
    {
        _isAnimating = true;

        if (ball1?.visualObject == null || ball2?.visualObject == null)
        {
            Debug.LogError("Visual objects missing!");
            onComplete?.Invoke();
            _isAnimating = false;
            yield break;
        }

        Vector3 pos1 = ball1.visualObject.transform.position;
        Vector3 pos2 = ball2.visualObject.transform.position;

        // Параллельная анимация обмена
        Sequence swapSequence = DOTween.Sequence();
        swapSequence.Join(ball1.visualObject.transform.DOMove(pos2, 0.3f).SetEase(Ease.InOutQuad));
        swapSequence.Join(ball2.visualObject.transform.DOMove(pos1, 0.3f).SetEase(Ease.InOutQuad));

        yield return swapSequence.WaitForCompletion();

        _isAnimating = false;
        onComplete?.Invoke(); // Вызываем callback ПОСЛЕ анимации
    }
}
