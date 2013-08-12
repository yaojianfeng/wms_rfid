using System;
using System.Linq;
using THOK.Wms.Bll.Interfaces;
using THOK.Wms.DbModel;
using Microsoft.Practices.Unity;
using THOK.Wms.Dal.Interfaces;
using System.Transactions;

namespace THOK.Wms.Bll.Service
{
    public class DailyBalanceService : ServiceBase<DailyBalance>, IDailyBalanceService
    {
        [Dependency]
        public IDailyBalanceRepository DailyBalanceRepository { get; set; }
        [Dependency]
        public IBusinessSystemsDailyBalanceRepository BusinessSystemsDailyBalanceRepository { get; set; }
        [Dependency]
        public IUnitRepository UnitRepository { get; set; }
        [Dependency]
        public IInBillDetailRepository InBillDetailRepository { get; set; }
        [Dependency]
        public IOutBillDetailRepository OutBillDetailRepository { get; set; }
        [Dependency]
        public IProfitLossBillDetailRepository ProfitLossBillDetailRepository { get; set; }

        [Dependency]
        public IInBillAllotRepository InBillAllotRepository { get; set; }

        [Dependency]
        public IOutBillAllotRepository OutBillAllotRepository { get; set; }

        [Dependency]
        public IMoveBillDetailRepository MoveBillDetailRepository { get; set; }

        [Dependency]
        public IStorageRepository StorageRepository { get; set; }

        protected override Type LogPrefix
        {
            get { return this.GetType(); }
        }

        #region IDailyBalanceService 成员

        public object GetDetails(int page, int rows, string beginDate, string endDate, string warehouseCode, string unitType)
        {
            IQueryable<DailyBalance> dailyBalanceQuery = DailyBalanceRepository.GetQueryable();

            var dailyBalance = dailyBalanceQuery.Where(i => 1 == 1);
            if (!string.IsNullOrWhiteSpace(beginDate))
            {
                DateTime begin = Convert.ToDateTime(beginDate);
                dailyBalance = dailyBalance.Where(i => i.SettleDate >= begin);
            }

            if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime end = Convert.ToDateTime(endDate);
                dailyBalance = dailyBalance.Where(i => i.SettleDate <= end);
            }

            var dailyBalances = dailyBalance.GroupBy(c => new { c.SettleDate, c.WarehouseCode, c.Warehouse.WarehouseName })
                                            .Select(c => new
                                            {
                                                SettleDate = c.Key.SettleDate,
                                                WarehouseCode = c.Key.WarehouseCode,
                                                WarehouseName = c.Key.WarehouseName,
                                                Beginning = c.Sum(p => p.Beginning),
                                                EntryAmount = c.Sum(p => p.EntryAmount),
                                                DeliveryAmount = c.Sum(p => p.DeliveryAmount),
                                                ProfitAmount = c.Sum(p => p.ProfitAmount),
                                                LossAmount = c.Sum(p => p.LossAmount),
                                                Ending = c.Sum(p => p.Ending)
                                            });

            if (!string.IsNullOrEmpty(warehouseCode))
            {               
                dailyBalances = dailyBalances.Where(i => i.WarehouseCode == warehouseCode);
            }
            else
            {
                dailyBalances = dailyBalances.GroupBy(d => new { d.SettleDate })
                                             .Select(c => new
                                             {
                                                 SettleDate = c.Key.SettleDate,
                                                 WarehouseCode = "",
                                                 WarehouseName = "全部仓库",
                                                 Beginning = c.Sum(p => p.Beginning),
                                                 EntryAmount = c.Sum(p => p.EntryAmount),
                                                 DeliveryAmount = c.Sum(p => p.DeliveryAmount),
                                                 ProfitAmount = c.Sum(p => p.ProfitAmount),
                                                 LossAmount = c.Sum(p => p.LossAmount),
                                                 Ending = c.Sum(p => p.Ending)
                                             });
            }

            dailyBalances = dailyBalances.OrderBy(c => new { c.SettleDate, c.WarehouseCode });
            int total = dailyBalances.Count();
            dailyBalances = dailyBalances.OrderByDescending(s => s.SettleDate).Skip((page - 1) * rows).Take(rows);

            string unitName = "标准件";
            decimal count = 10000;
            if (unitType == "2")
            {
                unitName = "标准条";
                count = 200;
            }

            var temp = dailyBalances.ToArray().Select(d => new
            {
                SettleDate = d.SettleDate.ToString("yyyy-MM-dd"),
                WarehouseCode = d.WarehouseCode,
                WarehouseName = d.WarehouseName == "" ? "全部仓库" : d.WarehouseName,
                UnitName = unitName,
                Beginning = d.Beginning / count,
                EntryAmount = d.EntryAmount / count,
                DeliveryAmount = d.DeliveryAmount / count,
                ProfitAmount = d.ProfitAmount / count,
                LossAmount = d.LossAmount / count,
                Ending = d.Ending / count
            });
            return new { total, rows = temp.ToArray() };
        }

        public object GetInfoDetails(int page, int rows, string warehouseCode, string settleDate, string unitType, string areas)
        {
            DateTime date = Convert.ToDateTime(settleDate);
            if (string.IsNullOrWhiteSpace(unitType))  unitType = "1";
            
