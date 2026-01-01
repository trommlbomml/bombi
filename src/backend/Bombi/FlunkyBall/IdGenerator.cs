namespace FlunkyBall;

public static class IdGenerator
{
    private static int _nextFreeId = 1;
    
    public static int NextId() => Interlocked.Increment(ref _nextFreeId);
}