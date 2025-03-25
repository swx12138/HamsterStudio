namespace HamsterStudio.Barefeet.Task;

public class TaskDescriptor
{

}

class TaskGenerator
{
    private static readonly TaskGenerator gen = new();
    public static TaskGenerator Instance { get { return gen; } }

    private TaskGenerator() { }

    private int n = 0;

    public IHamsterTask Generate()
    {
        return new TimerTask(++n, Random.Shared.Next() % 50 + 1);
    }

    public IHamsterTask Generate(TaskDescriptor descriptor)
    {
        throw new NotImplementedException();
    }

    public IHamsterTask Generate(string url)
    {

        throw new NotImplementedException();
    }
}
