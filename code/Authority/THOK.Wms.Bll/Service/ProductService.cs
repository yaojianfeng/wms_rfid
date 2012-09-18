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
    public class ProductService:ServiceBase<Product>,IProductService
    {
        [Dependency]
        public IProductRepository ProductRepository { get; set; }

        [Dependency]
        public IStorageRepository StorageRepository { get; set; }

        protected override Type LogPrefix
        {
            get { return this.GetType(); }
        }

        #region IProductService 增，删，改，查等方法

        public object GetDetails(int page, int rows, string ProductName, string ProductCode, string CustomCode, string BrandCode, string UniformCode, string AbcTypeCode, string ShortCode, string PriceLevelCode, string SupplierCode)
        {
            IQueryable<Product> ProductQuery = ProductRepository.GetQueryable();
            var product = ProductQuery.Where(c => c.ProductName.Contains(ProductName)
                && c.ProductCode.Contains(ProductCode)
                && c.BrandCode.Contains(BrandCode)
                && c.UniformCode.Contains(UniformCode)
                && c.SupplierCode.Contains(SupplierCode))
                .OrderBy(c => c.ProductCode)
                .Select(c => c);
            if (!CustomCode.Equals(string.Empty))
            {
                product = product.Where(p => p.CustomCode == CustomCode);
            }
            if (!AbcTypeCode.Equals(string.Empty))
            {
                product = product.Where(p => p.AbcTypeCode == AbcTypeCode);
            }
            if (!UniformCode.Equals(string.Empty))
            {
                product = product.Where(p => p.UniformCode == UniformCode);
            }
            if (!PriceLevelCode.Equals(string.Empty))
            {
                product = product.Where(p => p.PriceLevelCode == PriceLevelCode);
            }
            int total = product.Count();
            product = product.Skip((page - 1) * rows).Take(rows);

            var temp = product.ToArray().Select(c => new
            {
                c.AbcTypeCode,
                c.BarBarcode,
                c.BelongRegion,
                c.BrandCode,
                c.BuyPrice,
                c.CostPrice,
                c.CustomCode,
                c.Description,
                IsAbnormity = c.IsAbnormity == "1" ? "是" : "不是",
                IsActive = c.IsActive == "1" ? "可用" : "不可用",
                c.IsConfiscate,
                c.IsFamous,
                c.IsFilterTip,
                c.IsMainProduct,
                c.IsNew,
                c.IsProvinceMainProduct,
                c.OneProjectBarcode,
                c.PackageBarcode,
                c.PackTypeCode,
                c.PieceBarcode,
                c.PriceLevelCode,
                c.ProductCode,
                c.ProductName,
                c.ProductTypeCode,
                c.RetailPrice,
                c.ShortCode,
                c.StatisticType,
                c.SupplierCode,
                c.TradePrice,
                c.UniformCode,
                c.UnitCode,
                c.Unit.UnitName,
                c.UnitListCode,
                UpdateTime = c.UpdateTime.ToString("yyyy-MM-dd hh:mm:ss")
            });
            return new { total, rows = temp.ToArray() };
        }
        public bool Add(Product product)
        {
            var prod = new Product();
            prod.AbcTypeCode = product.AbcTypeCode;
            prod.BarBarcode = product.BarBarcode;
            prod.BelongRegion = product.BelongRegion;
            prod.BrandCode = product.BrandCode;

            prod.BuyPrice = product.BuyPrice;
            prod.CostPrice = product.CostPrice;
            prod.CustomCode = product.CustomCode;
            prod.Description = product.Description;
            prod.IsAbnormity = product.IsAbnormity;
            prod.IsActive = product.IsActive;
            prod.IsConfiscate = product.IsConfiscate;
            prod.IsFamous = product.IsFamous;
            prod.IsFilterTip = product.IsFilterTip;
            prod.IsMainProduct = product.IsMainProduct;
            prod.IsNew = product.IsNew;
            prod.IsProvinceMainProduct = product.IsProvinceMainProduct;
            prod.OneProjectBarcode = product.OneProjectBarcode;
            prod.PackageBarcode = product.PackageBarcode;
            prod.PackTypeCode = product.PackTypeCode;
            prod.PieceBarcode = product.PieceBarcode;
            prod.PriceLevelCode = product.PriceLevelCode;
            prod.ProductCode = product.ProductCode;
            prod.ProductName = product.ProductName;
            prod.ProductTypeCode = product.ProductTypeCode;
            prod.RetailPrice = product.RetailPrice;
            prod.ShortCode = product.ShortCode;
            prod.StatisticType = product.StatisticType;
            prod.SupplierCode = product.SupplierCode;
            prod.TradePrice = product.TradePrice;
            prod.UniformCode = product.UniformCode;
            prod.UnitCode = product.UnitCode;
            prod.UnitListCode = product.UnitListCode;
            prod.UpdateTime = DateTime.Now;

            ProductRepository.Add(prod);
            ProductRepository.SaveChanges();
            return true;
        }
        public bool Delete(string ProductCode)
        {
            var product = ProductRepository.GetQueryable()
                .FirstOrDefault(b => b.ProductCode == ProductCode);
            if (ProductCode != null)
            {
                ProductRepository.Delete(product);
                ProductRepository.SaveChanges();
            }
            else
                return false;
            return true;
        }
        public bool Save(Product product)
        {
            var prod = ProductRepository.GetQueryable().FirstOrDefault(b => b.ProductCode == product.ProductCode);
            prod.AbcTypeCode = product.AbcTypeCode;
            prod.BarBarcode = product.BarBarcode;
            prod.BelongRegion = product.BelongRegion;
            prod.BrandCode = product.BrandCode;
            prod.BuyPrice = product.BuyPrice;
            prod.CostPrice = product.CostPrice;
            prod.CustomCode = product.CustomCode;
            prod.Description = product.Description;
            prod.IsAbnormity = product.IsAbnormity;
            prod.IsActive = product.IsActive;
            prod.IsConfiscate = product.IsConfiscate;
            prod.IsFamous = product.IsFamous;
            prod.IsFilterTip = product.IsFilterTip;
            prod.IsMainProduct = product.IsMainProduct;
            prod.IsNew = product.IsNew;
            prod.IsProvinceMainProduct = product.IsProvinceMainProduct;
            prod.OneProjectBarcode = product.OneProjectBarcode;
            prod.PackageBarcode = product.PackageBarcode;
            prod.PackTypeCode = product.PackTypeCode;
            prod.PieceBarcode = product.PieceBarcode;
            prod.PriceLevelCode = product.PriceLevelCode;
            prod.ProductCode = product.ProductCode;
            prod.ProductName = product.ProductName;
            prod.ProductTypeCode = product.ProductTypeCode;
            prod.RetailPrice = product.RetailPrice;
            prod.ShortCode = product.ShortCode;
            prod.StatisticType = product.StatisticType;
            prod.SupplierCode = product.SupplierCode;
            prod.TradePrice = product.TradePrice;
            prod.UniformCode = product.UniformCode;
            prod.UnitCode = product.UnitCode;
            prod.UnitListCode = product.UnitListCode;
            prod.UpdateTime = DateTime.Now;

            ProductRepository.SaveChanges();
            return true;
        }
        #endregion

        #region IProductService 成员

        /// <summary>
        /// 查询卷烟信息 zxl 2012年7月24日 16:26:24
        /// </summary>
        /// <returns></returns>
        public object FindProduct(int page, int rows, string QueryString, string value)
        {
            IQueryable<Product> ProductQuery = ProductRepository.GetQueryable();
            var product = ProductQuery.OrderBy(p => p.ProductCode).Where(p => p.ProductCode == p.ProductCode);

            if (QueryString == "ProductCode")
            {
                var storages = StorageRepository.GetQueryable().OrderBy(s => s.ProductCode).Where(s => s.ProductCode == value).GroupBy(s=>s.ProductCode).Select(s => s.Key);
                product = product.Where(p => storages.Any(s => s == p.ProductCode));
            }
            else if (QueryString == "ProductName")
            {
                var storages = StorageRepository.GetQueryable().OrderBy(s => s.ProductCode).Where(s => s.Product.ProductName.Contains(value)).GroupBy(s => s.ProductCode).Select(s => s.Key);
                product = product.Where(p => storages.Any(s => s == p.ProductCode));
            }
            if (QueryString == string.Empty || QueryString == null || value == string.Empty || value == null)
            {
                var storages = StorageRepository.GetQueryable().OrderBy(s => s.ProductCode).Where(s => s.Product != null).GroupBy(s => s.ProductCode).Select(s => s.Key);
                product = ProductQuery.Where(p => storages.Any(s => s == p.ProductCode));
            }
            var temp = product.OrderBy(p => p.ProductCode).Select(c=>c);
            int total = temp.Count();
            temp = temp.Skip((page - 1) * rows).Take(rows);

            var tmp = temp.ToArray().Select(c => new
            {
                c.AbcTypeCode,
                c.BarBarcode,
                c.BelongRegion,
                c.BrandCode,
                c.BuyPrice,
                c.CostPrice,
                c.CustomCode,
                c.Description,
                IsAbnormity = c.IsAbnormity == "1" ? "是" : "不是",
                IsActive = c.IsActive == "1" ? "可用" : "不可用",
                c.IsConfiscate,
                c.IsFamous,
                c.IsFilterTip,
                c.IsMainProduct,
                c.IsNew,
                c.IsProvinceMainProduct,
                c.OneProjectBarcode,
                c.PackageBarcode,
                c.PackTypeCode,
                c.PieceBarcode,
                c.PriceLevelCode,
                c.ProductCode,
                c.ProductName,
                c.ProductTypeCode,
                c.RetailPrice,
                c.ShortCode,
                c.StatisticType,
                c.SupplierCode,
                c.TradePrice,
                c.UniformCode,
                c.UnitCode,
                c.Unit.UnitName,
                c.UnitListCode,
                UpdateTime = c.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss")
            });
            return new { total, rows = tmp.ToArray() };
        }

        /// <summary>
        /// 产品盘点显示卷烟信息，入库新增显示卷烟数据
        /// </summary>
        /// <returns></returns>
        public object checkFindProduct()
        {
            IQueryable<Product> ProductQuery = ProductRepository.GetQueryable();
            IQueryable<Storage> StorageQuery = StorageRepository.GetQueryable();
            var storage = StorageQuery.Join(ProductQuery,
                                           s => s.ProductCode,
                                           p => p.ProductCode,
                                           (s, p) => new { p.ProductCode, p.ProductName, s.Quantity, s.Product, p.Unit, p.BuyPrice }
                                           ).GroupBy(s => new { s.ProductCode, s.ProductName, s.Unit, s.BuyPrice })
                                           .Select(s => new
                                           {
                                               ProductCode = s.Key.ProductCode,
                                               ProductName = s.Key.ProductName,
                                               UnitCode = s.Key.Unit.UnitCode,
                                               UnitName = s.Key.Unit.UnitName,
                                               BuyPrice = s.Key.BuyPrice,
                                               Quantity = s.Sum(st => (st.Quantity / st.Product.Unit.Count))
                                           });
            // var product = ProductQuery.OrderBy(p => p.ProductCode).Where(p => p.Storages.Any(s => s.ProductCode == p.ProductCode));
            return storage.ToArray();
        }

        /// <summary>浏览加载卷烟信息</summary>
        public object LoadProduct(int page, int rows)
        {
            IQueryable<Product> ProductQuery = ProductRepository.GetQueryable();
            var product = ProductQuery.OrderBy(p => p.ProductCode).Select(p => new { p.ProductCode, p.ProductName });
            int total = product.Count();
            product = product.Skip((page - 1) * rows).Take(rows);
            return new { total, rows = product.ToArray() };
        }
        

        /// <summary>获取卷烟信息</summary>
        public object GetProductBy(int page, int rows, string QueryString, string Value)
        {
            string productCode = "", productName = "";
            
            if (QueryString == "ProductCode")
            {
                productCode = Value;
            }
            else
            {
                productName = Value;
            }
            IQueryable<Product> ProductQuery = ProductRepository.GetQueryable();
            var product = ProductQuery.Where(c => c.ProductCode.Contains(productCode) && c.ProductName.Contains(productName))
                .OrderBy(c => c.ProductCode)
                .Select(c => new { 
                    c.ProductCode,
                    c.ProductName
                });
            int total = product.Count();
            product = product.Skip((page - 1) * rows).Take(rows);
            return new { total, rows = product.ToArray() };
        }

        #endregion


        public object FindProduct()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="ProductName"></param>
        /// <param name="ProductCode"></param>
        /// <param name="CustomCode"></param>
        /// <param name="BrandCode"></param>
        /// <param name="UniformCode"></param>
        /// <param name="AbcTypeCode"></param>
        /// <param name="ShortCode"></param>
        /// <param name="PriceLevelCode"></param>
        /// <param name="SupplierCode"></param>
        /// <returns></returns>
        public System.Data.DataTable GetProduct(int page, int rows, string ProductName, string ProductCode, string CustomCode, string BrandCode, string UniformCode, string AbcTypeCode, string ShortCode, string PriceLevelCode, string SupplierCode)
        {
            IQueryable<Product> ProductQuery = ProductRepository.GetQueryable();
            var product = ProductQuery.Where(c => c.ProductName.Contains(ProductName)
                && c.ProductCode.Contains(ProductCode)
                && c.BrandCode.Contains(BrandCode)
                && c.UniformCode.Contains(UniformCode)
                && c.SupplierCode.Contains(SupplierCode))
                .OrderBy(c => c.ProductCode)
                .Select(c => c);
            if (!CustomCode.Equals(string.Empty))
            {
                product = product.Where(p => p.CustomCode == CustomCode);
            }
            if (!AbcTypeCode.Equals(string.Empty))
            {
                product = product.Where(p => p.AbcTypeCode == AbcTypeCode);
            }
            if (!UniformCode.Equals(string.Empty))
            {
                product = product.Where(p => p.UniformCode == UniformCode);
            }
            if (!PriceLevelCode.Equals(string.Empty))
            {
                product = product.Where(p => p.PriceLevelCode == PriceLevelCode);
            }

            var temp = product.ToArray().Select(c => new
            {
                c.BrandCode,
                IsAbnormity = c.IsAbnormity == "1" ? "是" : "不是",
                IsActive = c.IsActive == "1" ? "可用" : "不可用",
                c.ProductCode,
                c.ProductName,
                c.ProductTypeCode,
                c.RetailPrice,
                c.SupplierCode,
                c.UniformCode,
                c.UnitCode,
                c.Unit.UnitName,
                c.UnitListCode,
                UpdateTime = c.UpdateTime.ToString("yyyy-MM-dd hh:mm:ss")
            });
            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Columns.Add("商品编码", typeof(string));
            dt.Columns.Add("商品名称", typeof(string));
            dt.Columns.Add("统一编码", typeof(string));
            dt.Columns.Add("计量单位系列", typeof(string));
            dt.Columns.Add("缺省计量单位", typeof(string));
            dt.Columns.Add("厂商", typeof(string));
            dt.Columns.Add("商品品牌", typeof(string));
            dt.Columns.Add("异形烟", typeof(string));
            dt.Columns.Add("是否可用", typeof(string));
            dt.Columns.Add("更新时间", typeof(string));
            foreach (var item in temp)
            {
                dt.Rows.Add
                    (
                        item.ProductCode,
                        item.ProductName,
                        item.UniformCode,
                        item.UnitListCode,
                        item.UnitCode,
                        item.SupplierCode,
                        item.BrandCode,
                        item.IsAbnormity,
                        item.IsActive,
                        item.UpdateTime
                    );
            }
            return dt;
        }
    }
}
