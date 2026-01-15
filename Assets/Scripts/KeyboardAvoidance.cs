using TMPro;
using UnityEngine;

public class KeyboardAvoidance : MonoBehaviour
{
        [Header("References")] 
        [SerializeField] private RectTransform formContainer;

        [Header("Settings")]
        [SerializeField] private float moveUpOffset = 200f;
        [SerializeField] private float animationSpeed = 5f;
        [SerializeField] private bool useKeyboardHeight = true;

        [SerializeField] private bool isKeyboardOpen = false;
        private Vector2 _originalPosition;
        private float _targetYPosition;
        private TMP_InputField _currentInputField;

        private void Start()
        {
            if (formContainer == null)
                formContainer = GetComponent<RectTransform>();
        
            _originalPosition = formContainer.anchoredPosition;
            _targetYPosition = _originalPosition.y;
        }

        private void Update()
        {
            CheckKeyboardStatus();
            AnimatePosition();
        }

        private void CheckKeyboardStatus()
        {
            if (TouchScreenKeyboard.visible)
            {
                if (!isKeyboardOpen)
                    OnKeyboardOpened();
            }
            else
            {
                if (isKeyboardOpen)
                    OnKeyboardClosed();
            }
        }

        private void OnKeyboardOpened()
        {
            isKeyboardOpen = true;

            var keyboardHeight = 0f;

            if (useKeyboardHeight && TouchScreenKeyboard.area.height > 0)
            {
                keyboardHeight = TouchScreenKeyboard.area.height;
            }
            else
            {
                keyboardHeight = moveUpOffset;
            }
        
            _targetYPosition = _originalPosition.y + keyboardHeight;
        }

        private void OnKeyboardClosed()
        {
            isKeyboardOpen = false;
            _targetYPosition = _originalPosition.y;
        }

        private void AnimatePosition()
        {
            var currentPos = formContainer.anchoredPosition;
            var newY = Mathf.Lerp(currentPos.y, _targetYPosition, Time.deltaTime * animationSpeed);
            formContainer.anchoredPosition = new Vector2(currentPos.x, newY);
        }

        public void RegisterInputField(TMP_InputField inputField)
        {
            if (inputField != null)
            {
                inputField.onSelect.AddListener((text) => OnInputFieldSelected(inputField));
                inputField.onDeselect.AddListener((text) => OnInputFieldDeselected());
            }
        }

        private void OnInputFieldSelected(TMP_InputField inputField)
        {
            _currentInputField = inputField;
        }
    
        private void OnInputFieldDeselected()
        {
            _currentInputField = null;
        }
}