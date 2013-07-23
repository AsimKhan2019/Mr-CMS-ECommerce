﻿using MrCMS.Web.Apps.Ecommerce.Payment.PayPalExpress;
using MrCMS.Web.Apps.Ecommerce.Settings;
using MrCMS.Website;

namespace MrCMS.Web.Apps.Ecommerce.Payment.CashOnDelivery
{
    public class CashOnDeliveryPaymentMethod : BasePaymentMethod
    {
        public override string Name
        {
            get { return "Cash On Delivery"; }
        }

        public override PaymentType PaymentType
        {
            get { return PaymentType.ServiceBased; }
        }

        public override bool Enabled
        {
            get { return MrCMSApplication.Get<PaymentSettings>().CashOnDeliveryEnabled; }
        }
    }
}