            IQueryable<DailyBalance> dailyBalanceQuery = DailyBalanceRepository.GetQueryable();
            var query = dailyBalanceQuery.Where(i => i.SettleDate == date)
                                         .Select(i => new
                                         {
                                             i.SettleDate,
                                             i.ProductCode,
                                             i.Product.ProductName,
                                             UnitCode01 = i.Product.UnitList.Unit01.UnitCode,
                                             UnitName01 = i.Product.UnitList.Unit01.UnitName,
                                             UnitCode02 = i.Product.UnitList.Unit02.UnitCode,
                                             UnitName02 = i.Product.UnitList.Unit02.UnitName,
                                             Count01 = i.Product.UnitList.Unit01.Count,
                                             Count02 = i.Product.UnitList.Unit02.Count,
                                             i.WarehouseCode,
                                             i.Warehouse.WarehouseName,
                                             i.AreaCode,
                                             i.Area.AreaName,
                                             Beginning = i.Beginning,
                                             EntryAmount = i.EntryAmount,
                                             DeliveryAmount = i.DeliveryAmount,
                                             ProfitAmount = i.ProfitAmount,
                                             LossAmount = i.LossAmount,
                                             Ending = i.Ending
                                         });
            if (areas != null)
            {
                query = query.GroupBy(q => new
                {
                    q.SettleDate,
                    q.WarehouseCode,
                    q.WarehouseName,
                    q.AreaCode,
                    q.AreaName,
                    q.ProductCode,
                    q.ProductName,
                    q.UnitCode01,
                    q.UnitName01,
                    q.UnitCode02,
                    q.UnitName02,
                    q.Count01,
                    q.Count02,
                })
                .Select(i => new
                {
                    i.Key.SettleDate,
                    i.Key.ProductCode,
                    i.Key.ProductName,
                    i.Key.UnitCode01,
                    i.Key.UnitName01,
                    i.Key.UnitCode02,
                    i.Key.UnitName02,
                    i.Key.Count01,
                    i.Key.Count02,
                    i.Key.WarehouseCode,
                    i.Key.WarehouseName,
                    i.Key.AreaCode,
                    i.Key.AreaName,
                    Beginning = i.Sum(q => q.Beginning),
                    EntryAmount = i.Sum(q => q.EntryAmount),
                    DeliveryAmount = i.Sum(q => q.DeliveryAmount),
                    ProfitAmount = i.Sum(q => q.ProfitAmount),
                    LossAmount = i.Sum(q => q.LossAmount),
                    Ending = i.Sum(q => q.Ending)
                });
                if (areas != "*") query = query.Where(i => areas.Contains(i.AreaCode));
            }
            else
            {
                if (!string.IsNullOrEmpty(warehouseCode))
                {
                    query = query.GroupBy(q => new
                    {
                        q.SettleDate,
                        q.WarehouseCode,
                        q.WarehouseName,
                        q.ProductCode,
                        q.ProductName,
                        q.UnitCode01,
                        q.UnitName01,
                        q.UnitCode02,
                        q.UnitName02,
                        q.Count01,
                        q.Count02,
                    })
                    .Select(i => new
                    {
                        i.Key.SettleDate,
                        i.Key.ProductCode,
                        i.Key.ProductName,
                        i.Key.UnitCode01,
                        i.Key.UnitName01,
                        i.Key.UnitCode02,
                        i.Key.UnitName02,
                        i.Key.Count01,
                        i.Key.Count02,
                        i.Key.WarehouseCode,
                        i.Key.WarehouseName,
                        AreaCode = "",
                        AreaName = "",
                        Beginning = i.Sum(q => q.Beginning),
                        EntryAmount = i.Sum(q => q.EntryAmount),
                        DeliveryAmount = i.Sum(q => q.DeliveryAmount),
                        ProfitAmount = i.Sum(q => q.ProfitAmount),
                        LossAmount = i.Sum(q => q.LossAmount),
                        Ending = i.Sum(q => q.Ending)
                    });
                    query = query.Where(i => i.WarehouseCode == warehouseCode);
                }
                else
                {
                    query = query.GroupBy(q => new
                    {
                        q.SettleDate,
                        q.ProductCode,
                        q.ProductName,
                        q.UnitCode01,
                        q.UnitName01,
                        q.UnitCode02,
                        q.UnitName02,
                        q.Count01,
                        q.Count02,
                    })
                    .Select(i => new
                    {
                        i.Key.SettleDate,
                        i.Key.ProductCode,
                        i.Key.ProductName,
                        i.Key.UnitCode01,
                        i.Key.UnitName01,
                        i.Key.UnitCode02,
                        i.Key.UnitName02,
                        i.Key.Count01,
                        i.Key.Count02,
                        WarehouseCode = "全部仓库",
                        WarehouseName = "全部仓库",
                        AreaCode = "",
                        AreaName = "",
                        Beginning = i.Sum(q => q.Beginning),
                        EntryAmount = i.Sum(q => q.EntryAmount),
                        DeliveryAmount = i.Sum(q => q.DeliveryAmount),
                        ProfitAmount = i.Sum(q => q.ProfitAmount),
                        LossAmount = i.Sum(q => q.LossAmount),
                        Ending = i.Sum(q => q.Ending)
                    });
                }
            }
            query = query.OrderByDescending(i => i.SettleDate)
                         .ThenBy(i => i.WarehouseCode)
                         .ThenBy(i => i.AreaCode)
                         .ThenBy(i => i.ProductCode);

            int total = query.Count();
            query = query.Skip((page - 1) * rows).Take(rows);

            string unitName = "";
            decimal count = 1;

            //标准单位（标准件||标准条）
            if (unitType == "1" || unitType == "2")
            {
                if (unitType == "1")
                {
                    unitName = "标准件";
                    count = 10000;
                }

                if (unitType == "2")
                {
                    unitName = "标准条";
                    count = 200;
                }
                var dailyBalance = query.ToArray().Select(i => new
                                         {
                                             SettleDate = i.SettleDate.ToString("yyyy-MM-dd"),
                                             i.ProductCode,
                                             i.ProductName,
                                             UnitCode = "",
                                             UnitName = unitName,
                                             i.WarehouseCode,
                                             i.WarehouseName,
                                             i.AreaCode,
                                             i.AreaName,
                                             Beginning = i.Beginning / count,
                                             EntryAmount = i.EntryAmount / count,
                                             DeliveryAmount = i.DeliveryAmount / count,
                                             ProfitAmount = i.ProfitAmount / count,
                                             LossAmount = i.LossAmount / count,
                                             Ending = i.Ending / count
                                         });
                return new { total, rows = dailyBalance.ToArray() };
            }

            //自然件
            if (unitType == "3" || unitType == "4")
            {
                var dailyBalance = query.ToArray().Select(i => new
                                         {
                                             SettleDate = i.SettleDate.ToString("yyyy-MM-dd"),
                                             i.ProductCode,
                                             i.ProductName,
                                             UnitCode = unitType == "3" ? i.UnitCode01 : i.UnitCode02,
                                             UnitName = unitType == "3" ? i.UnitName01 : i.UnitName02,
                                             i.WarehouseCode,
                                             i.WarehouseName,
                                             i.AreaCode,
                                             i.AreaName,
                                             Beginning = i.Beginning / (unitType == "3" ? i.Count01 : i.Count02),
                                             EntryAmount = i.EntryAmount / (unitType == "3" ? i.Count01 : i.Count02),
                                             DeliveryAmount = i.DeliveryAmount / (unitType == "3" ? i.Count01 : i.Count02),
                                             ProfitAmount = i.ProfitAmount / (unitType == "3" ? i.Count01 : i.Count02),
                                             LossAmount = i.LossAmount / (unitType == "3" ? i.Count01 : i.Count02),
                                             Ending = i.Ending / (unitType == "3" ? i.Count01 : i.Count02),
                                         });
                return new { total, rows = dailyBalance.ToArray() };
            }
            return new { total, rows = query.ToArray() };
        }

