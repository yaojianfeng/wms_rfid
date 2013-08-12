using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using THOK.Wms.DbModel;
using THOK.Wms.Bll.Interfaces;
using Microsoft.Practices.Unity;
using THOK.Wms.Dal.Interfaces;

namespace THOK.Wms.Bll.Service
{
    public class SortingLowerlimitService : ServiceBase<SortingLowerlimit>, ISortingLowerlimitService
    {
        [Dependency]
        public ISortingLowerlimitRepository SortingLowerlimitRepository { get; set; }

        [Dependency]
        public IUnitRepository UnitRepository { get; set; }

        [Dependency]
        public IProductRepository ProductRepository { get; set; }

        [Dependency]
        public IStorageRepository StorageRepository { get; set; }

        protected override Type LogPrefix
        {
            get { return this.GetType(); }
        }

        #region ISortingLowerlimitService 成员

        public string GetSortType(string type,string stype)
        {
            string typeStr = "";
            if (stype == "3")
            {
                typeStr = "通道机";
                return typeStr;
            }
            else
            {
                switch (type)
                {
                    case "0":
                        typeStr = "取整件";
                        break;
                    case "1":
                        typeStr = "不取整";
                        break;
                    case "2":
                        typeStr = "整托盘";
                        break;
                }
                return typeStr;
            }
        }

        public object GetDetails(int page, int rows, string sortingLineCode, string sortingLineName, string productName, string productCode, string IsActive)
        {
            IQueryable<SortingLowerlimit> lowerLimitQuery = SortingLowerlimitRepository.GetQueryable();
            IQueryable<Storage> storageQuery = StorageRepository.GetQueryable();

            var lowerLimit = lowerLimitQuery.OrderBy(b => new { b.SortingLineCode,b.ProductCode }).Where(s => s.SortingLineCode == s.SortingLineCode);
            if (sortingLineCode != string.Empty && sortingLineCode != null)
            {
                lowerLimit = lowerLimit.Where(l => l.SortingLineCode.Contains(sortingLineCode));
            }
            if (sortingLineName != string.Empty && sortingLineName != null)
            {
                lowerLimit = lowerLimit.Where(l => l.SortingLine.SortingLineName.Contains(sortingLineName));
            }
            if (productCode != string.Empty && productCode != null)
            {
                lowerLimit = lowerLimit.Where(l => l.ProductCode.Contains(productCode));
            }
            if (productName != string.Empty && productName != null)
            {
                lowerLimit = lowerLimit.Where(l => l.Product.ProductName.Contains(productName));
            }
            if (IsActive != string.Empty && IsActive != null)
            {
                lowerLimit = lowerLimit.Where(l => l.IsActive == IsActive);
            }
            int total = lowerLimit.Count();
            lowerLimit = lowerLimit.OrderBy(r => r.SortingLineCode).ThenBy(r=>r.SortOrder);
            lowerLimit = lowerLimit.Skip((page - 1) * rows).Take(rows);

            var temp1 = lowerLimit.GroupJoin(storageQuery,
                            l => new { l.SortingLine.CellCode, l.ProductCode },
                            s => new { s.CellCode, s.ProductCode },
                            (l, s) => new
                            {
                                l.ID,
                                l.SortingLineCode,
                                l.SortingLine.SortingLineName,
                                l.ProductCode,
                                l.Product.ProductName,
                                l.UnitCode,
                                l.Unit.UnitName,
                                l.Unit,
                                l.Quantity,
                                StorageQuantity = (decimal?)s.Sum(r => (decimal?)r.Quantity ?? 0) ?? 0,
                                l.SortOrder,
                                l.SortType,
                                l.Product.IsRounding,
                                l.IsActive,
                                l.UpdateTime
                            });

            var temp2 = temp1.ToArray().AsEnumerable().Select(b => new
            {
                b.ID,
                b.SortingLineCode,
                b.SortingLineName,
                b.ProductCode,
                b.ProductName,
                b.UnitCode,
                b.UnitName,
                Quantity = b.Quantity / b.Unit.Count,
                StorageQuantity = b.StorageQuantity / b.Unit.Count,
                b.SortOrder,
                IsRounding = GetSortType(b.IsRounding,b.SortType),
                IsActive = b.IsActive == "1" ? "可用" : "不可用",
                UpdateTime = b.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss")
            });

