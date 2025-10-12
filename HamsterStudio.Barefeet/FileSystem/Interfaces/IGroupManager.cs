using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Barefeet.FileSystem.Interfaces;

public interface IGroupManager
{
    /// <summary>
    /// 已存在或创建成功返回true，不需要分组返回false
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
    bool CreateGroup(string groupName);
}
