using System.IO;
using MrCMS.Web.Apps.Ecommerce.Areas.Admin.Models;

namespace MrCMS.Web.Apps.Ecommerce.Areas.Admin.Services
{
    public interface IBulkStockUpdateAdminService
    {
        BulkStockUpdateResult BulkStockUpdate(Stream file);
    }
}