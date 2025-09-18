using System.Collections.Generic;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class VirtualDpad : MonoBehaviour
{
    public Vector2 Direction => _currentVector;
    public Vector2 direction => _currentVector;
    
    private Vector2 _currentVector = Vector2.zero;

    private readonly List<DPadButton.Dir> _pressed = new List<DPadButton.Dir>(2);

    public void Press(DPadButton.Dir dir)
    {
        if (!_pressed.Contains(dir))
            _pressed.Add(dir);
        UpdateCurrent();
    }

    public void Release(DPadButton.Dir dir)
    {
        _pressed.Remove(dir);
        UpdateCurrent();
    }

    private void UpdateCurrent()
    {
        if (_pressed.Count == 0)
        {
            _currentVector = Vector2.zero;
            return;
        }
        
        var last = _pressed[_pressed.Count - 1];
        switch (last)
        {
            case DPadButton.Dir.Up: _currentVector = Vector2.up; break;
            case DPadButton.Dir.Down: _currentVector = Vector2.down; break;
            case DPadButton.Dir.Left: _currentVector = Vector2.left; break;
            case DPadButton.Dir.Right: _currentVector = Vector2.right; break;
        }
    }
}
