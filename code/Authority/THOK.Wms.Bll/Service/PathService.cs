using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using THOK.Wms.DbModel;
using THOK.Wms.Bll.Interfaces;
using Microsoft.Practices.Unity;
using THOK.Wms.Dal.Interfaces;
using System.Data;
using THOK.WMS.Upload.Bll;

namespace THOK.Wms.Bll.Service
{
    public class PathService : ServiceBase<Path>, IPathService
    {
        [Dependency]
        public IPathRepository PathRepository { get; set; }

        [Dependency]
        public IRegionRepository RegionRepository { get; set; }

        [Dependency]
        public IPathNodeRepository PathNodeRepository { get; set; }

        UploadBll Upload = new UploadBll();

        protected override Type LogPrefix
        {
            get { return this.GetType(); }
        }

        public object GetDetails(int page, int rows, Path path)
        {
          
           IQueryable<Path> pathQuery = PathRepository.GetQueryable();
           IQueryable<Region> regionQuery = RegionRepository.GetQueryable();

           var pathDetail1 = pathQuery;
           if (path.OriginRegionID != null && path.OriginRegionID != 0)
           {
               pathDetail1 = pathQuery.Where(p => p.OriginRegionID == path.OriginRegionID).OrderBy(p => p.ID);
           }
           var pathDetail2 = pathDetail1;
           if (path.TargetRegionID != null && path.TargetRegionID != 0)
           {
               pathDetail2 = pathDetail1.Where(p => p.TargetRegionID == path.TargetRegionID).OrderBy(p => p.ID);
           }
           var pathDetail3 = pathDetail2;
           if (path.PathName != null)
           {
               pathDetail3 = pathDetail2.Where(p => p.PathName.Contains(path.PathName)).OrderBy(p => p.ID);
           }
           var pathDetail4 = pathDetail3;
           if (path.State != null)
           {
               pathDetail4 = pathDetail3.Where(p => p.State.Contains(path.State)).OrderBy(p => p.ID);
           }
           //int total = pathDetail4.Count();
           //var pathDetails = pathDetail4.Skip((page - 1) * rows).Take(rows);

           var path_Detail = pathDetail4.Join(regionQuery,
                       p => p.OriginRegionID,
                       r => r.ID,
                       (p, r) => new
                       {
                           p.ID,
                           p.PathName,
                           p.OriginRegionID,
                           p.TargetRegionID,
                           p.Description,
                           p.State,
                          
                           OriginRegionName = r.RegionName
                       })
                      .Join(regionQuery,
                      p => p.TargetRegionID,
                      r => r.ID,
                      (p, r) => new
                      {
                          p.ID,
                          p.PathName,
                          p.OriginRegionID,
                          p.TargetRegionID,
                          p.Description,
                          p.State,
                          p.OriginRegionName,
                          TargetRegionName=r.RegionName
                      })
                      //.Where(p => p.PathName.Contains(path.PathName)
                      //    &&p.State.Contains(path.State))
                       //&& p.TargetRegionName.Contains(path.TargetRegion)
                       //&& p.OriginRegionName.Contains(path.OriginRegion)
               .OrderBy(p => p.ID).AsEnumerable()
                .Select(p => new
                 {
                     p.ID,
                     p.PathName,
                     p.OriginRegionID,
                     p.OriginRegionName,
                     p.TargetRegionID,
                     p.TargetRegionName,
                     p.Description,
                     State = p.State == "01" ? "可用" : "不可用",
                 });
           int total = path_Detail.Count();
           var pathDetails = path_Detail.Skip((page - 1) * rows).Take(rows);

           return new { total, rows = pathDetails.ToArray() };
        }


        public bool Add(Path path)
        {
            var emp = new Path();
            emp.ID = path.ID;
            emp.PathName = path.PathName;
            emp.Description = path.Description;
            emp.OriginRegionID = path.OriginRegionID;
            emp.TargetRegionID = path.TargetRegionID;
            emp.State = path.State;
            PathRepository.Add(emp);

            PathRepository.SaveChanges();             
            return true;
        }

