using UnityEngine;

public class ExitButtonProxy : MonoBehaviour
{
    public void OpenExitPanel()
    {
        if (ExitUIManager.Instance != null)
            ExitUIManager.Instance.Open();
    }

    public void ConfirmYes()
    {
        if (ExitUIManager.Instance != null)
            ExitUIManager.Instance.ConfirmYes();
    }

    public void ConfirmNo()
    {
        if (ExitUIManager.Instance != null)
            ExitUIManager.Instance.ConfirmNo();
    }
}