﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace THOK.Wms.Bll.Interfaces
{
    public interface IOutBillMasterHistoryService
    {
        bool Add(DateTime datetime);
    }
}