            return new { total, rows = temp2.ToArray() };
        }

        public new bool Add(SortingLowerlimit sortLowerLimit)
        {
            var lowerLimit = new SortingLowerlimit();
            var unit = UnitRepository.GetQueryable().FirstOrDefault(u => u.UnitCode == sortLowerLimit.UnitCode);
            var productAdd = ProductRepository.GetQueryable().FirstOrDefault(p => p.ProductCode == sortLowerLimit.ProductCode);
            var lowerLimitList = SortingLowerlimitRepository.GetQueryable().FirstOrDefault(l => l.ProductCode == sortLowerLimit.ProductCode && l.SortingLineCode == sortLowerLimit.SortingLineCode);
            if (lowerLimitList == null)
            {
                lowerLimit.SortingLineCode = sortLowerLimit.SortingLineCode;
                lowerLimit.ProductCode = sortLowerLimit.ProductCode;
                lowerLimit.UnitCode = sortLowerLimit.UnitCode;
                lowerLimit.Quantity = sortLowerLimit.Quantity * unit.Count;              
                lowerLimit.SortOrder = sortLowerLimit.SortOrder;
                if (sortLowerLimit.SortType == "3")
                {
                    lowerLimit.SortType = sortLowerLimit.SortType;
                }
                else
                {
                    productAdd.IsRounding = sortLowerLimit.SortType;
                    lowerLimit.SortType = "";
                }
                lowerLimit.IsActive = sortLowerLimit.IsActive;
                lowerLimit.UpdateTime = DateTime.Now;

                SortingLowerlimitRepository.Add(lowerLimit);
                SortingLowerlimitRepository.SaveChanges();
            }
            else
            {
                lowerLimitList.Quantity = lowerLimitList.Quantity + (sortLowerLimit.Quantity * unit.Count);
                lowerLimitList.SortOrder = sortLowerLimit.SortOrder;
                if (sortLowerLimit.SortType == "3")
                {
                    lowerLimitList.SortType = sortLowerLimit.SortType;
                }
                else
                {
                    productAdd.IsRounding = sortLowerLimit.SortType;
                    lowerLimitList.SortType = "";
                }
                lowerLimitList.UpdateTime = DateTime.Now;
                SortingLowerlimitRepository.SaveChanges();
            }
            return true;
        }

        public bool Delete(string id)
        {
            var lowerLimit = SortingLowerlimitRepository.GetQueryable().ToArray().AsEnumerable().Where(s => id.Contains(s.ID.ToString()));
            if (lowerLimit.Count()>0)
            {
                foreach (var item in lowerLimit)
                {
                    SortingLowerlimitRepository.Delete(item);
                }
                SortingLowerlimitRepository.SaveChanges();
            }
            else
                return false;
            return true;
        }

        public bool Save(SortingLowerlimit sortLowerLimit)
        {
            var lowerLimitSave = SortingLowerlimitRepository.GetQueryable().FirstOrDefault(s => s.ID == sortLowerLimit.ID);
            var unit = UnitRepository.GetQueryable().FirstOrDefault(u => u.UnitCode == sortLowerLimit.UnitCode);
            var productSave = ProductRepository.GetQueryable().FirstOrDefault(p => p.ProductCode == sortLowerLimit.ProductCode);
            lowerLimitSave.SortingLineCode = sortLowerLimit.SortingLineCode;
            lowerLimitSave.ProductCode = sortLowerLimit.ProductCode;
            lowerLimitSave.UnitCode = sortLowerLimit.UnitCode;
            lowerLimitSave.Quantity = sortLowerLimit.Quantity * unit.Count;
            lowerLimitSave.SortOrder = sortLowerLimit.SortOrder;
            if (sortLowerLimit.SortType == "3")
            {
                lowerLimitSave.SortType = sortLowerLimit.SortType;                
            }
            else
            {
                productSave.IsRounding = sortLowerLimit.SortType;
                lowerLimitSave.SortType = "";
            }
            lowerLimitSave.IsActive = sortLowerLimit.IsActive;
            lowerLimitSave.UpdateTime = DateTime.Now;

            SortingLowerlimitRepository.SaveChanges();
            return true;
        }

        #endregion

