using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Pellet : MonoBehaviour
{
    [SerializeField] private SoundEvent sfxPickup;
    
    public int points = 10;

    protected virtual void Eat()
    {
        GameEvents.RaisePelletEaten();
      //  PelletWakaController.I?.NotifyPelletEaten();
        GameManager.Instance.PelletEaten(this);
        if (AudioManager.I != null && sfxPickup) AudioManager.I.Play2D(sfxPickup);
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Pacman"))
        {
            Eat();
        }
    }
}