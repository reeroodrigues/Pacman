using UnityEngine;

public static class GhostSpeedSettings
{
    private const string PrefKey = "GhostBaseSpeed";
    public static int _speed = int.MinValue;

    public static int Speed
    {
        get
        {
            if (_speed == int.MinValue)
            {
                if(PlayerPrefs.HasKey(PrefKey))
                    _speed = Mathf.Clamp(PlayerPrefs.GetInt(PrefKey,3), 0, 10);
                else
                    _speed = 3;
            }
            return _speed;
        }
        set
        {
            _speed = Mathf.Clamp(value, 0, 10);
            PlayerPrefs.SetInt(PrefKey,_speed);
            PlayerPrefs.Save();
        }
    }

    public static void EnsureDefault(int fallback)
    {
        if (!PlayerPrefs.HasKey(PrefKey))
            _speed = Mathf.Clamp(fallback, 0, 10);
    }
}