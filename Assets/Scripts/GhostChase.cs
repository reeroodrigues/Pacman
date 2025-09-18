using UnityEngine;

public class GhostChase : GhostBehavior
{
    private void OnDisable()
    {
        ghost.Scatter.Enable();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Node node = other.GetComponent<Node>();
        
        if (node != null && enabled && !ghost.Frightened.enabled)
        {
            Vector2 direction = Vector2.zero;
            float minDistance = float.MaxValue;
            
            Vector2 availableDirections = Vector2.zero;
            foreach (Vector2 dir in node.availableDirections)
            {
                if (dir != -ghost.Movement.direction)
                {
                    Vector3 newPosition = transform.position + new Vector3(dir.x, dir.y);
                    float distance = (ghost.target.position - newPosition).sqrMagnitude;

                    if (distance < minDistance)
                    {
                        direction = dir;
                        minDistance = distance;
                    }
                }
            }

            ghost.Movement.SetDirection(direction);
        }
    }
}