using System.Web.Mvc;
using MrCMS.Web.Apps.Ecommerce.Areas.Admin.Models;
using MrCMS.Web.Apps.Ecommerce.Areas.Admin.Services;
using MrCMS.Web.Apps.Ecommerce.Entities.GiftCards;
using MrCMS.Web.Areas.Admin.Helpers;
using MrCMS.Website.Controllers;

namespace MrCMS.Web.Apps.Ecommerce.Areas.Admin.Controllers
{
    public class GiftCardController : MrCMSAppAdminController<EcommerceApp>
    {
        private readonly IGiftCardAdminService _giftCardAdminService;

        public GiftCardController(IGiftCardAdminService giftCardAdminService)
        {
            _giftCardAdminService = giftCardAdminService;
        }

        public ViewResult Index(GiftCardSearchQuery query)
        {
            ViewData["results"] = _giftCardAdminService.Search(query);
            return View(query);
        }

        [HttpGet]
        public JsonResult GenerateCode()
        {
            return Json(_giftCardAdminService.GenerateCode(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ViewResult Add()
        {
            ViewData["gift-card-type-options"] = _giftCardAdminService.GetTypeOptions();
            return View();
        }

        [HttpPost]
        public RedirectToRouteResult Add(GiftCard giftCard)
        {
            _giftCardAdminService.Add(giftCard);
            TempData.SuccessMessages().Add("Gift card added");
            return RedirectToAction("Edit", new { id = giftCard.Id });
        }

        [HttpGet]
        public ViewResult Edit(GiftCard giftCard)
        {
            ViewData["gift-card-type-options"] = _giftCardAdminService.GetTypeOptions();
            ViewData["activation-status-options"] = _giftCardAdminService.GetStatusOptions();
            return View(giftCard);
        }

        [HttpPost]
        [ActionName("Edit")]
        public RedirectToRouteResult Edit_POST(GiftCard giftCard)
        {
            _giftCardAdminService.Update(giftCard);
            TempData.SuccessMessages().Add("Gift card updated");
            return RedirectToAction("Edit", giftCard.Id);
        }

        [HttpGet]
        public ViewResult Delete(GiftCard giftCard)
        {
            return View(giftCard);
        }

        [HttpPost]
        [ActionName("Delete")]
        public RedirectToRouteResult Delete_POST(GiftCard giftCard)
        {
            _giftCardAdminService.Delete(giftCard);
            TempData.SuccessMessages().Add("Gift card deleted");
            return RedirectToAction("Index");
        }
    }
}