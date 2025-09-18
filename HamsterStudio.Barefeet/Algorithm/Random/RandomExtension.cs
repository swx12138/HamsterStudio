
namespace HamsterStudio.Barefeet.Algorithm.Random;

public static class RandomExtension
{
    public static T Choice<T>(this IEnumerable<T> values)
    {
        int size = values.Count();
        int pos = System.Random.Shared.Next() % size;
        return values.ElementAt(pos);
    }

    public static int IndexSearch(int[] values)
    {
        int sum = values.Sum();
        int index = System.Random.Shared.Next(0, sum);
        for (int i = 0; i < values.Length; i++)
        {
            sum -= values[i];
            if (sum <= 0)
            {
                return i;
            }
        }
        return 0;
    }

}
