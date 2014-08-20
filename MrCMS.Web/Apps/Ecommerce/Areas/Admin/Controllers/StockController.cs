﻿using System;
using System.Web;
using System.Web.Mvc;
using MrCMS.Web.Apps.Ecommerce.ACL;
using MrCMS.Website;
using MrCMS.Website.Controllers;
using MrCMS.Web.Apps.Ecommerce.Entities.Products;
using MrCMS.Web.Apps.Ecommerce.Services.Inventory;
using MrCMS.Website.Filters;

namespace MrCMS.Web.Apps.Ecommerce.Areas.Admin.Controllers
{
    public class StockController : MrCMSAppAdminController<EcommerceApp>
    {
        private readonly IStockAdminService _stockAdminService;

        public StockController(IStockAdminService stockAdminService)
        {
            _stockAdminService = stockAdminService; 
        }

        [HttpGet]
        [MrCMSACLRule(typeof(LowStockReportACL), LowStockReportACL.View)]
        public ViewResult LowStockReport(int threshold = 10)
        {
            if (TempData.ContainsKey("export-status"))
                ViewBag.ExportStatus = TempData["export-status"];
            ViewData["threshold"] = threshold;
            return View();
        }

        [HttpGet]
        [MrCMSACLRule(typeof(LowStockReportACL), LowStockReportACL.LowStockReportProductVariants)]
        public PartialViewResult LowStockReportProductVariants(int threshold = 10, int page = 1)
        {
            var items = _stockAdminService.GetAllVariantsWithLowStock(threshold, page);
            return PartialView(items);
        }

        [HttpPost]
        [ForceImmediateLuceneUpdate]
        [MrCMSACLRule(typeof(LowStockReportACL), LowStockReportACL.Update)]
        public JsonResult UpdateStock(ProductVariant productVariant)
        {
            _stockAdminService.Update(productVariant);
            return Json(true);
        }

        [HttpGet]
        [MrCMSACLRule(typeof(LowStockReportACL), LowStockReportACL.CanExportLowStockReport)]
        public ActionResult ExportLowStockReport(int threshold = 10)
        {
            try
            {
                var file = _stockAdminService.ExportLowStockReport(threshold);
                ViewBag.ExportStatus = "Low Stock Report successfully exported.";
                return File(file, "text/csv", "MrCMS-LowStockReport-"+DateTime.UtcNow+".csv");
            }
            catch (Exception ex)
            {
                CurrentRequestData.ErrorSignal.Raise(ex);
                ViewBag.ExportStatus = "Low Stock Report exporting has failed. Please try again and contact system administration if error continues to appear.";
                return RedirectToAction("LowStockReport");
            }
        }

        [HttpGet]
        [MrCMSACLRule(typeof(BulkStockUpdateACL), BulkStockUpdateACL.BulkStockUpdate)]
        public ViewResult BulkStockUpdate()
        {
            if (TempData.ContainsKey("messages"))
                ViewBag.Messages = TempData["messages"];
            if (TempData.ContainsKey("import-status"))
                ViewBag.ImportStatus = TempData["import-status"];
            if (TempData.ContainsKey("export-status"))
                ViewBag.ExportStatus = TempData["export-status"];
            return View();
        }

        [HttpPost]
        [ActionName("BulkStockUpdate")]
        [ForceImmediateLuceneUpdate]
        [MrCMSACLRule(typeof(BulkStockUpdateACL), BulkStockUpdateACL.BulkStockUpdate)]
        public RedirectToRouteResult BulkStockUpdate_POST(HttpPostedFileBase document)
        {
            if (document != null && document.ContentLength > 0 && (document.ContentType.ToLower() == "text/csv" || document.ContentType.ToLower().Contains("excel")))
                TempData["messages"] = _stockAdminService.BulkStockUpdate(document.InputStream);
            else
                TempData["import-status"] = "Please choose non-empty CSV (.csv) file before uploading.";
            return RedirectToAction("BulkStockUpdate");
        }

        [HttpGet]
        [MrCMSACLRule(typeof(LowStockReportACL), LowStockReportACL.CanExportLowStockReport)]
        public ActionResult ExportStockReport()
        {
            try
            {
                var file = _stockAdminService.ExportStockReport();
                TempData["export-status"] = "Stock Report successfully exported.";
                return File(file, "text/csv", "MrCMS-StockReport-" + DateTime.UtcNow + ".csv");
            }
            catch (Exception ex)
            {
                CurrentRequestData.ErrorSignal.Raise(ex);
                TempData["export-status"] = "Stock Report exporting has failed. Please try again and contact system administration if error continues to appear.";
                return RedirectToAction("BulkStockUpdate");
            }
        }
    }
}