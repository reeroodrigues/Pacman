using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public static class VictoryPayload
{
    public static string Message;
    public static Sprite PrizeSprite;
    public static string PrizeItemId;
    public static string MenuSceneName = "MainMenu";
    public static int Score;
    public static bool IsZeroPoints { get; set; } = false;

    public static void Clear()
    {
        Message = null;
        PrizeSprite = null;
        PrizeItemId = null;
        MenuSceneName = null;
        IsZeroPoints = false;
        Score = 0;
    }
}