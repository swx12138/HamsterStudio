
namespace HamsterStudio.Barefeet.Algorithm;

public class ArrayMerger
{
    public static T[] AlternateMerge<T>(T[] arrayA, T[] arrayB)
    {
        var resultList = new List<T>();

        int maxLength = Math.Max(arrayA.Length, arrayB.Length);
        int minLength = Math.Min(arrayA.Length, arrayB.Length);

        for (int i = 0; i < maxLength; i++)
        {
            if (i < arrayA.Length)
                resultList.Add(arrayA[i]);

            if (i < arrayB.Length)
                resultList.Add(arrayB[i]);
        }

        return [.. resultList];
    }
}
