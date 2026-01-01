namespace FlunkyBall;

public interface IClock
{
    DateTimeOffset Now();
}

public sealed class Clock : IClock
{
    
    public DateTimeOffset Now() => DateTimeOffset.UtcNow;
}