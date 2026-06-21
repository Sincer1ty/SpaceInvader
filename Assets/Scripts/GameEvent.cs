using System;

public static class GameEvent // 게임 내 이벤트 모음
{
    public static Action EnemyKilled;
    public static Action HitPlayer;
    
    public static Action<int> OnHpChanged;
    public static Action PlayerDead;
}