        public System.Data.DataTable GetSortingLowerlimit(int page, int rows, string sortingLineCode, string sortingLineName, string productName, string productCode, string IsActive)
        {
            IQueryable<SortingLowerlimit> lowerLimitQuery = SortingLowerlimitRepository.GetQueryable();
            IQueryable<Storage> storageQuery = StorageRepository.GetQueryable();

            var lowerLimit = lowerLimitQuery.OrderBy(b => new { b.SortingLineCode, b.ProductCode }).Where(s => s.SortingLineCode == s.SortingLineCode);
            if (sortingLineCode != string.Empty && sortingLineCode != null)
            {
                lowerLimit = lowerLimit.Where(l => l.SortingLineCode.Contains(sortingLineCode));
            }
            if (sortingLineName != string.Empty && sortingLineName != null)
            {
                lowerLimit = lowerLimit.Where(l => l.SortingLine.SortingLineName.Contains(sortingLineName));
            }
            if (productCode != string.Empty && productCode != null)
            {
                lowerLimit = lowerLimit.Where(l => l.ProductCode.Contains(productCode));
            }
            if (productName != string.Empty && productName != null)
            {
                lowerLimit = lowerLimit.Where(l => l.Product.ProductName.Contains(productName));
            }
            if (IsActive != string.Empty && IsActive != null)
            {
                lowerLimit = lowerLimit.Where(l => l.IsActive == IsActive);
            }
            //lowerLimit = lowerLimit
            var temp1 = lowerLimit.GroupJoin(storageQuery,
                            l => new { l.SortingLine.CellCode, l.ProductCode },
                            s => new { s.CellCode, s.ProductCode },
                            (l, s) => new
                            {
                                l.ID,
                                l.SortingLineCode,
                                l.SortingLine.SortingLineName,
                                l.ProductCode,
                                l.Product.ProductName,
                                l.UnitCode,
                                l.Unit.UnitName,
                                l.Unit,
                                l.Product.UnitList,
                                l.Quantity,                               
                                StorageQuantity = (decimal?)s.Sum(r => (decimal?)r.Quantity ?? 0) ?? 0,                                
                                l.SortOrder,
                                l.IsActive,
                                l.UpdateTime
                            });

            var temp2 = temp1.ToArray().AsEnumerable().Select(b => new
            {
                b.ID,
                b.SortingLineCode,
                b.SortingLineName,
                b.ProductCode,
                b.ProductName,
                b.UnitCode,
                b.UnitName,
                Quantity = b.Quantity / b.Unit.Count,
                StorageQuantity = b.StorageQuantity / b.Unit.Count,
                StorageBarQuantity = b.StorageQuantity / (b.UnitList.Quantity02*b.UnitList.Quantity03),
                b.SortOrder,
                IsActive = b.IsActive == "1" ? "可用" : "不可用",
                UpdateTime = b.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss")
            }).OrderBy(r => r.SortingLineCode).ThenBy(r => r.SortOrder); 

            System.Data.DataTable dt = new System.Data.DataTable();
            //dt.Columns.Add("分拣线编码", typeof(string));
            dt.Columns.Add("分拣线名称", typeof(string));
            //dt.Columns.Add("卷烟编码", typeof(string));
            dt.Columns.Add("仓位顺序", typeof(string));
            dt.Columns.Add("卷烟名称", typeof(string));
            //dt.Columns.Add("单位编码", typeof(string));
            //dt.Columns.Add("单位名称", typeof(string));
            dt.Columns.Add("数量(件)", typeof(decimal));
            dt.Columns.Add("数量(条)", typeof(decimal));
            dt.Columns.Add("下限数量", typeof(decimal));            
            //dt.Columns.Add("是否可用", typeof(string));
            //dt.Columns.Add("修改时间", typeof(string));
            foreach (var t in temp2)
            {
                dt.Rows.Add
                    (
                        //t.SortingLineCode,
                        t.SortingLineName,
                        //t.ProductCode,
                        t.SortOrder,
                        t.ProductName,
                        //t.UnitCode,
                        //t.UnitName,
                        t.StorageQuantity,
                        t.StorageBarQuantity,
                        t.Quantity
                       
                        //t.IsActive,
                        //t.UpdateTime
                    );
            }
            return dt;
        }
    }
}
