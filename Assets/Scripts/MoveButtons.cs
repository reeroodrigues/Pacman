using System.Collections;
using UnityEngine;

public class MoveButtons : MonoBehaviour
{
    public enum Side { Left, Right, RightVictory }
    public Side buttonSide = Side.Left;

    public float speed = 400f; 

    private RectTransform rect;
    private Vector2 targetPosition;

    void Start()
    {
        rect = GetComponent<RectTransform>();

        float currentY = rect.anchoredPosition.y;

        switch (buttonSide)
        {
            case Side.Left:
                targetPosition = new Vector2(37f, currentY);
                break;
            case Side.Right:
                targetPosition = new Vector2(-41f, currentY);
                break;
            case Side.RightVictory:
                targetPosition = new Vector2(-106f, currentY);
                break;
        }

        StartCoroutine(MoveToTarget());
    }

    IEnumerator MoveToTarget()
    {
        yield return new WaitForSeconds(0.5f);

        while (Vector2.Distance(rect.anchoredPosition, targetPosition) > 0.1f)
        {
            rect.anchoredPosition = Vector2.MoveTowards(
                rect.anchoredPosition,
                targetPosition,
                speed * Time.deltaTime
            );
            yield return null;
        }

        rect.anchoredPosition = targetPosition;
    }
}
