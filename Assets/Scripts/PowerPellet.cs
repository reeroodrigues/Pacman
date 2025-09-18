using UnityEngine;
using UnityEngine.SceneManagement;

public class PowerPellet : Pellet
{
    public float duration = 8f;

    protected override void Eat()
    {
        GameManager.Instance.PowerPelletEaten(this);
    }

    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (transform.childCount >= 2)
        {
            if (sceneName == "Pacman")
            {
                transform.GetChild(0).gameObject.SetActive(false);
                transform.GetChild(1).gameObject.SetActive(false);
            }
            else if (sceneName == "Pacman - Second Phase")
            {
                transform.GetChild(0).gameObject.SetActive(false);
                transform.GetChild(1).gameObject.SetActive(false);
            }
        }
    }
}
