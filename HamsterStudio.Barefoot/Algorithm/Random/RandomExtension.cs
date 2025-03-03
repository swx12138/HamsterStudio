
namespace HamsterStudio.Barefoot.Algorithm.Random;

static class RandomExtension
{
    public static T Choice<T>(this IEnumerable<T> values)
    {
        int size = values.Count();
        int pos = System.Random.Shared.Next() % size;
        return values.ElementAt(pos);
    }
}