        public object GetInfoCheck(int page, int rows, string warehouseCode, string settleDate, string unitType)
        {
            DateTime date = Convert.ToDateTime(settleDate);
            if (unitType == null || unitType == "")
            {
                unitType = "1";
            }
            IQueryable<DailyBalance> dailyBalanceQuery = DailyBalanceRepository.GetQueryable();
            IQueryable<BusinessSystemsDailyBalance> BusinessSystemsDailyBalanceQuery = BusinessSystemsDailyBalanceRepository.GetQueryable();
            var query = dailyBalanceQuery.Where(d => d.WarehouseCode.Contains(warehouseCode) && d.SettleDate == date)
                                         .Select(d => new //本系统产品日结结果
                                         {
                                             d.SettleDate,
                                             d.ProductCode,
                                             d.Product.ProductName,
                                             UnitCode01 = d.Product.UnitList.Unit01.UnitCode,
                                             UnitName01 = d.Product.UnitList.Unit01.UnitName,
                                             UnitCode02 = d.Product.UnitList.Unit02.UnitCode,
                                             UnitName02 = d.Product.UnitList.Unit02.UnitName,
                                             Count01 = d.Product.UnitList.Unit01.Count,
                                             Count02 = d.Product.UnitList.Unit02.Count,
                                             d.WarehouseCode,
                                             d.Warehouse.WarehouseName,
                                             Beginning = d.Beginning,
                                             EntryAmount = d.EntryAmount,
                                             DeliveryAmount = d.DeliveryAmount,
                                             ProfitAmount = d.ProfitAmount,
                                             LossAmount = d.LossAmount,
                                             Ending = d.Ending,
                                             BSys_Beginning = decimal.Zero,
                                             BSys_EntryAmount = decimal.Zero,
                                             BSys_DeliveryAmount = decimal.Zero,
                                             BSys_ProfitAmount = decimal.Zero,
                                             BSys_LossAmount = decimal.Zero,
                                             BSys_Ending = decimal.Zero
                                         }).Concat(BusinessSystemsDailyBalanceQuery.Where(b => b.WarehouseCode.Contains(warehouseCode) && b.SettleDate == date)
                                                                                   .Select(b => new //营销系统产品日结结果
                                                                                    {
                                                                                        b.SettleDate,
                                                                                        b.ProductCode,
                                                                                        b.Product.ProductName,
                                                                                        UnitCode01 = b.Product.UnitList.Unit01.UnitCode,
                                                                                        UnitName01 = b.Product.UnitList.Unit01.UnitName,
                                                                                        UnitCode02 = b.Product.UnitList.Unit02.UnitCode,
                                                                                        UnitName02 = b.Product.UnitList.Unit02.UnitName,
                                                                                        Count01 = b.Product.UnitList.Unit01.Count,
                                                                                        Count02 = b.Product.UnitList.Unit02.Count,
                                                                                        b.WarehouseCode,
                                                                                        b.Warehouse.WarehouseName,
                                                                                        Beginning = decimal.Zero,
                                                                                        EntryAmount = decimal.Zero,
                                                                                        DeliveryAmount = decimal.Zero,
                                                                                        ProfitAmount = decimal.Zero,
                                                                                        LossAmount = decimal.Zero,
                                                                                        Ending = decimal.Zero,
                                                                                        BSys_Beginning = b.Beginning,
                                                                                        BSys_EntryAmount = b.EntryAmount,
                                                                                        BSys_DeliveryAmount = b.DeliveryAmount,
                                                                                        BSys_ProfitAmount = b.ProfitAmount,
                                                                                        BSys_LossAmount = b.LossAmount,
                                                                                        BSys_Ending = b.Ending
                                                                                    }));
            var InfoCheck = query.GroupBy(q => new { q.SettleDate, q.WarehouseCode, q.ProductCode })
                        .Select(q => new   //两个系统产品日结结果汇总
                        {
                            q.Key.SettleDate,
                            q.Key.WarehouseCode,
                            q.Key.ProductCode,
                            ProductName = q.Max(i => i.ProductName),
                            WarehouseName = q.Max(i => i.WarehouseName),
                            UnitCode01 = q.Max(i => i.UnitCode01),
                            UnitName01 = q.Max(i => i.UnitName01),
                            UnitCode02 = q.Max(i => i.UnitCode02),
                            UnitName02 = q.Max(i => i.UnitName02),
                            Count01 = q.Max(i => i.Count01),
                            Count02 = q.Max(i => i.Count02),
                            Beginning = q.Sum(i => i.Beginning),
                            EntryAmount = q.Sum(i => i.EntryAmount),
                            DeliveryAmount = q.Sum(i => i.DeliveryAmount),
                            ProfitAmount = q.Sum(i => i.ProfitAmount),
                            LossAmount = q.Sum(i => i.LossAmount),
                            Ending = q.Sum(i => i.Ending),
                            BSys_Beginning = q.Sum(i => i.BSys_Beginning),
                            BSys_EntryAmount = q.Sum(i => i.BSys_EntryAmount),
                            BSys_DeliveryAmount = q.Sum(i => i.BSys_DeliveryAmount),
                            BSys_ProfitAmount = q.Sum(i => i.BSys_ProfitAmount),
                            BSys_LossAmount = q.Sum(i => i.BSys_LossAmount),
                            BSys_Ending = q.Sum(i => i.BSys_Ending),
                        }).Select(n => new //两个系统产品日结结果比对
                        {
                            n.SettleDate,
                            n.WarehouseCode,
                            n.ProductCode,
                            n.ProductName,
                            n.WarehouseName,
                            n.UnitCode01,
                            n.UnitName01,
                            n.UnitCode02,
                            n.UnitName02,
                            n.Count01,
                            n.Count02,
                            n.Beginning,
                            n.EntryAmount,
                            n.DeliveryAmount,
                            n.ProfitAmount,
                            n.LossAmount,
                            n.Ending,
                            n.BSys_Beginning,
                            n.BSys_EntryAmount,
                            n.BSys_DeliveryAmount,
                            n.BSys_ProfitAmount,
                            n.BSys_LossAmount,
                            n.BSys_Ending,
                            Status = n.Beginning == n.BSys_Beginning &&
                                        n.EntryAmount == n.BSys_EntryAmount &&
                                        n.DeliveryAmount == n.BSys_DeliveryAmount &&
                                        n.ProfitAmount == n.BSys_ProfitAmount &&
                                        n.LossAmount == n.BSys_LossAmount &&
                                        n.Ending == n.BSys_Ending ?
                                        "T" : "F",
                            Status1 = n.Beginning == n.BSys_Beginning ?
                                "T" : "F",
                            Status2 = n.EntryAmount == n.BSys_EntryAmount ?
                                "T" : "F",
                            Status3 = n.DeliveryAmount == n.BSys_DeliveryAmount ?
                                "T" : "F",
                            Status4 = n.ProfitAmount == n.BSys_ProfitAmount ?
                                "T" : "F",
                            Status5 = n.LossAmount == n.BSys_LossAmount ?
                                "T" : "F",
                            Status6 = n.Ending == n.BSys_Ending ?
                                "true" : "F"
                        }).OrderBy(n => n.Status)
                          .OrderBy(n => n.WarehouseName)
                          .OrderBy(n => n.ProductCode);

            int total = InfoCheck.Count();
            var infoCheck = InfoCheck.Skip((page - 1) * rows).Take(rows);

            //两个系统产品日结结果比对结果按照单位换算
            string unitName = "";
            decimal count = 1;

            //标准单位（标准件||标准条）
            if (unitType == "1" || unitType == "2")
            {
                if (unitType == "1")
                {
                    unitName = "标准件";
                    count = 10000;
                }

                if (unitType == "2")
                {
                    unitName = "标准条";
                    count = 200;
                }
                var dailyBalance = infoCheck.ToArray().Select(i => new
                {
                    SettleDate = i.SettleDate.ToString("yyyy-MM-dd"),
                    i.ProductCode,
                    i.ProductName,
                    UnitCode = "",
                    UnitName = unitName,
                    i.WarehouseCode,
                    i.WarehouseName,
                    Beginning = i.Beginning == i.BSys_Beginning ?
                    (i.Beginning / count).ToString() :
                    (i.Beginning / count).ToString() + "≠" + (i.BSys_Beginning / count).ToString(),
                    EntryAmount = i.EntryAmount == i.BSys_EntryAmount ?
                    (i.EntryAmount / count).ToString() :
                    (i.EntryAmount / count).ToString() + "≠" + (i.BSys_EntryAmount / count).ToString(),
                    DeliveryAmount = i.DeliveryAmount == i.BSys_DeliveryAmount ?
                    (i.DeliveryAmount / count).ToString() :
                    (i.DeliveryAmount / count).ToString() + "≠" + (i.BSys_DeliveryAmount / count).ToString(),
                    ProfitAmount = i.ProfitAmount == i.BSys_ProfitAmount ?
                    (i.ProfitAmount / count).ToString() :
                    (i.ProfitAmount / count).ToString() + "≠" + (i.BSys_ProfitAmount / count).ToString(),
                    LossAmount = i.LossAmount == i.BSys_LossAmount ?
                    (i.LossAmount / count).ToString() :
                    (i.LossAmount / count).ToString() + "≠" + (i.BSys_LossAmount / count).ToString(),
                    Ending = i.Ending == i.BSys_Ending ?
                    (i.Ending / count).ToString() :
                    (i.Ending / count).ToString() + "≠" + (i.BSys_Ending / count).ToString(),
                    i.Status1,
                    i.Status2,
                    i.Status3,
                    i.Status4,
                    i.Status5,
                    i.Status6
                });
                return new { total, rows = dailyBalance.ToArray() };
            }

