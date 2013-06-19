﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using MrCMS.Entities.Documents.Media;
using MrCMS.Entities.Documents.Web;
using MrCMS.Services;
using MrCMS.Web.Apps.Ecommerce.Entities.Products;
using MrCMS.Web.Apps.Ecommerce.Entities.Tax;
using MrCMS.Web.Apps.Ecommerce.Models;
using System.Linq;
using MrCMS.Helpers;
using MrCMS.Web.Apps.Ecommerce.Settings;
using MrCMS.Website;
using NHibernate;

namespace MrCMS.Web.Apps.Ecommerce.Pages
{
    public class Product : Webpage, IBuyableItem
    {
        public Product()
        {
            Variants = new List<ProductVariant>();
            SpecificationValues = new List<ProductSpecificationValue>();
            Categories = new List<Category>();
            AttributeOptions = new List<ProductAttributeOption>();
        }

        public virtual MediaCategory Gallery { get; set; }

        public virtual ProductAvailability Availability
        {
            get
            {
                if (PublishOn.HasValue && PublishOn <= DateTime.UtcNow)
                    return ProductAvailability.Available;
                return ProductAvailability.PreOrder;
            }
        }

        public virtual bool InStock
        {
            get { return !StockRemaining.HasValue || StockRemaining > 0; }
        }

        [Remote("IsUniqueSKU", "Product", AdditionalFields = "Id")]
        public virtual string SKU { get; set; }

        public virtual decimal TaxRatePercentage
        {
            get
            {
                return TaxRate == null
                           ? 0
                           : TaxRate.Percentage;
            }
        }

        [DisplayName("Stock Remaining")]
        public virtual int? StockRemaining { get; set; }

        [DisplayName("Price Pre Tax")]
        public virtual decimal PricePreTax
        {
            get { return TaxAwarePrice.GetPriceExcludingTax(BasePrice, TaxRate); }
        }

        public virtual decimal Weight { get; set; }

        public virtual decimal ReducedByIncludingTax
        {
            get
            {
                return !PreviousPriceIncludingTax.HasValue
                           ? 0
                           : PreviousPriceIncludingTax > Price
                                 ? PreviousPriceIncludingTax.Value - Price
                                 : 0;
            }
        }
        public virtual decimal ReducedByExcludingTax
        {
            get
            {
                return !PreviousPriceExcludingTax.HasValue
                           ? 0
                           : PreviousPriceExcludingTax > PricePreTax
                                 ? PreviousPriceExcludingTax.Value - PricePreTax
                                 : 0;
            }
        }

        [DisplayName("Previous Price")]
        public virtual decimal? PreviousPrice { get; set; }

        public virtual decimal? PreviousPriceIncludingTax
        {
            get { return TaxAwarePrice.GetPriceIncludingTax(PreviousPrice, TaxRate); }
        }

        public virtual decimal? PreviousPriceExcludingTax
        {
            get { return TaxAwarePrice.GetPriceExcludingTax(PreviousPrice, TaxRate); }
        }

        public virtual decimal ReducedByPercentage
        {
            get
            {
                return PreviousPriceIncludingTax.GetValueOrDefault() > 0
                           ? ReducedByIncludingTax / PreviousPriceIncludingTax.Value
                           : 0;
            }
        }

        [DisplayName("Price")]
        public virtual decimal BasePrice { get; set; }

        public virtual decimal Price
        {
            get { return TaxAwarePrice.GetPriceIncludingTax(BasePrice, TaxRate); }
        }

        public virtual decimal GetPrice(int quantity)
        {
            if (PriceBreaks.Any())
            {
                List<PriceBreak> priceBreaks = PriceBreaks.Where(x => quantity >= x.Quantity).OrderBy(x => x.Price).ToList();
                if (priceBreaks.Any())
                    return priceBreaks.First().PriceIncludingTax * quantity;
            }

            return Price * quantity;
        }

