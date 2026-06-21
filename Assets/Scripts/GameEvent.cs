using System;

public static class GameEvent
{
    public static Action EnemyKilled;
    public static Action HitPlayer;
    public static Action<int> OnHpChanged;
}
