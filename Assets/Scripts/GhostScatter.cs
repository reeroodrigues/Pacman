using UnityEngine;

public class GhostScatter : GhostBehavior
{
    private void OnDisable()
    {
        ghost.Chase.Enable();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Node node = other.GetComponent<Node>();
        
        if (node != null && enabled && !ghost.Frightened.enabled)
        {
            int index = Random.Range(0, node.availableDirections.Count);
            
            if (node.availableDirections.Count > 1 && node.availableDirections[index] == -ghost.Movement.direction)
            {
                index++;
                
                if (index >= node.availableDirections.Count) {
                    index = 0;
                }
            }

            ghost.Movement.SetDirection(node.availableDirections[index]);
        }
    }

}