        public bool Save(Path path, out string strResult)
        {
            strResult = string.Empty;
            bool result = false;
            IQueryable<Region> regionQuery = RegionRepository.GetQueryable();
            var region = regionQuery.FirstOrDefault(r => r.ID == path.ID);
            var emp = PathRepository.GetQueryable().FirstOrDefault(p => p.ID == path.ID);
            if (emp != null)
            {
                emp.ID = path.ID;
                emp.PathName = path.PathName;
                emp.Description = path.Description;
                emp.OriginRegionID = path.OriginRegionID;
                emp.TargetRegionID = path.TargetRegionID;
                emp.State = path.State;
                PathRepository.SaveChanges();
                result = true;
            }
            else
            {
                strResult = "原因：未找到当前需要修改的数据！";
            }
            return result;
            //IQueryable<Region> regionQuery = RegionRepository.GetQueryable();
            //var region = regionQuery.FirstOrDefault(r => r.ID == path.ID);
            //var emp = PathRepository.GetQueryable().FirstOrDefault(p => p.ID == path.ID);
            //if(emp != null)
            //{
            //emp.ID = path.ID;
            //emp.PathName = path.PathName;
            //emp.Description = path.Description;
            //emp.OriginRegionID = path.OriginRegionID;
            //emp.TargetRegionID = path.TargetRegionID;
            //emp.State = path.State;
            //PathRepository.SaveChanges();
            //}
            //return true;
        }

        public bool Delete(int pathId)
        {
            var pa = PathRepository.GetQueryable().FirstOrDefault(p => p.ID == pathId);
            var pathnode = PathNodeRepository.GetQueryable().FirstOrDefault(p => p.PathID == pathId);
            if (pa!= null)
            {
                PathNodeRepository.Delete(pathnode);
                PathNodeRepository.SaveChanges();

                PathRepository.Delete(pa);
                PathRepository.SaveChanges();
            }
            else
                return false;
            return true;
        }
  

        public object GetPath(int page, int rows, string queryString, string value)
        {
            string id = "", pathName = "";


            if (queryString == "PathName")
            {
                id = value;
            }
            else
            {
                pathName = value;
            }
            IQueryable<Path> paQuery = PathRepository.GetQueryable();
            var path = paQuery.Where(p => p.PathName.Contains(pathName) && p.State == "01")
                .OrderBy(p => p.ID).AsEnumerable()
                .Select(p => new
                {
                    p.ID,
                    p.PathName,
                    p.Description,
                    State = p.State == "01" ? "可用" : "不可用",
                   
                });
            int total = path.Count();
            path = path.Skip((page - 1) * rows).Take(rows);
            return new { total, rows = path.ToArray() };
        }

        public DataTable GetPath(int page, int rows,Path path)
        {
            IQueryable<Path> pathQuery = PathRepository.GetQueryable();
            IQueryable<Region> regionQuery = RegionRepository.GetQueryable();
             var pathDetail = pathQuery.Join(regionQuery,
                       p => p.OriginRegionID,
                       r => r.ID,
                       (p, r) => new
                       {
                           p.ID,
                           p.PathName,
                           p.OriginRegionID,
                           p.TargetRegionID,
                           p.Description,
                           p.State,
                           
                           OriginRegionName = r.RegionName
                       })
                      .Join(regionQuery,
                      p => p.TargetRegionID,
                      r => r.ID,
                      (p, r) => new
                      {
                          p.ID,
                          p.PathName,
                          p.OriginRegionID,
                          p.TargetRegionID,
                          p.Description,
                          p.State,
                          p.OriginRegionName,
                          TargetRegionName=r.RegionName
                      })
                 .OrderBy(p => p.ID).AsEnumerable()
                 .Select(p => new
                 {
                     p.ID,
                     p.PathName,
                     p.OriginRegionName,
                     p.TargetRegionName,
                     p.Description,
                     State = p.State == "01" ? "可用" : "不可用",
                 });       
         
            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Columns.Add("路径编号", typeof(int));
            dt.Columns.Add("路径名称", typeof(string));
            dt.Columns.Add("起始区域名称", typeof(string));
            dt.Columns.Add("目标区域名称", typeof(string));
            dt.Columns.Add("描述", typeof(string));
            dt.Columns.Add("状态", typeof(string));
            foreach (var item in pathDetail)
            {
                dt.Rows.Add
                    (
                        item.ID,
                        item.PathName,
                        item.OriginRegionName,
                        item.TargetRegionName,
                        item.Description,
                        item.State
                       
                    );   
            }
            return dt;
        }

    }
}

