using System.Collections;
using UnityEngine;

public class MoveButtons : MonoBehaviour
{
    public enum Side { Left, Right, RightVictory }
    public Side buttonSide = Side.Left;

    public float speed = 400f; 

    private RectTransform _rect;
    private Vector2 _targetPosition;

    void Start()
    {
        _rect = GetComponent<RectTransform>();

        var currentY = _rect.anchoredPosition.y;

        _targetPosition = buttonSide switch
        {
            Side.Left => new Vector2(37f, currentY),
            Side.Right => new Vector2(-41f, currentY),
            Side.RightVictory => new Vector2(-215.1f, currentY),
            _ => _targetPosition
        };

        StartCoroutine(MoveToTarget());
    }

    IEnumerator MoveToTarget()
    {
        yield return new WaitForSeconds(0.5f);

        while (Vector2.Distance(_rect.anchoredPosition, _targetPosition) > 0.1f)
        {
            _rect.anchoredPosition = Vector2.MoveTowards(
                _rect.anchoredPosition,
                _targetPosition,
                speed * Time.deltaTime
            );
            yield return null;
        }

        _rect.anchoredPosition = _targetPosition;
    }
}
