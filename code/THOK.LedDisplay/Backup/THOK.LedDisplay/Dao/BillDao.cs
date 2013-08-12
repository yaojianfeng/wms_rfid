using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using THOK.Util;

namespace THOK.LedDisplay.Dao
{
    public class BillDao : BaseDao
    {

        public DataTable GetBillList(string sql)
        {
            return this.ExecuteQuery(sql).Tables[0];
        }

    }
}