        public virtual decimal GetSaving(int quantity)
        {
            return PreviousPriceIncludingTax.GetValueOrDefault() != 0
                       ? ((PreviousPriceIncludingTax*quantity) - GetPrice(quantity)).Value
                       : (Price*quantity) - GetPrice(quantity);
        }

        public virtual decimal GetTax(int quantity)
        {
            return Math.Round(GetPrice(quantity) - GetPricePreTax(quantity), 2, MidpointRounding.AwayFromZero);
        }

        public virtual decimal GetPricePreTax(int quantity)
        {
            return Math.Round(GetPrice(quantity) / ((TaxRatePercentage + 100) / 100), 2, MidpointRounding.AwayFromZero);
        }

        public virtual decimal GetUnitPrice()
        {
            return Price;
        }

        public virtual TaxRate TaxRate { get; set; }

        public virtual decimal Tax
        {
            get { return Price - PricePreTax; }
        }

        public virtual bool HasVariants
        {
            get { return Variants.Any(); }
        }

        public virtual IList<ProductVariant> Variants { get; set; }

        public virtual IList<ProductSpecificationValue> SpecificationValues { get; set; }

        public virtual bool CanBuy(int quantity)
        {
            return quantity > 0 && (!StockRemaining.HasValue || StockRemaining >= quantity);
        }

        public virtual string GetSpecification(string name)
        {
            var spec = SpecificationValues.FirstOrDefault(value => value.ProductSpecificationAttribute.Name == name);
            if (spec == null)
                return null;
            return spec.Value;
        }

        public override void AdminViewData(ViewDataDictionary viewData, ISession session)
        {
            base.AdminViewData(viewData, session);
            viewData["brands"] = session.QueryOver<Brand>()
                                        .OrderBy(brand => brand.Name).Asc
                                        .Cacheable()
                                        .List()
                                        .BuildSelectItemList(brand => brand.Name, brand => brand.Id.ToString(),
                                                             brand => brand == Brand, "None selected");
        }

        public virtual IList<Category> Categories { get; set; }

        protected override void CustomInitialization(IDocumentService service, ISession session)
        {
            base.CustomInitialization(service, session);

            var mediaCategory = service.GetDocumentByUrl<MediaCategory>("product-galleries");
            if (mediaCategory == null)
            {
                mediaCategory = new MediaCategory
                                    {
                                        Name = "Product Galleries",
                                        UrlSegment = "product-galleries",
                                        IsGallery = true
                                    };
                service.AddDocument(mediaCategory);
            }
            var productGallery = new MediaCategory
                                     {
                                         Name = Name,
                                         UrlSegment = "product-galleries/" + UrlSegment,
                                         IsGallery = true,
                                         Parent = mediaCategory
                                     };
            Gallery = productGallery;

            service.AddDocument(productGallery);
        }

        public virtual string ContainerUrl
        {
            get
            {
                var webpage = (Parent as Webpage);
                return webpage != null ? webpage.LiveUrlSegment : string.Empty;
            }
        }

        public virtual IList<ProductAttributeOption> AttributeOptions { get; set; }

        public virtual Brand Brand { get; set; }

        [StringLength(1000)]
        public virtual string Abstract { get; set; }

        public virtual IEnumerable<MediaFile> Images
        {
            get
            {
                return Gallery != null
                           ? (IEnumerable<MediaFile>)
                             Gallery.Files.Where(file => file.IsImage).OrderBy(file => file.DisplayOrder)
                           : new List<MediaFile>();
            }
        }

        public virtual string EditUrl
        {
            get { return "~/Admin/Webpage/Edit/" + Id; }
        }

        public virtual IList<PriceBreak> PriceBreaks
        {
            get
            {
                return MrCMSApplication.Get<ISession>()
                                       .QueryOver<PriceBreak>()
                                       .Where(@break => @break.Item == this)
                                       .Cacheable()
                                       .List();
            }
        }
    }
}