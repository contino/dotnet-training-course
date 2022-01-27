﻿using System;

namespace BusinessObjectLayer.Progressive.OnlineShop.V2.Part7
{
    public class OnlineShop : IShopContainer
    {
        private IShopAssistant shopAssistant;
        private IOrderManager orderManager;
        private IPaymentManager payManager;
        private IInteractor interactor;
        private ICustomerService custService;

        public OnlineShop(IShopAssistant shopAssistant, IOrderManager orderManager, IPaymentManager payManager, 
            IInteractor interactor, ICustomerService custService)
        {
            this.shopAssistant = shopAssistant;
            this.orderManager = orderManager;
            this.payManager = payManager;
            this.interactor = interactor;
            this.custService = custService;
        }

        public void ServeCustomer()
        {
            OnlineBasket basket = default;
            var browseDecision = shopAssistant.Browse(out basket);
            switch(browseDecision)
            {
                case CustomerDecision.Quit:
                    interactor.Message("Thank for you calling.  See you soon!");
                    return;

                case CustomerDecision.SaveAndQuit:
                    custService.SaveBasket(basket);
                    break;

                case CustomerDecision.Checkout:
                    var customer = custService.EstablishCustomerContext(browseDecision);
                    interactor.Message($"Ready to checkout {customer.Email}");

                    var order = orderManager.Create(basket);
                    var payResult = payManager.ProcessPayment(customer, order);

                    break;

                default:
                    throw new Exception($"An invalid CustomerDecision {browseDecision} was returned from { shopAssistant.GetType().FullName }.Browse()");
            }
        }
    }
}