            //自然件
            if (unitType == "3" || unitType == "4")
            {
                var dailyBalance = infoCheck.ToArray().Select(i => new
                {
                    SettleDate = i.SettleDate.ToString("yyyy-MM-dd"),
                    i.ProductCode,
                    i.ProductName,
                    UnitCode = unitType == "3" ? i.UnitCode01 : i.UnitCode02,
                    UnitName = unitType == "3" ? i.UnitName01 : i.UnitName02,
                    i.WarehouseCode,
                    i.WarehouseName,
                    Beginning = i.Beginning == i.BSys_Beginning ?
                    (i.Beginning / (unitType == "3" ? i.Count01 : i.Count02)).ToString() :
                    (i.Beginning / (unitType == "3" ? i.Count01 : i.Count02)).ToString() + "≠" + (i.BSys_Beginning / (unitType == "3" ? i.Count01 : i.Count02)).ToString(),
                    EntryAmount = i.EntryAmount == i.BSys_EntryAmount ?
                    (i.EntryAmount / (unitType == "3" ? i.Count01 : i.Count02)).ToString() :
                    (i.EntryAmount / (unitType == "3" ? i.Count01 : i.Count02)).ToString() + "≠" + (i.BSys_EntryAmount / (unitType == "3" ? i.Count01 : i.Count02)).ToString(),
                    DeliveryAmount = i.DeliveryAmount == i.BSys_DeliveryAmount ?
                    (i.DeliveryAmount / (unitType == "3" ? i.Count01 : i.Count02)).ToString() :
                    (i.DeliveryAmount / (unitType == "3" ? i.Count01 : i.Count02)).ToString() + "≠" + (i.BSys_DeliveryAmount / (unitType == "3" ? i.Count01 : i.Count02)).ToString(),
                    ProfitAmount = i.ProfitAmount == i.BSys_ProfitAmount ?
                    (i.ProfitAmount / (unitType == "3" ? i.Count01 : i.Count02)).ToString() :
                    (i.ProfitAmount / (unitType == "3" ? i.Count01 : i.Count02)).ToString() + "≠" + (i.BSys_ProfitAmount / (unitType == "3" ? i.Count01 : i.Count02)).ToString(),
                    LossAmount = i.LossAmount == i.BSys_LossAmount ?
                    (i.LossAmount / (unitType == "3" ? i.Count01 : i.Count02)).ToString() :
                    (i.LossAmount / (unitType == "3" ? i.Count01 : i.Count02)).ToString() + "≠" + (i.BSys_LossAmount / (unitType == "3" ? i.Count01 : i.Count02)).ToString(),
                    Ending = i.Ending == i.BSys_Ending ?
                    (i.Ending / (unitType == "3" ? i.Count01 : i.Count02)).ToString() :
                    (i.Ending / (unitType == "3" ? i.Count01 : i.Count02)).ToString() + "≠" + (i.BSys_Ending / (unitType == "3" ? i.Count01 : i.Count02)).ToString(),
                    i.Status1,
                    i.Status2,
                    i.Status3,
                    i.Status4,
                    i.Status5,
                    i.Status6
                });
                return new { total, rows = dailyBalance.ToArray() };
            }
            return new { total, rows = query.ToArray() };
        }

