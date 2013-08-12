using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using THOK.Wms.Bll.Interfaces;
using THOK.Common.WebUtil;
using THOK.Wms.DbModel;
using THOK.Security;
using THOK.Common.NPOI.Service;
using THOK.Common.NPOI.Models;

namespace Wms.Controllers.Wms.BasisInfo
{
    [TokenAclAuthorize]
    public class PathController : Controller
    {
       
            [Dependency]
            public IPathService PathService { get; set; }

            //
            // GET: /Path/Index

            public ActionResult Index(string moduleID)
            {
                ViewBag.hasSearch = true;
                ViewBag.hasAdd = true;
                ViewBag.hasEdit = true;
                ViewBag.hasDelete = true;
                ViewBag.hasNode = true;
                ViewBag.hasPrint = true;
                ViewBag.hasHelp = true;
                ViewBag.ModuleID = moduleID;
                return View();
            }

            public ActionResult AddPage()
            {
                return View();
            }

            public ActionResult SearchPage()
            {
                return View();
            }

            //
            // GET: /Path/Details/5

            public ActionResult Details(int page, int rows, FormCollection collection)
            {
                Path path = new Path();
                path.PathName = collection["PathName"] ?? "";
                path.State = collection["State"] ?? "";
                string OriginRegionID = collection["OriginRegionID"] ?? "";
                string TargetRegionID = collection["TargetRegionID"] ?? "";
                if (OriginRegionID != "" && OriginRegionID != null)
                {
                    path.OriginRegionID = Convert.ToInt32(OriginRegionID);
                }
                if (TargetRegionID!=""&&TargetRegionID!=null)
                {
                    path.TargetRegionID = Convert.ToInt32(TargetRegionID);
                }
                var pathDetail = PathService.GetDetails(page, rows, path);
                return Json(pathDetail, "text", JsonRequestBehavior.AllowGet);
            }


            //
            // POST: /Path/Create

            [HttpPost]
            public ActionResult Create(Path path)
            {
                string strResult = string.Empty;
                bool bResult = PathService.Add(path);
                string msg = bResult ? "新增成功" : "新增失败";
                return Json(JsonMessageHelper.getJsonMessage(bResult, msg, strResult), "text", JsonRequestBehavior.AllowGet);
            }

          
            //
            // POST: /Path/Edit/5

            [HttpPost]
            public ActionResult Edit(Path path)
            {
                string strResult = string.Empty;
                bool bResult = PathService.Save(path,out strResult);
                string msg = bResult ? "修改成功" : "修改失败";
                return Json(JsonMessageHelper.getJsonMessage(bResult, msg, strResult), "text", JsonRequestBehavior.AllowGet);
            }

           
            
             //
             // POST: /Path/Delete/

        [HttpPost]
        public ActionResult Delete(int pathId)
        {
            string strResult = string.Empty;
            bool bResult = false;
            bResult = PathService.Delete(pathId);
            string msg = bResult ? "删除成功" : "删除失败";
            return Json(JsonMessageHelper.getJsonMessage(bResult, msg, strResult), "text", JsonRequestBehavior.AllowGet);
        }





            // POST: /Path/GetPath/
            public ActionResult GetPath(int page, int rows, string queryString, string value)
            {
                if (queryString == null)
                {
                    queryString = "PathName";
                }
                if (value == null)
                {
                    value = "";
                }
                var path = PathService.GetPath(page, rows, queryString, value);
                return Json(path, "text", JsonRequestBehavior.AllowGet);
            }

            #region /Path/CreateExcelToClient/
            public FileStreamResult CreateExcelToClient()
            {
                int page = 0, rows = 0;
                string PathName = Request.QueryString["PathName"];
                int OriginRegionID = Convert.ToInt32( Request.QueryString["OriginRegionID"]);
                int TargetRegionID = Convert.ToInt32(Request.QueryString["TargetRegionID"]);
                string State = Request.QueryString["State"];
                Path path = new Path();
                path.PathName = PathName;
                path.OriginRegionID = OriginRegionID;
                path.TargetRegionID = TargetRegionID;
                path.State = State;

                ExportParam ep = new ExportParam();
                ep.DT1 = PathService.GetPath(page, rows, path);
                ep.HeadTitle1 = "路径信息";
                return PrintService.Print(ep);
            }
            #endregion
    }
}



