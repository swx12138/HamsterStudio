﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Web.Interfaces
{
    public interface IBinaryDataTypeChecker
    {
        string CheckBinaryType(byte[] data);
    }

}
