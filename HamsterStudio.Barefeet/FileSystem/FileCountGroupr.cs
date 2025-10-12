using HamsterStudio.Barefeet.FileSystem.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Barefeet.FileSystem;

public class FileCountGroupr(int minCount = 10) : IGroupManager
{
    private readonly Dictionary<string, int> FileCount = [];

    public void UpdateFileCount(string groupName, int newsCount)
    {
        if (FileCount.ContainsKey(groupName))
        {
            FileCount[groupName] += newsCount;
        }
        else
        {
            FileCount[groupName] = newsCount;
        }
    }

    public bool CreateGroup(string groupName)
    {
        return (FileCount.TryGetValue(groupName, out int count) && count > minCount);
    }

}
