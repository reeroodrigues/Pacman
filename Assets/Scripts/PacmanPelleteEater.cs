using UnityEngine;
using UnityEngine.Tilemaps;

public class PacmanPelletEater : MonoBehaviour
{
    [SerializeField] private int scorePerPellet = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Pellet"))
        {
            Destroy(other.gameObject);
            Debug.Log("Pellet comido! +" + scorePerPellet + " pontos");
        }
    }
}