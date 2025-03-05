using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Barefeet.Algorithm
{
    public static class CollectionExtensions
    {
        public static IEnumerable<T> ReplaceCollectionExceptFirst<T>(this List<T> list, IEnumerable<T> newElems)
        {
            yield return list.First();
            foreach (var elem in newElems)
            {
                yield return elem;
            }
        }


    }
}
