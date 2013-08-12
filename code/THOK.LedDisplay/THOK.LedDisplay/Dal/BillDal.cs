using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using THOK.Util;
using THOK.LedDisplay.Dao;


namespace THOK.LedDisplay.Dal
{
    public class BillDal
    {
        public DataTable GetBillList(string type,int number,int startPage)
        {
            using (PersistentManager pm = new PersistentManager())
            {
                BillDao dao = new BillDao();
                string sql = "";
                if (type == "入库")
                {
                    sql = @"SELECT CELL_NAME AS CLEENAME,'入库' AS BILLTYPE,PRODUCT_NAME AS PRODUCTNAME,
                            CAST(ALLOT_QUANTITY/(U.QUANTITY01 * U.QUANTITY02 * U.QUANTITY03) AS INT) AS OPERATEPIECE,
                            CAST((ALLOT_QUANTITY/(U.QUANTITY01 * U.QUANTITY02 * U.QUANTITY03)) * U.QUANTITY01 AS INT) AS OPERATEITEM,
                            CASE WHEN I.STATUS='0' THEN '未执行' ELSE '执行中' END AS STATUS,'' AS MOVECELLNAME
                            FROM WMS_IN_BILL_ALLOT I                                        --入库分配表
                            LEFT JOIN WMS_IN_BILL_MASTER M ON I.BILL_NO=M.BILL_NO           --关联入库主表
                            LEFT JOIN WMS_CELL C ON I.CELL_CODE=C.CELL_CODE                 --关联货位表
                            LEFT JOIN WMS_PRODUCT P ON I.PRODUCT_CODE=P.PRODUCT_CODE        --关联卷烟表
                            LEFT JOIN WMS_UNIT_LIST U ON P.UNIT_LIST_CODE=U.UNIT_LIST_CODE  --关联单位系列表
                            WHERE I.STATUS != 2 AND M.STATUS IN('4','5') ORDER BY STATUS DESC";
                }
                else if(type=="出库")
                {
                    sql = @"SELECT CELL_NAME AS CLEENAME,'出库' AS BILLTYPE,PRODUCT_NAME AS PRODUCTNAME,
                            CAST(ALLOT_QUANTITY/(U.QUANTITY01 * U.QUANTITY02 * U.QUANTITY03) AS INT) AS OPERATEPIECE,
                            CAST((ALLOT_QUANTITY/(U.QUANTITY01 * U.QUANTITY02 * U.QUANTITY03)) * U.QUANTITY01 AS INT) AS OPERATEITEM,
                            CASE WHEN I.STATUS='0' THEN '未执行' ELSE '执行中' END AS STATUS,'' AS MOVECELLNAME
                            FROM WMS_OUT_BILL_ALLOT I
                            LEFT JOIN WMS_OUT_BILL_MASTER M ON I.BILL_NO=M.BILL_NO
                            LEFT JOIN WMS_CELL C ON I.CELL_CODE=C.CELL_CODE
                            LEFT JOIN WMS_PRODUCT P ON I.PRODUCT_CODE=P.PRODUCT_CODE
                            LEFT JOIN WMS_UNIT_LIST U ON P.UNIT_LIST_CODE=U.UNIT_LIST_CODE
                            WHERE I.STATUS != 2 AND M.STATUS IN('4','5') ORDER BY STATUS DESC";
                }
                else if (type == "移库")
                {
                    sql = @"SELECT C.CELL_NAME AS CLEENAME,'移库' AS BILLTYPE,PRODUCT_NAME AS PRODUCTNAME,
                            CAST(REAL_QUANTITY/(U.QUANTITY01 * U.QUANTITY02 * U.QUANTITY03) AS INT) AS OPERATEPIECE,
                            CAST((REAL_QUANTITY/(U.QUANTITY01 * U.QUANTITY02 * U.QUANTITY03)) * U.QUANTITY01 AS INT) AS OPERATEITEM,
                            CASE WHEN I.STATUS='0' THEN '未执行' ELSE '执行中' END AS STATUS,
                            W.CELL_NAME AS MOVECELLNAME									
                            FROM WMS_MOVE_BILL_DETAIL I										--移库细表
                            LEFT JOIN WMS_MOVE_BILL_MASTER M ON I.BILL_NO=M.BILL_NO			--移库主表
                            LEFT JOIN WMS_CELL C ON I.OUT_CELL_CODE=C.CELL_CODE				--出货位表
                            LEFT JOIN WMS_CELL W ON I.IN_CELL_CODE=W.CELL_CODE				--入货位表
                            LEFT JOIN WMS_PRODUCT P ON I.PRODUCT_CODE=P.PRODUCT_CODE		--卷烟表
                            LEFT JOIN WMS_UNIT_LIST U ON P.UNIT_LIST_CODE=U.UNIT_LIST_CODE	--单位系列表
                            WHERE I.STATUS != 2 AND M.STATUS IN('2','3') ORDER BY STATUS DESC";
                }
                else if (type == "盘点")
                {
                    sql = @"SELECT CELL_NAME AS CLEENAME,'盘点' AS BILLTYPE,PRODUCT_NAME AS PRODUCTNAME,
                            CAST(REAL_QUANTITY/(U.QUANTITY01 * U.QUANTITY02 * U.QUANTITY03) AS INT) AS OPERATEPIECE,
                            CAST((REAL_QUANTITY/(U.QUANTITY01 * U.QUANTITY02 * U.QUANTITY03)) * U.QUANTITY01 AS INT) AS OPERATEITEM,
                            CASE WHEN I.STATUS='0' THEN '未执行' ELSE '执行中' END AS STATUS,'' AS MOVECELLNAME
                            FROM WMS_CHECK_BILL_DETAIL I
                            LEFT JOIN WMS_CHECK_BILL_MASTER M ON I.BILL_NO=M.BILL_NO
                            LEFT JOIN WMS_CELL C ON I.CELL_CODE=C.CELL_CODE 
                            LEFT JOIN WMS_PRODUCT P ON I.PRODUCT_CODE=P.PRODUCT_CODE
                            LEFT JOIN WMS_UNIT_LIST U ON P.UNIT_LIST_CODE=U.UNIT_LIST_CODE
                            WHERE I.STATUS != 2 AND M.STATUS IN('2','3') ORDER BY STATUS DESC";
                }
                else if (type == "实时")
                {
                    sql = @"SELECT C.CELL_NAME AS CLEENAME,'移库' AS BILLTYPE,PRODUCT_NAME AS PRODUCTNAME,
                            CAST(REAL_QUANTITY/(U.QUANTITY01 * U.QUANTITY02 * U.QUANTITY03) AS INT) AS OPERATEPIECE,
                            CAST((REAL_QUANTITY/(U.QUANTITY01 * U.QUANTITY02 * U.QUANTITY03)) * U.QUANTITY01 AS INT) AS OPERATEITEM,
                            CASE WHEN I.STATUS='0' THEN '未执行' ELSE '执行中' END AS STATUS,
                            W.CELL_NAME AS MOVECELLNAME									
                            FROM WMS_MOVE_BILL_DETAIL I										--移库细表
                            LEFT JOIN WMS_MOVE_BILL_MASTER M ON I.BILL_NO=M.BILL_NO			--移库主表
                            LEFT JOIN WMS_CELL C ON I.OUT_CELL_CODE=C.CELL_CODE				--出货位表
                            LEFT JOIN WMS_CELL W ON I.IN_CELL_CODE=W.CELL_CODE				--入货位表
                            LEFT JOIN WMS_PRODUCT P ON I.PRODUCT_CODE=P.PRODUCT_CODE		--卷烟表
                            LEFT JOIN WMS_UNIT_LIST U ON P.UNIT_LIST_CODE=U.UNIT_LIST_CODE	--单位系列表
                            WHERE I.STATUS != 2 AND M.STATUS IN('2','3') AND I.CAN_REAL_OPERATE = 1 ORDER BY STATUS DESC";
                }
                return dao.GetBillList(sql);
            }
        }

    }
}
