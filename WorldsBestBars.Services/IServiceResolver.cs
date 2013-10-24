namespace WorldsBestBars.Services
{
    public interface IServiceResolver
    {
        T GetService<T>() where T : new();
    }
}
