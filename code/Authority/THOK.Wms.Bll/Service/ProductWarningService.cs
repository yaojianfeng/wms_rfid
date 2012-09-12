﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using THOK.Wms.DbModel;
using THOK.Wms.Bll.Interfaces;
using Microsoft.Practices.Unity;
using THOK.Wms.Dal.Interfaces;

namespace THOK.Wms.Bll.Service
{
     public class ProductWarningService:ServiceBase<ProductWarning>,IProductWarningService
    {
         [Dependency]
         public IProductWarningRepository ProductWarningRepository { get; set; }
         [Dependency]
         public IStorageRepository StorageRepository { get; set; }
         [Dependency]
         public IProductRepository ProductRepository { get; set; }
         [Dependency]
         public IUnitRepository UnitRepository { get; set; }
         [Dependency]
         public IDailyBalanceRepository DailyBalanceRepository { get; set; }

         protected override Type LogPrefix
         {
             get { return this.GetType(); }
         }
        #region  卷烟预警设置的增，删，改，查方法
         public object GetDetail(int page, int rows, string productCode, decimal minLimited, decimal maxLimited, decimal assemblyTime)
         {
             IQueryable<ProductWarning> productWarningQuery = ProductWarningRepository.GetQueryable();
             var productWarning = productWarningQuery.Where(p => p.ProductCode.Contains(productCode)).OrderBy(p => p.ProductCode).Select(p => p).ToArray();
             if (productCode != "")
             {
                 productWarning = productWarning.Where(p => p.ProductCode == productCode).ToArray();
             }
             if (minLimited != 100000)
             {
                 productWarning = productWarning.Where(p => p.MinLimited <= minLimited).ToArray();
             }
             if (maxLimited != 100000)
             {
                 productWarning = productWarning.Where(p => p.MaxLimited >= maxLimited).ToArray();
             }
             if (assemblyTime != 3600)
             {
                 productWarning = productWarning.Where(p => p.AssemblyTime >= assemblyTime).ToArray();
             }
             var productWarn = productWarning
                 .Select(p => new
                 {
                     p.ProductCode,
                     ProductName = ProductRepository.GetQueryable().FirstOrDefault(q => q.ProductCode==p.ProductCode).ProductName,
                     p.UnitCode,
                     p.Unit.UnitName,
                     MinLimited = p.MinLimited / p.Unit.Count,
                     MaxLimited = p.MaxLimited / p.Unit.Count,
                     p.AssemblyTime,
                     p.Memo
                 });
             int total = productWarn.Count();
             productWarn = productWarn.Skip((page - 1) * rows).Take(rows);
             return new { total, rows = productWarn.ToArray() };
         }

         public bool Add(ProductWarning productWarning)
         {
             var unit = UnitRepository.GetQueryable().FirstOrDefault(u => u.UnitCode ==productWarning.UnitCode);
             var productWarn = new ProductWarning();
             productWarn.ProductCode = productWarning.ProductCode;
             productWarn.UnitCode = productWarning.UnitCode;
             productWarn.MinLimited = productWarning.MinLimited * unit.Count;
             productWarn.MaxLimited = productWarning.MaxLimited * unit.Count;
             productWarn.AssemblyTime = productWarning.AssemblyTime;
             productWarn.Memo = productWarning.Memo;

             ProductWarningRepository.Add(productWarn);
             ProductWarningRepository.SaveChanges();
             return true;
         }

         public bool Delete(string productCode)
         {
             var productWarn = ProductWarningRepository.GetQueryable()
                 .FirstOrDefault(p => p.ProductCode == productCode);

             if (productWarn != null)
             {
                 ProductWarningRepository.Delete(productWarn);
                 ProductWarningRepository.SaveChanges();
             }
             else 
             {
                 return false;
             }
             return true;
         }

         public bool Save(ProductWarning productWarning)
         {
             var productWarn = ProductWarningRepository.GetQueryable()
                 .FirstOrDefault(p => p.ProductCode == productWarning.ProductCode);
             var unit = UnitRepository.GetQueryable().FirstOrDefault(u => u.UnitCode == productWarning.UnitCode);
             productWarn.ProductCode = productWarning.ProductCode;
             productWarn.UnitCode = productWarning.UnitCode;
             productWarn.MinLimited = productWarning.MinLimited * unit.Count;
             productWarn.MaxLimited = productWarning.MaxLimited * unit.Count;
             productWarn.AssemblyTime = productWarning.AssemblyTime;
             productWarn.Memo = productWarning.Memo;

             ProductWarningRepository.SaveChanges();
             return true;
         }
        #endregion

