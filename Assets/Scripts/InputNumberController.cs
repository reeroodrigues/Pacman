using UnityEngine;
using TMPro;

public class InputNumberController : MonoBehaviour
{
    [SerializeField] private TMP_InputField[] inputFields;
    private int currentValue = 0;
    private bool isHoldingUp;
    private bool isHoldingDown;
    private int currentInputIndex = -1;

    [SerializeField] private float holdDelay = 1.0f;
    [SerializeField] private float repeatRate = 1.0f;

    private float nextRepeatTime;
    private float lastPressTime = 0f;
    [SerializeField] private float pressDebounce = 0.05f;

    public void OnUpButtonPressed(int inputIndex)
    {
        if (!IsValidIndex(inputIndex)) return;
        if (isHoldingUp && currentInputIndex == inputIndex) return;
        if (Time.time - lastPressTime < pressDebounce) return;

        lastPressTime = Time.time;
        currentInputIndex = inputIndex;
        isHoldingUp = true;

        currentValue = GetCurrentValue();
        currentValue++;
        UpdateInputValue();

        nextRepeatTime = Time.time + holdDelay;
    }

    public void OnUpButtonReleased(int inputIndex)
    {
        if (inputIndex == currentInputIndex)
        {
            isHoldingUp = false;
            currentInputIndex = -1;
        }
    }

    public void OnDownButtonPressed(int inputIndex)
    {
        if (!IsValidIndex(inputIndex)) return;
        if (isHoldingDown && currentInputIndex == inputIndex) return;
        if (Time.time - lastPressTime < pressDebounce) return;

        lastPressTime = Time.time;
        currentInputIndex = inputIndex;
        isHoldingDown = true;

        currentValue = GetCurrentValue();
        currentValue--;
        UpdateInputValue();

        nextRepeatTime = Time.time + holdDelay;
    }

    public void OnDownButtonReleased(int inputIndex)
    {
        if (inputIndex == currentInputIndex)
        {
            isHoldingDown = false;
            currentInputIndex = -1;
        }
    }

    private void Update()
    {
        if (!IsValidIndex(currentInputIndex)) return;

        if (isHoldingUp && Time.time >= nextRepeatTime)
        {
            currentValue++;
            UpdateInputValue();
            nextRepeatTime = Time.time + repeatRate;
        }
        else if (isHoldingDown && Time.time >= nextRepeatTime)
        {
            currentValue--;
            UpdateInputValue();
            nextRepeatTime = Time.time + repeatRate;
        }
    }

    private int GetCurrentValue()
    {
        string text = inputFields[currentInputIndex].text;
        return string.IsNullOrEmpty(text) ? 0 : int.Parse(text);
    }

    private void UpdateInputValue()
    {
        if (IsValidIndex(currentInputIndex))
        {
            inputFields[currentInputIndex].text = currentValue.ToString();
        }
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < inputFields.Length && inputFields[index] != null;
    }
}