        public Boolean DoDailyBalance(string warehouseCode, string settleDate, ref string errorInfo)
        {
            try
            {
                var inQuery = InBillAllotRepository.GetQueryable();
                var outQuery = OutBillAllotRepository.GetQueryable();
                var moveQuery = MoveBillDetailRepository.GetQueryable();
                var profitLossQuery = ProfitLossBillDetailRepository.GetQueryable();
                var dailyBalanceQuery = DailyBalanceRepository.GetQueryable();
                var storageQuery = StorageRepository.GetQueryable();

                DateTime dt1 = Convert.ToDateTime(settleDate);
                var maxDailyBalance = dailyBalanceQuery.Where(d => d.SettleDate < dt1)
                                           .OrderByDescending(d => d.SettleDate)
                                           .FirstOrDefault();
                if (maxDailyBalance != null)
                {
                    if (maxDailyBalance.SettleDate < DateTime.Now 
                        && maxDailyBalance.SettleDate.ToString("yyyy-MM-dd") != DateTime.Now.ToString("yyyy-MM-dd"))
                    {
                        dt1 = maxDailyBalance.SettleDate.AddDays(1);
                        settleDate = dt1.ToString("yyyy-MM-dd");
                    }
                }

                DateTime dt2 = dt1.AddDays(1);
                if (DateTime.Now < dt1)
                {
                    errorInfo = "选择日结日期大于当前日期，不可以进行日结！";
                    return false;
                }
                var dailyBalance = dailyBalanceQuery.Where(d => d.SettleDate < dt1)
                                           .OrderByDescending(d => d.SettleDate)
                                           .FirstOrDefault();
                DateTime? dt3 = null;
                if (dailyBalance != null) dt3 = dailyBalance.SettleDate;

                var oldDailyBalance = dailyBalanceQuery.Where(d => (d.WarehouseCode == warehouseCode || string.IsNullOrEmpty(warehouseCode))
                                                                 && d.SettleDate >= dt1).ToArray();
                DailyBalanceRepository.Delete(oldDailyBalance);
                DailyBalanceRepository.SaveChanges();

                //if (inQuery.Where(i => i.InBillMaster.BillDate >= dt1 && i.InBillMaster.BillDate < dt2 && i.InBillMaster.Status != "6").Any()
                //    || outQuery.Where(i => i.OutBillMaster.BillDate >= dt1 && i.OutBillMaster.BillDate < dt2 && i.OutBillMaster.Status != "6").Any()
                //    || moveQuery.Where(i => i.MoveBillMaster.BillDate >= dt1 && i.MoveBillMaster.BillDate < dt2 && i.MoveBillMaster.Status != "4").Any()
                //    || profitLossQuery.Where(i => i.ProfitLossBillMaster.BillDate >= dt1 && i.ProfitLossBillMaster.BillDate < dt2 && i.ProfitLossBillMaster.Status != "2").Any())
                //{
                //    errorInfo = "选择日结日期还有订单未结单，不可以进行日结！";
                //    return false;
                //}

                var query = inQuery.Where(a => (a.InBillMaster.WarehouseCode == warehouseCode || string.IsNullOrEmpty(warehouseCode))
                                              && a.FinishTime >= dt1 && a.FinishTime < dt2
                                              && dt3 != null)
                    .Select(a => new
                    {
                        BillDate = settleDate,
                        WarehouseCode = a.InBillMaster.Warehouse.WarehouseCode,
                        AreaCode = a.Cell.AreaCode,
                        ProductCode = a.ProductCode,
                        UnitCode = a.Product.UnitCode,
                        Beginning = decimal.Zero,
                        EntryAmount = a.RealQuantity,
                        DeliveryAmount = decimal.Zero,
                        ProfitAmount = decimal.Zero,
                        LossAmount = decimal.Zero,
                        Ending = decimal.Zero
                    })
                .Concat(
                    outQuery.Where(a => (a.OutBillMaster.WarehouseCode == warehouseCode || string.IsNullOrEmpty(warehouseCode))
                                            && a.FinishTime >= dt1 && a.FinishTime < dt2
                                            && dt3 != null)
                    .Select(a => new
                    {
                        BillDate = settleDate,
                        WarehouseCode = a.OutBillMaster.Warehouse.WarehouseCode,
                        AreaCode = a.Cell.AreaCode,
                        ProductCode = a.ProductCode,
                        UnitCode = a.Product.UnitCode,
                        Beginning = decimal.Zero,
                        EntryAmount = decimal.Zero,
                        DeliveryAmount = a.RealQuantity,
                        ProfitAmount = decimal.Zero,
                        LossAmount = decimal.Zero,
                        Ending = decimal.Zero
                    })
                ).Concat(
                    moveQuery.Where(a => (a.MoveBillMaster.WarehouseCode == warehouseCode || string.IsNullOrEmpty(warehouseCode))
                                            && a.FinishTime >= dt1 && a.FinishTime < dt2
                                            && a.Status == "2"
                                            && dt3 != null)
                    .Select(a => new
                    {
                        BillDate = settleDate,
                        WarehouseCode = a.MoveBillMaster.Warehouse.WarehouseCode,
                        AreaCode = a.OutCell.AreaCode,
                        ProductCode = a.ProductCode,
                        UnitCode = a.Product.UnitCode,
                        Beginning = decimal.Zero,
                        EntryAmount = decimal.Zero,
                        DeliveryAmount = a.RealQuantity,
                        ProfitAmount = decimal.Zero,
                        LossAmount = decimal.Zero,
                        Ending = decimal.Zero
                    })
                )
                .Concat(
                    moveQuery.Where(a => (a.MoveBillMaster.WarehouseCode == warehouseCode || string.IsNullOrEmpty(warehouseCode))
                                            && a.FinishTime >= dt1 && a.FinishTime < dt2
                                            && a.Status == "2"
                                            && dt3 != null)
                    .Select(a => new
                    {
                        BillDate = settleDate,
                        WarehouseCode = a.MoveBillMaster.Warehouse.WarehouseCode,
                        AreaCode = a.InCell.AreaCode,
                        ProductCode = a.ProductCode,
                        UnitCode = a.Product.UnitCode,
                        Beginning = decimal.Zero,
                        EntryAmount = a.RealQuantity,
                        DeliveryAmount = decimal.Zero,
                        ProfitAmount = decimal.Zero,
                        LossAmount = decimal.Zero,
                        Ending = decimal.Zero
                    })
                ).Concat(
                    profitLossQuery.Where(a => (a.ProfitLossBillMaster.WarehouseCode == warehouseCode || string.IsNullOrEmpty(warehouseCode))
                                                    && a.ProfitLossBillMaster.VerifyDate >= dt1 && a.ProfitLossBillMaster.VerifyDate < dt2
                                                    && dt3 != null)
                    .Select(a => new
                    {
                        BillDate = settleDate,
                        WarehouseCode = a.ProfitLossBillMaster.Warehouse.WarehouseCode,
                        AreaCode = a.Storage.Cell.AreaCode,
                        ProductCode = a.ProductCode,
                        UnitCode = a.Product.UnitCode,
                        Beginning = decimal.Zero,
                        EntryAmount = decimal.Zero,
                        DeliveryAmount = decimal.Zero,
                        ProfitAmount = a.Quantity > 0 ? Math.Abs(a.Quantity) : decimal.Zero,
                        LossAmount = a.Quantity < 0 ? Math.Abs(a.Quantity) : decimal.Zero,
                        Ending = decimal.Zero
                    })
                ).Concat(
                    dailyBalanceQuery.Where(d => (d.WarehouseCode == warehouseCode || string.IsNullOrEmpty(warehouseCode))
                                                      && dt3 != null && d.SettleDate == dt3
                                                      && d.Ending != decimal.Zero)
                    .Select(a => new
                    {
                        BillDate = settleDate,
                        WarehouseCode = a.WarehouseCode,
                        AreaCode = a.AreaCode,
                        ProductCode = a.ProductCode,
                        UnitCode = a.Product.UnitCode,
                        Beginning = a.Ending,
                        EntryAmount = decimal.Zero,
                        DeliveryAmount = decimal.Zero,
                        ProfitAmount = decimal.Zero,
                        LossAmount = decimal.Zero,
                        Ending = decimal.Zero
                    }
                )).Concat(
                    storageQuery.Where(s => (s.Cell.Area.WarehouseCode == warehouseCode || string.IsNullOrEmpty(warehouseCode))
                                                && s.Product != null && dt3 == null)
                    .GroupBy(s => new { s.Cell.Area.WarehouseCode,s.Cell.AreaCode, s.ProductCode, s.Product.UnitCode })
                    .Select(s => new
                    {
                        BillDate = settleDate,
                        WarehouseCode = s.Key.WarehouseCode,
                        AreaCode = s.Key.AreaCode,
                        ProductCode = s.Key.ProductCode,
                        UnitCode = s.Key.UnitCode,
                        Beginning = s.Sum(p => p.Quantity),
                        EntryAmount = decimal.Zero,
                        DeliveryAmount = decimal.Zero,
                        ProfitAmount = decimal.Zero,
                        LossAmount = decimal.Zero,
                        Ending = decimal.Zero
                    }
                )).ToArray();

                var newDailyBalance = query.AsParallel().GroupBy(a => new { a.BillDate, a.WarehouseCode,a.AreaCode, a.ProductCode, a.UnitCode })
                .Select(a => new DailyBalance
                {
                    SettleDate = Convert.ToDateTime(a.Key.BillDate),
                    WarehouseCode = a.Key.WarehouseCode,
                    AreaCode = a.Key.AreaCode,
                    ProductCode = a.Key.ProductCode,
                    UnitCode = a.Key.UnitCode,
                    Beginning = a.Sum(d => d.Beginning),
                    EntryAmount = a.Sum(d => d.EntryAmount),
                    DeliveryAmount = a.Sum(d => d.DeliveryAmount),
                    ProfitAmount = a.Sum(d => d.ProfitAmount),
                    LossAmount = a.Sum(d => d.LossAmount),
                    Ending = a.Sum(d => d.Beginning) + a.Sum(d => d.EntryAmount) - a.Sum(d => d.DeliveryAmount) + a.Sum(d => d.ProfitAmount) - a.Sum(d => d.LossAmount)
                }).ToArray();

                foreach (var item in newDailyBalance)
                {
                    item.ID = Guid.NewGuid();
                    DailyBalanceRepository.Add(item);
                }

                DailyBalanceRepository.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                errorInfo = "日结时出现错误，详情：" + e.Message;
                return false;
            }
        }

        #endregion
        
        #region 日结明细
        public System.Data.DataTable GetInfoDetail(int page, int rows, string warehouseCode, string settleDate, string unitType, string areas = null)
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            DateTime date = Convert.ToDateTime(settleDate);
            if (string.IsNullOrWhiteSpace(unitType)) unitType = "1";