        #region 产品短缺、超储查询
         public object GetQtyLimitsDetail(int page, int rows, string productCode, decimal minLimited, decimal maxLimited, string unitCode)
         {
             IQueryable<ProductWarning> ProductWarningQuery = ProductWarningRepository.GetQueryable();
             var unit = UnitRepository.GetQueryable().FirstOrDefault(u => u.UnitCode == unitCode);
             var ProductWarning = ProductWarningQuery.Where(p => p.ProductCode.Contains(productCode)).ToArray(); 
             if(productCode!="")
             {
                 ProductWarning = ProductWarning.Where(p=>p.ProductCode == productCode).ToArray();
             }
              if (minLimited != 100000)
             {
                 ProductWarning = ProductWarning.Where(p=>p.MinLimited<=minLimited * unit.Count).ToArray();
             }
             if (maxLimited != 100000)
             {
                 ProductWarning = ProductWarning.Where(p => p.MaxLimited >= maxLimited * unit.Count).ToArray();
             }
             var priductWarning = ProductWarning.Select(t => new 
                 {
                     ProductCode =t.ProductCode,
                     ProductName = ProductRepository.GetQueryable().FirstOrDefault(q => q.ProductCode == t.ProductCode).ProductName,
                     UnitCode=t.UnitCode,
                     UnitName = t.Unit.UnitName,
                     Quantity = StorageRepository.GetQueryable().AsEnumerable().Where(s=>s.ProductCode==t.ProductCode).Sum(s=>s.Quantity)/t.Unit.Count,
                     MinLimited =t.MinLimited/t.Unit.Count,
                     MaxLimited = t.MaxLimited / t.Unit.Count
                 });
             int total = priductWarning.Count();
             priductWarning = priductWarning.OrderBy(q => q.ProductCode);
             priductWarning = priductWarning.Skip((page - 1) * rows).Take(rows);
             return new { total, rows = priductWarning.ToArray() };
         }
        #endregion

        #region 积压产品查询
         public object GetProductDetails(int page, int rows, string productCode, decimal assemblyTime)
         {
             IQueryable<Storage> StorageQuery = StorageRepository.GetQueryable();
             IQueryable<ProductWarning> ProductWarningQuery = ProductWarningRepository.GetQueryable();
             var ProductWarning = ProductWarningQuery.Where(p => p.ProductCode.Contains(productCode));
             var storage = StorageQuery.Where(s => s.ProductCode.Contains(productCode));
             var Storages = storage.Join(ProductWarning, s => s.ProductCode, p => p.ProductCode, (s, p) => new { storage = s, ProductWarning = p }).ToArray();

             Storages = Storages.Where(s => string.IsNullOrEmpty(s.ProductWarning.AssemblyTime.ToString())).ToArray();

             if (productCode != "")
             {
                 Storages = Storages.Where(s => s.storage.ProductCode == productCode).ToArray();
             }
             if (assemblyTime != 360)
             {
                 Storages = Storages.Where(s => s.ProductWarning.AssemblyTime >= assemblyTime).ToArray();
             }
             var ProductTimeOut = Storages.AsEnumerable()
                 .Select(s => new
                 {
                     ProductCode = s.storage.ProductCode,
                     ProductName = s.storage.Product.ProductName,
                     cellCode = s.storage.CellCode,
                     cellName = s.storage.Cell.CellName,
                     quantity = s.storage.Quantity / s.storage.Product.Unit.Count,
                     storageTime = s.storage.StorageTime.ToString("yyyy-MM-dd hh:mm:ss"),
                     days = (DateTime.Now - s.storage.StorageTime).Days
                 });
             int total = ProductTimeOut.Count();
             ProductTimeOut = ProductTimeOut.OrderBy(p => p.ProductCode);
             ProductTimeOut = ProductTimeOut.Skip((page - 1) * rows).Take(rows);
             return new { total, rows = ProductTimeOut.ToArray() };
         }
        #endregion

        #region 库存分析数据
         public object GetStorageByTime()
         {
             IQueryable<DailyBalance> EndQuantity = DailyBalanceRepository.GetQueryable();
             var storageQuantity = EndQuantity.GroupBy(e=>e.SettleDate).Select(e => new 
             {
                TimeInter=e.Max(m=>m.SettleDate).Date,
                TotalQuantity=e.Sum(s=>s.Ending)
             });
             return storageQuantity.ToArray();
         }
        #endregion

    }
}
