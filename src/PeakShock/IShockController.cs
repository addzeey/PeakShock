public interface IShockController
{
    void EnqueueShock(int intensity, int duration, string? code = null);
}