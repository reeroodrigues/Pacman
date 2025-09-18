using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(Rigidbody2D))]
public class AntiStuckHandler : MonoBehaviour
{
    [SerializeField] private float stuckTime = 2f;
    [SerializeField] private float epsilon = 0.1f; 

    private Movement movement;
    private Rigidbody2D rb;
    private float stuckTimer = 0f;
    private Vector2 lastPosition;

    private void Awake()
    {
        movement = GetComponent<Movement>();
        rb = GetComponent<Rigidbody2D>();
        lastPosition = rb.position;
    }

    private void FixedUpdate()
    {
        Vector2 currentPosition = rb.position;
        float movementDelta = (currentPosition - lastPosition).magnitude;

        if (movementDelta < epsilon)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer > stuckTime)
            {
                Vector2 newDirection = GetRandomAlternativeDirection(movement.direction);
                if (newDirection != Vector2.zero && newDirection != movement.direction)
                {
                    movement.SetDirection(newDirection, true); 
                  //  DebugStuckInfo(movement.direction, newDirection); // Depuração
                    stuckTimer = 0f; 
                }
            }
        }
        else
        {
            stuckTimer = 0f; 
            lastPosition = currentPosition;
        }
    }

    private Vector2 GetRandomAlternativeDirection(Vector2 currentDirection)
    {
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        System.Random rand = new System.Random();

        var validDirections = directions
            .Where(d => d != currentDirection && d != -currentDirection)
            .ToArray();

        if (validDirections.Length == 0)
        {
            Debug.LogError("Nenhuma direção válida encontrada! Verifique a direção atual: " + currentDirection);
            return Vector2.zero; 
        }

      
        int index = rand.Next(validDirections.Length);
        return validDirections[index];
    }


    private void DebugStuckInfo(Vector2 originalDirection, Vector2 newDirection)
    {
        Debug.LogWarning("Fantasma identificado como travado por mais de " + stuckTime + "s!");
        Debug.LogWarning("Direção original: " + originalDirection);
        Debug.LogWarning("Nova direção após mudar: " + newDirection);
    }
}