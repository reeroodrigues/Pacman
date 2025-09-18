using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RestartController : MonoBehaviour
{
    [Header("Modo de reinício")]
    [SerializeField] private bool hardReload = false;

    [Header("UI opcional")]
    [SerializeField] private GameObject gameOverUI;

    private void Awake()
    {
        var btn = GetComponent<Button>();
        if (btn) btn.onClick.AddListener(Restart);
    }

    private void OnDestroy()
    {
        var btn = GetComponent<Button>();
        if (btn) btn.onClick.RemoveListener(Restart);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) Restart();

        #if ENABLE_INPUT_SYSTEM
        var pad = UnityEngine.InputSystem.Gamepad.current;
        if (pad != null)
        {
            if (pad.startButton.isPressed && pad.selectButton.wasPressedThisFrame) Restart();
            if (pad.buttonNorth.wasPressedThisFrame) Restart();
        }
        #endif
    }

    public void Restart()
    {
        Time.timeScale = 1f;

        if (hardReload)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }
        
        if (gameOverUI) gameOverUI.SetActive(false);
        
        foreach (var p in FindObjectsOfType<Pellet>(true))
            p.gameObject.SetActive(true);
        foreach (var pp in FindObjectsOfType<PowerPellet>(true))
            pp.gameObject.SetActive(true);
        
        var pac = FindObjectOfType<Pacman>(true);
        if (pac) pac.ResetState();

        foreach (var g in FindObjectsOfType<Ghost>(true))
            g.ResetState();
        
        var gm = FindObjectOfType<GameManager>(true);
        if (gm != null)
        {
        }
    }
}
