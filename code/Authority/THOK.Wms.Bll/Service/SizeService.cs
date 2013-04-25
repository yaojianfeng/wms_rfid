﻿using System;
using Microsoft.Practices.Unity;
using THOK.Wms.Bll.Interfaces;
using THOK.Wms.Dal.Interfaces;
using THOK.Wms.DbModel;
using System.Linq;

namespace THOK.Wms.Bll.Service
{
    public class SizeService : ServiceBase<Size>, ISizeService
    {
        [Dependency]
        public ISizeRepository SizeRepository { get; set; }

        protected override Type LogPrefix
        {
            get { return this.GetType(); }
        }

        public object GetDetails(int page, int rows,string SizeName, string SizeNo)
        {
            IQueryable<Size> sizeQuery = SizeRepository.GetQueryable();
            var size = sizeQuery.Where(s => s.SizeName.Contains(SizeName))
                .OrderBy(s => s.ID).AsEnumerable()
                .Select(s => new
                {
                    s.ID,
                    s.SizeName,
                    s.SizeNo,
                    s.Length,
                    s.Width,
                    s.Height
                });
            if(SizeNo != "" && SizeNo!=null)
            {
                int sizeno = -1;
                try { sizeno = Convert.ToInt32(SizeNo); }
                catch { sizeno = -1; }
                finally 
                {
                    size = size.Where(s => s.SizeNo == sizeno)
                        .OrderBy(s => s.ID).AsEnumerable()
                        .Select(s => new
                        {
                            s.ID,
                            s.SizeName,
                            s.SizeNo,
                            s.Length,
                            s.Width,
                            s.Height
                        });
                }
            }
            
            int total = size.Count();
            size = size.Skip((page - 1) * rows).Take(rows);
            return new { total, rows = size.ToArray() };
        }

        public bool Add(Size size)
        {
            var s = new Size();
            s.ID = size.ID;
            s.Height = size.Height;
            s.Length = size.Length;
            s.SizeName = size.SizeName;
            s.SizeNo = size.SizeNo;
            s.Width = size.Width;
            SizeRepository.Add(s);
            SizeRepository.SaveChanges();
            return true;
        }

        public bool Save(Size size)
        {
            var si = SizeRepository.GetQueryable().FirstOrDefault(s => s.ID == size.ID);
            si.ID = size.ID;
            si.Height = size.Height;
            si.Length = size.Length;
            si.Width = size.Width;
            si.SizeNo = size.SizeNo;
            si.SizeName = size.SizeName;
            SizeRepository.SaveChanges();
            return true;
        }

        public bool Delete(int sizeId)
        {
            var size= SizeRepository.GetQueryable().FirstOrDefault(s => s.ID == sizeId);
            if (size != null)
            {
                SizeRepository.Delete(size);
                SizeRepository.SaveChanges();
            }
            else
                return false;
            return true;
        }

        public object GetSize(int page, int rows, string queryString, string value)
        {
            string id = "", sizeName = "";

            if (queryString == "id")
            {
                id = value;
            }
            else
            {
                sizeName = value;
            }
            IQueryable<Size> sizeQuery = SizeRepository.GetQueryable();
            //int Id = Convert.ToInt32(id);
            var size = sizeQuery.Where(si => si.SizeName.Contains(sizeName))
                .OrderBy(si => si.ID).AsEnumerable().
                Select(si => new
                {
                    si.ID,
                    si.SizeName,
                    si.SizeNo
                });
            int total = size.Count();
            size = size.Skip((page - 1) * rows).Take(rows);
            return new { total, rows = size.ToArray() };
        }
        public System.Data.DataTable GetSize(int page, int rows, string sizeName)
        {
            IQueryable<Size> sizeQuery = SizeRepository.GetQueryable();
            var size = sizeQuery.Where(si => si.SizeName.Contains(sizeName))
                .OrderBy(si => si.ID).AsEnumerable()
                .Select(si => new
                {
                    si.ID,
                    si.SizeName,
                    si.SizeNo,
                    si.Length,
                    si.Width,
                    si.Height
                });
            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Columns.Add("尺寸ID", typeof(string));
            dt.Columns.Add("尺寸名称", typeof(string));
            dt.Columns.Add("尺寸编号", typeof(string));
            dt.Columns.Add("长度", typeof(string));
            dt.Columns.Add("宽度", typeof(string));
            dt.Columns.Add("高度", typeof(string));
            foreach (var item in size)
            {
                dt.Rows.Add
                    (
                        item.ID,
                        item.SizeName,
                        item.SizeNo,
                        item.Length,
                        item.Width,
                        item.Height
                    );
            }
            return dt;
        }

    }
}