            IQueryable<DailyBalance> dailyBalanceQuery = DailyBalanceRepository.GetQueryable();
            var query = dailyBalanceQuery.Where(i => i.SettleDate == date)
                                         .Select(i => new
                                         {
                                             i.SettleDate,
                                             i.ProductCode,
                                             i.Product.ProductName,
                                             UnitCode01 = i.Product.UnitList.Unit01.UnitCode,
                                             UnitName01 = i.Product.UnitList.Unit01.UnitName,
                                             UnitCode02 = i.Product.UnitList.Unit02.UnitCode,
                                             UnitName02 = i.Product.UnitList.Unit02.UnitName,
                                             Count01 = i.Product.UnitList.Unit01.Count,
                                             Count02 = i.Product.UnitList.Unit02.Count,
                                             i.WarehouseCode,
                                             i.Warehouse.WarehouseName,
                                             i.AreaCode,
                                             i.Area.AreaName,
                                             Beginning = i.Beginning,
                                             EntryAmount = i.EntryAmount,
                                             DeliveryAmount = i.DeliveryAmount,
                                             ProfitAmount = i.ProfitAmount,
                                             LossAmount = i.LossAmount,
                                             Ending = i.Ending
                                         });
            if (areas != null)
            {
                query = query.GroupBy(q => new
                {
                    q.SettleDate,
                    q.WarehouseCode,
                    q.WarehouseName,
                    q.AreaCode,
                    q.AreaName,
                    q.ProductCode,
                    q.ProductName,
                    q.UnitCode01,
                    q.UnitName01,
                    q.UnitCode02,
                    q.UnitName02,
                    q.Count01,
                    q.Count02,
                })
                .Select(i => new
                {
                    i.Key.SettleDate,
                    i.Key.ProductCode,
                    i.Key.ProductName,
                    i.Key.UnitCode01,
                    i.Key.UnitName01,
                    i.Key.UnitCode02,
                    i.Key.UnitName02,
                    i.Key.Count01,
                    i.Key.Count02,
                    i.Key.WarehouseCode,
                    i.Key.WarehouseName,
                    i.Key.AreaCode,
                    i.Key.AreaName,
                    Beginning = i.Sum(q => q.Beginning),
                    EntryAmount = i.Sum(q => q.EntryAmount),
                    DeliveryAmount = i.Sum(q => q.DeliveryAmount),
                    ProfitAmount = i.Sum(q => q.ProfitAmount),
                    LossAmount = i.Sum(q => q.LossAmount),
                    Ending = i.Sum(q => q.Ending)
                });
                if (areas != "*") query = query.Where(i => areas.Contains(i.AreaCode));
            }
            else
            {
                if (!string.IsNullOrEmpty(warehouseCode))
                {
                    query = query.GroupBy(q => new
                    {
                        q.SettleDate,
                        q.WarehouseCode,
                        q.WarehouseName,
                        q.ProductCode,
                        q.ProductName,
                        q.UnitCode01,
                        q.UnitName01,
                        q.UnitCode02,
                        q.UnitName02,
                        q.Count01,
                        q.Count02,
                    })
                    .Select(i => new
                    {
                        i.Key.SettleDate,
                        i.Key.ProductCode,
                        i.Key.ProductName,
                        i.Key.UnitCode01,
                        i.Key.UnitName01,
                        i.Key.UnitCode02,
                        i.Key.UnitName02,
                        i.Key.Count01,
                        i.Key.Count02,
                        i.Key.WarehouseCode,
                        i.Key.WarehouseName,
                        AreaCode = "",
                        AreaName = "",
                        Beginning = i.Sum(q => q.Beginning),
                        EntryAmount = i.Sum(q => q.EntryAmount),
                        DeliveryAmount = i.Sum(q => q.DeliveryAmount),
                        ProfitAmount = i.Sum(q => q.ProfitAmount),
                        LossAmount = i.Sum(q => q.LossAmount),
                        Ending = i.Sum(q => q.Ending)
                    });
                    query = query.Where(i => i.WarehouseCode == warehouseCode);
                }
                else
                {
                    query = query.GroupBy(q => new
                    {
                        q.SettleDate,
                        q.ProductCode,
                        q.ProductName,
                        q.UnitCode01,
                        q.UnitName01,
                        q.UnitCode02,
                        q.UnitName02,
                        q.Count01,
                        q.Count02,
                    })
                    .Select(i => new
                    {
                        i.Key.SettleDate,
                        i.Key.ProductCode,
                        i.Key.ProductName,
                        i.Key.UnitCode01,
                        i.Key.UnitName01,
                        i.Key.UnitCode02,
                        i.Key.UnitName02,
                        i.Key.Count01,
                        i.Key.Count02,
                        WarehouseCode = "全部仓库",
                        WarehouseName = "全部仓库",
                        AreaCode = "",
                        AreaName = "",
                        Beginning = i.Sum(q => q.Beginning),
                        EntryAmount = i.Sum(q => q.EntryAmount),
                        DeliveryAmount = i.Sum(q => q.DeliveryAmount),
                        ProfitAmount = i.Sum(q => q.ProfitAmount),
                        LossAmount = i.Sum(q => q.LossAmount),
                        Ending = i.Sum(q => q.Ending)
                    });
                }
            }
            query = query.OrderByDescending(i => i.SettleDate)
                         .ThenBy(i => i.WarehouseCode)
                         .ThenBy(i => i.AreaCode)
                         .ThenBy(i => i.ProductCode);
            string unitName = "";
            decimal count = 1;
            //标准单位（标准件||标准条）
            if (unitType == "1" || unitType == "2")
            {
                if (unitType == "1")
                {
                    unitName = "标准件";
                    count = 10000;
                }
                if (unitType == "2")
                {
                    unitName = "标准条";
                    count = 200;
                }
                var dailyBalance = query.AsEnumerable().Select(i => new
                {
                    SettleDate = i.SettleDate.ToString("yyyy-MM-dd"),
                    i.ProductCode,
                    i.ProductName,
                    UnitCode = "",
                    UnitName = unitName,
                    i.WarehouseCode,
                    i.WarehouseName,
                    i.AreaName,
                    Beginning = i.Beginning / count,
                    EntryAmount = i.EntryAmount / count,
                    DeliveryAmount = i.DeliveryAmount / count,
                    ProfitAmount = i.ProfitAmount / count,
                    LossAmount = i.LossAmount / count,
                    Ending = i.Ending / count
                });
                dt.Columns.Add("结算日期", typeof(string));
                dt.Columns.Add("仓库名称", typeof(string));
                dt.Columns.Add("库区名称", typeof(string));
                dt.Columns.Add("商品代码", typeof(string));
                dt.Columns.Add("商品名称", typeof(string));
                dt.Columns.Add("单位", typeof(string));
                dt.Columns.Add("期初量", typeof(decimal));
                dt.Columns.Add("入库量", typeof(decimal));
                dt.Columns.Add("出库量", typeof(decimal));
                dt.Columns.Add("报损量", typeof(decimal));
                dt.Columns.Add("报益量", typeof(decimal));
                dt.Columns.Add("结余量", typeof(decimal));
                foreach (var item in dailyBalance)
                {
                    dt.Rows.Add
                        (
                            item.SettleDate,
                            item.WarehouseName,
                            item.AreaName,
                            item.ProductCode,
                            item.ProductName,
                            item.UnitName,
                            item.Beginning,
                            item.EntryAmount,
                            item.DeliveryAmount,
                            item.ProfitAmount,
                            item.LossAmount,
                            item.Ending
                        );
                }
                return dt;
            }
            //自然件
            if (unitType == "3" || unitType == "4")
            {
                var dailyBalance = query.ToArray().Select(i => new
                {
                    SettleDate = i.SettleDate.ToString("yyyy-MM-dd"),
                    i.ProductCode,
                    i.ProductName,
                    UnitCode = unitType == "3" ? i.UnitCode01 : i.UnitCode02,
                    UnitName = unitType == "3" ? i.UnitName01 : i.UnitName02,
                    i.WarehouseCode,
                    i.WarehouseName,
                    i.AreaName,
                    Beginning = i.Beginning / (unitType == "3" ? i.Count01 : i.Count02),
                    EntryAmount = i.EntryAmount / (unitType == "3" ? i.Count01 : i.Count02),
                    DeliveryAmount = i.DeliveryAmount / (unitType == "3" ? i.Count01 : i.Count02),
                    ProfitAmount = i.ProfitAmount / (unitType == "3" ? i.Count01 : i.Count02),
                    LossAmount = i.LossAmount / (unitType == "3" ? i.Count01 : i.Count02),
                    Ending = i.Ending / (unitType == "3" ? i.Count01 : i.Count02),
                });
                dt.Columns.Add("结算日期", typeof(string));
                dt.Columns.Add("仓库名称", typeof(string));
                dt.Columns.Add("库区名称", typeof(string));
                dt.Columns.Add("商品代码", typeof(string));
                dt.Columns.Add("商品名称", typeof(string));
                dt.Columns.Add("单位", typeof(string));
                dt.Columns.Add("期初量", typeof(decimal));
                dt.Columns.Add("入库量", typeof(decimal));
                dt.Columns.Add("出库量", typeof(decimal));
                dt.Columns.Add("报损量", typeof(decimal));
                dt.Columns.Add("报益量", typeof(decimal));
                dt.Columns.Add("结余量", typeof(decimal));
                foreach (var item in dailyBalance)
                {
                    dt.Rows.Add
                        (
                            item.SettleDate,
                            item.WarehouseName,
                            item.AreaName,
                            item.ProductCode,
                            item.ProductName,
                            item.UnitName,
                            item.Beginning,
                            item.EntryAmount,
                            item.DeliveryAmount,
                            item.ProfitAmount,
                            item.LossAmount,
                            item.Ending
                        );
                }
                return dt;
            }
            return dt;
        } 
        #endregion

        #region 日结核对
        public System.Data.DataTable GetInfoChecking(int page, int rows, string warehouseCode, string settleDate, string unitType)
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            DateTime date = Convert.ToDateTime(settleDate);
            if (unitType == null || unitType == "")
            {
                unitType = "1";
            }
            IQueryable<DailyBalance> dailyBalanceQuery = DailyBalanceRepository.GetQueryable();
            IQueryable<BusinessSystemsDailyBalance> BusinessSystemsDailyBalanceQuery = BusinessSystemsDailyBalanceRepository.GetQueryable();
            var query = dailyBalanceQuery.Where(d => d.WarehouseCode.Contains(warehouseCode) && d.SettleDate == date)
                                         .Select(d => new //本系统产品日结结果
                                         {
                                             d.SettleDate,
                                             d.ProductCode,
                                             d.Product.ProductName,
                                             UnitCode01 = d.Product.UnitList.Unit01.UnitCode,
                                             UnitName01 = d.Product.UnitList.Unit01.UnitName,
                                             UnitCode02 = d.Product.UnitList.Unit02.UnitCode,
                                             UnitName02 = d.Product.UnitList.Unit02.UnitName,
                                             Count01 = d.Product.UnitList.Unit01.Count,
                                             Count02 = d.Product.UnitList.Unit02.Count,
                                             d.WarehouseCode,
                                             d.Warehouse.WarehouseName,
                                             Beginning = d.Beginning,
                                             EntryAmount = d.EntryAmount,
                                             DeliveryAmount = d.DeliveryAmount,
                                             ProfitAmount = d.ProfitAmount,
                                             LossAmount = d.LossAmount,
                                             Ending = d.Ending,
                                             BSys_Beginning = decimal.Zero,
                                             BSys_EntryAmount = decimal.Zero,
                                             BSys_DeliveryAmount = decimal.Zero,
                                             BSys_ProfitAmount = decimal.Zero,
                                             BSys_LossAmount = decimal.Zero,
                                             BSys_Ending = decimal.Zero
                                         }).Concat(BusinessSystemsDailyBalanceQuery.Where(b => b.WarehouseCode.Contains(warehouseCode) && b.SettleDate == date)
                                                                                   .Select(b => new //营销系统产品日结结果
                                                                                   {
                                                                                       b.SettleDate,
                                                                                       b.ProductCode,
                                                                                       b.Product.ProductName,
                                                                                       UnitCode01 = b.Product.UnitList.Unit01.UnitCode,
                                                                                       UnitName01 = b.Product.UnitList.Unit01.UnitName,
                                                                                       UnitCode02 = b.Product.UnitList.Unit02.UnitCode,
                                                                                       UnitName02 = b.Product.UnitList.Unit02.UnitName,
                                                                                       Count01 = b.Product.UnitList.Unit01.Count,
                                                                                       Count02 = b.Product.UnitList.Unit02.Count,
                                                                                       b.WarehouseCode,
                                                                                       b.Warehouse.WarehouseName,
                                                                                       Beginning = decimal.Zero,
                                                                                       EntryAmount = decimal.Zero,
                                                                                       DeliveryAmount = decimal.Zero,
                                                                                       ProfitAmount = decimal.Zero,
                                                                                       LossAmount = decimal.Zero,
                                                                                       Ending = decimal.Zero,
                                                                                       BSys_Beginning = b.Beginning,
                                                                                       BSys_EntryAmount = b.EntryAmount,
                                                                                       BSys_DeliveryAmount = b.DeliveryAmount,
                                                                                       BSys_ProfitAmount = b.ProfitAmount,
                                                                                       BSys_LossAmount = b.LossAmount,
                                                                                       BSys_Ending = b.Ending
                                                                                   }));
            var InfoCheck = query.GroupBy(q => new { q.SettleDate, q.WarehouseCode, q.ProductCode })
                        .Select(q => new   //两个系统产品日结结果汇总
                        {
                            q.Key.SettleDate,
                            q.Key.WarehouseCode,
                            q.Key.ProductCode,
                            ProductName = q.Max(i => i.ProductName),
                            WarehouseName = q.Max(i => i.WarehouseName),
                            UnitCode01 = q.Max(i => i.UnitCode01),
                            UnitName01 = q.Max(i => i.UnitName01),
                            UnitCode02 = q.Max(i => i.UnitCode02),
                            UnitName02 = q.Max(i => i.UnitName02),
                            Count01 = q.Max(i => i.Count01),
                            Count02 = q.Max(i => i.Count02),
                            Beginning = q.Sum(i => i.Beginning),
                            EntryAmount = q.Sum(i => i.EntryAmount),
                            DeliveryAmount = q.Sum(i => i.DeliveryAmount),
                            ProfitAmount = q.Sum(i => i.ProfitAmount),
                            LossAmount = q.Sum(i => i.LossAmount),
                            Ending = q.Sum(i => i.Ending),
                            BSys_Beginning = q.Sum(i => i.BSys_Beginning),
                            BSys_EntryAmount = q.Sum(i => i.BSys_EntryAmount),
                            BSys_DeliveryAmount = q.Sum(i => i.BSys_DeliveryAmount),
                            BSys_ProfitAmount = q.Sum(i => i.BSys_ProfitAmount),
                            BSys_LossAmount = q.Sum(i => i.BSys_LossAmount),
                            BSys_Ending = q.Sum(i => i.BSys_Ending),
                        }).Select(n => new //两个系统产品日结结果比对
                        {
                            n.SettleDate,
                            n.WarehouseCode,
                            n.ProductCode,
                            n.ProductName,
                            n.WarehouseName,
                            n.UnitCode01,
                            n.UnitName01,
                            n.UnitCode02,
                            n.UnitName02,
                            n.Count01,
                            n.Count02,
                            n.Beginning,
                            n.EntryAmount,
                            n.DeliveryAmount,
                            n.ProfitAmount,
                            n.LossAmount,
                            n.Ending,
                            n.BSys_Beginning,
                            n.BSys_EntryAmount,
                            n.BSys_DeliveryAmount,
                            n.BSys_ProfitAmount,
                            n.BSys_LossAmount,
                            n.BSys_Ending,
                            Status = n.Beginning == n.BSys_Beginning &&
                                        n.EntryAmount == n.BSys_EntryAmount &&
                                        n.DeliveryAmount == n.BSys_DeliveryAmount &&
                                        n.ProfitAmount == n.BSys_ProfitAmount &&
                                        n.LossAmount == n.BSys_LossAmount &&
                                        n.Ending == n.BSys_Ending ?
                                        "T" : "F",
                            Status1 = n.Beginning == n.BSys_Beginning ?
                                "T" : "F",
                            Status2 = n.EntryAmount == n.BSys_EntryAmount ?
                                "T" : "F",
                            Status3 = n.DeliveryAmount == n.BSys_DeliveryAmount ?
                                "T" : "F",
                            Status4 = n.ProfitAmount == n.BSys_ProfitAmount ?
                                "T" : "F",
                            Status5 = n.LossAmount == n.BSys_LossAmount ?
                                "T" : "F",
                            Status6 = n.Ending == n.BSys_Ending ?
                                "true" : "F"
                        }).OrderBy(n => n.Status)
                          .OrderBy(n => n.WarehouseName)
                          .OrderBy(n => n.ProductCode);
            //两个系统产品日结结果比对结果按照单位换算
            string unitName = "";
            decimal count = 1;

            //标准单位（标准件||标准条）
            if (unitType == "1" || unitType == "2")
            {
                if (unitType == "1")
                {
                    unitName = "标准件";
                    count = 10000;
                }

                if (unitType == "2")
                {
                    unitName = "标准条";
                    count = 200;
                }
                var dailyBalance = InfoCheck.ToArray().Select(i => new
                {
                    SettleDate = i.SettleDate.ToString("yyyy-MM-dd"),
                    i.ProductCode,
                    i.ProductName,
                    UnitCode = "",
                    UnitName = unitName,
                    i.WarehouseCode,
                    i.WarehouseName,
                    Beginning = i.Beginning == i.BSys_Beginning ?
                    (i.Beginning / count).ToString() :
                    (i.Beginning / count).ToString() + "≠" + (i.BSys_Beginning / count).ToString(),
                    EntryAmount = i.EntryAmount == i.BSys_EntryAmount ?
                    (i.EntryAmount / count).ToString() :
                    (i.EntryAmount / count).ToString() + "≠" + (i.BSys_EntryAmount / count).ToString(),
                    DeliveryAmount = i.DeliveryAmount == i.BSys_DeliveryAmount ?
                    (i.DeliveryAmount / count).ToString() :
                    (i.DeliveryAmount / count).ToString() + "≠" + (i.BSys_DeliveryAmount / count).ToString(),
                    ProfitAmount = i.ProfitAmount == i.BSys_ProfitAmount ?
                    (i.ProfitAmount / count).ToString() :
                    (i.ProfitAmount / count).ToString() + "≠" + (i.BSys_ProfitAmount / count).ToString(),
                    LossAmount = i.LossAmount == i.BSys_LossAmount ?
                    (i.LossAmount / count).ToString() :
                    (i.LossAmount / count).ToString() + "≠" + (i.BSys_LossAmount / count).ToString(),
                    Ending = i.Ending == i.BSys_Ending ?
                    (i.Ending / count).ToString() :
                    (i.Ending / count).ToString() + "≠" + (i.BSys_Ending / count).ToString(),
                    i.Status1,
                    i.Status2,
                    i.Status3,
                    i.Status4,
                    i.Status5,
                    i.Status6
                });
                dt.Columns.Add("结算日期", typeof(string));
                dt.Columns.Add("仓库名称", typeof(string));
                dt.Columns.Add("商品代码", typeof(string));
                dt.Columns.Add("商品名称", typeof(string));
                dt.Columns.Add("单位", typeof(string));
                dt.Columns.Add("期初量(本/销)", typeof(string));
                dt.Columns.Add("入库量(本/销)", typeof(string));
                dt.Columns.Add("出库量(本/销)", typeof(string));
                dt.Columns.Add("报损量(本/销)", typeof(string));
                dt.Columns.Add("报益量(本/销)", typeof(string));
                dt.Columns.Add("结余量(本/销)", typeof(string));
                foreach (var item in dailyBalance)
                {
                    dt.Rows.Add
                        (
                            item.SettleDate,
                            item.WarehouseName,
                            item.ProductCode,
                            item.ProductName,
                            item.UnitName,
                            item.Beginning,
                            item.EntryAmount,
                            item.DeliveryAmount,
                            item.ProfitAmount,
                            item.LossAmount,
                            item.Ending
                        );
                }
                return dt;
            }

            //自然件
            if (unitType == "3" || unitType == "4")
            {
                var dailyBalance = InfoCheck.ToArray().Select(i => new
                {
                    SettleDate = i.SettleDate.ToString("yyyy-MM-dd"),
                    i.ProductCode,
                    i.ProductName,
                    UnitCode = unitType == "3" ? i.UnitCode01 : i.UnitCode02,
                    UnitName = unitType == "3" ? i.UnitName01 : i.UnitName02,
                    i.WarehouseCode,
                    i.WarehouseName,
                    Beginning = i.Beginning == i.BSys_Beginning ?
                    (i.Beginning / (unitType == "3" ? i.Count01 : i.Count02)).ToString() :
                    (i.Beginning / (unitType == "3" ? i.Count01 : i.Count02)).ToString() + "≠" + (i.BSys_Beginning / (unitType == "3" ? i.Count01 : i.Count02)).ToString(),
                    EntryAmount = i.EntryAmount == i.BSys_EntryAmount ?
                    (i.EntryAmount / (unitType == "3" ? i.Count01 : i.Count02)).ToString() :
                    (i.EntryAmount / (unitType == "3" ? i.Count01 : i.Count02)).ToString() + "≠" + (i.BSys_EntryAmount / (unitType == "3" ? i.Count01 : i.Count02)).ToString(),
                    DeliveryAmount = i.DeliveryAmount == i.BSys_DeliveryAmount ?
                    (i.DeliveryAmount / (unitType == "3" ? i.Count01 : i.Count02)).ToString() :
                    (i.DeliveryAmount / (unitType == "3" ? i.Count01 : i.Count02)).ToString() + "≠" + (i.BSys_DeliveryAmount / (unitType == "3" ? i.Count01 : i.Count02)).ToString(),
                    ProfitAmount = i.ProfitAmount == i.BSys_ProfitAmount ?
                    (i.ProfitAmount / (unitType == "3" ? i.Count01 : i.Count02)).ToString() :
                    (i.ProfitAmount / (unitType == "3" ? i.Count01 : i.Count02)).ToString() + "≠" + (i.BSys_ProfitAmount / (unitType == "3" ? i.Count01 : i.Count02)).ToString(),
                    LossAmount = i.LossAmount == i.BSys_LossAmount ?
                    (i.LossAmount / (unitType == "3" ? i.Count01 : i.Count02)).ToString() :
                    (i.LossAmount / (unitType == "3" ? i.Count01 : i.Count02)).ToString() + "≠" + (i.BSys_LossAmount / (unitType == "3" ? i.Count01 : i.Count02)).ToString(),
                    Ending = i.Ending == i.BSys_Ending ?
                    (i.Ending / (unitType == "3" ? i.Count01 : i.Count02)).ToString() :
                    (i.Ending / (unitType == "3" ? i.Count01 : i.Count02)).ToString() + "≠" + (i.BSys_Ending / (unitType == "3" ? i.Count01 : i.Count02)).ToString(),
                    i.Status1,
                    i.Status2,
                    i.Status3,
                    i.Status4,
                    i.Status5,
                    i.Status6
                });
                dt.Columns.Add("结算日期", typeof(string));
                dt.Columns.Add("仓库名称", typeof(string));
                dt.Columns.Add("商品代码", typeof(string));
                dt.Columns.Add("商品名称", typeof(string));
                dt.Columns.Add("单位", typeof(string));
                dt.Columns.Add("期初量(本/销)", typeof(string));
                dt.Columns.Add("入库量(本/销)", typeof(string));
                dt.Columns.Add("出库量(本/销)", typeof(string));
                dt.Columns.Add("报损量(本/销)", typeof(string));
                dt.Columns.Add("报益量(本/销)", typeof(string));
                dt.Columns.Add("结余量(本/销)", typeof(string));
                foreach (var item in dailyBalance)
                {
                    dt.Rows.Add
                        (
                            item.SettleDate,
                            item.WarehouseName,
                            item.ProductCode,
                            item.ProductName,
                            item.UnitName,
                            item.Beginning,
                            item.EntryAmount,
                            item.DeliveryAmount,
                            item.ProfitAmount,
                            item.LossAmount,
                            item.Ending
                        );
                }
                return dt;
            }
            return dt;
        }
        #endregion
    }
}
