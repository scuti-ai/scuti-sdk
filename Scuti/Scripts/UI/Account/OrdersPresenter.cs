using System;
using System.Threading.Tasks;

using UnityEngine;


using System.Collections.Generic;
using Scuti.Net;

// WIP
namespace Scuti.UI {
    public class OrdersPresenter : Presenter<OrdersPresenter.Model> 
    {
        [Serializable]
        public class Model : Presenter.Model
        {
            public List<OrderItemPresenter.Model> Orders;
        }


        public OrderItemPresenter OrderPrefab;
        public Transform OrderContainer;

        public override void Open()
        {
            base.Open();

            TryToLoadData();
        }

        private async void TryToLoadData()
        {
            var orderInfo = await ScutiAPI.GetOrderHistory();
            if (orderInfo != null)
            {
                var loadingData =  new Model() { Orders = new List<OrderItemPresenter.Model>() };

                loadingData.Orders.Clear();
                foreach (var order in orderInfo.Nodes)
                {
                    foreach (var item in order.Items)
                    {
                        var orderData = new OrderItemPresenter.Model()
                        {
                            orderId = order.Id.ToString(),
                            orderStatus = order.Status.Value.ToString(),
                            quantity = (int)item.Quantity.Value,
                            purchaseDate = order.CreatedAt.Value.ToString("MM/dd/yyyy"),
                            price = item.Amount.Value,
                            product = item.Product
                        };
                        loadingData.Orders.Add(orderData);
                   }
                }
                Data = loadingData;
            }
        }

        protected override void OnSetState()
        {
            base.OnSetState();
            RefreshList();
        }

        private void RefreshList()
        { 
            ClearOrders();
            
            if(Data!=null && Data.Orders!=null)
            {
                foreach(var order in Data.Orders)
                {
                    var orderView = Instantiate(OrderPrefab, OrderContainer);
                    orderView.Data = order;
                }
            }
        }

        private void ClearOrders()
        {
            foreach(Transform child in OrderContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
