using System;

public static class GameEvents
{
    public static event Action PelletEaten;
    public static event Action PacmanDied;
    public static event Action AllPelletsCollected;
    
    public static void RaisePelletEaten() =>  PelletEaten?.Invoke();
    public static void RaisePacmanDied() => PacmanDied?.Invoke();
    public static void RaiseAllPelletsCollected() => AllPelletsCollected?.Invoke();
}