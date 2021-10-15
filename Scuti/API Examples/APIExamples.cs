using System.Collections;
using System;

using Scuti;
using UnityEngine;

using Scuti.Net;
using Scuti.GraphQL;
using System.Collections.Generic;

namespace Scuti.Examples
{
    public class APIExamples : MonoBehaviour
    {

        [ContextMenu("Get offers")]
        public async void GetOffers()
        {

            var res = await ScutiNetClient.Instance.Offer.GetOffers(new List<GraphQL.Generated.CampaignType> { GraphQL.Generated.CampaignType.Product, GraphQL.Generated.CampaignType.ProductListing }, GraphQL.Generated.FILTER_TYPE.In, null);
            foreach (GraphQL.Generated.Offer offer in res.Nodes)
                ScutiLogger.Log($"{offer.Name} {offer.Id}");
        }


        [ContextMenu("Get offer details")]
        public async void GetOfferDetails()
        {
            var res = await ScutiNetClient.Instance.Offer.GetOfferByID("82853dc6-e309-4386-8469-fedd114bdb36");

            ScutiLogger.Log(
                "Details -->\n" +
                res.Id + " \n" +
                res.Name + " \n" +
                res.Product.Description + "\n" +
                res.Category + "\n" +
                res.Review + "\n" +
                res.Review + "\n" +
                res.Promotions.IsHotItem + "\n" +
                res.Promotions.IsNew
            );
        }



        [ContextMenu("get rewards")]
        public async void GetRewards()
        {
            System.Collections.Generic.List<GraphQL.Generated.Reward> res = await ScutiAPI.GetRewards();
            foreach (var r in res)
            {
                ScutiLogger.Log($"{r.Id} {r.Activated}");
            }
        }

        [ContextMenu("Checkout")]
        public async void CheckoutTest()
        {
            await ScutiNetClient.Instance.CreateCheckoutToken(new CheckoutTokenRequest
            {
                type = "card",
                expiry_month = 4,
                expiry_year = 2023,
                number = "4007400000000007"
            });
        }


        [ContextMenu("Card Details")]
        public async void GetCardDetails()
        {
            var rest = await ScutiAPI.GetCardDetails("9fc5d6d8-4bbe-42c5-804a-c98c9a7494b6");
        }

        [ContextMenu("Payments")]
        public async void GetPaymentInfo()
        {
            var rest = await ScutiAPI.GetPayments();

        }

        [ContextMenu("Create Card")]
        public async void CreateCard()
        {
            /*var rest = await ScutiAPI.CreateOrReplaceCard(10, 2022, "Blogs", encrypted: new GraphQL.Generated.EncryptedInput{
                EncryptedData = "1524asdfsa7d5sdf45sdaf4asdf7",

            }, addressInput: new GraphQL.Generated.AddressInput{

                Address1 = "Hello",
                Address2 = "Hello address 2",
                City = "Sacramento",
                ZipCode = "12345",
                Country = "US",
                State = "CA"
            });*/

        }


        [ContextMenu("activate rewards")]
        public async void ActivateRewards()
        {
            ActivateRewardResponse res = await ScutiAPI.ActivateReward(new string[] { "aaabf37c-1ba9-499b-87aa-d35e1b93f029" });
            foreach (var r in res.activateRewards)
            {
                ScutiLogger.Log(r);
            }
        }

        [ContextMenu("Get Wallet")]
        public async void GetWallet()
        {
            GraphQL.Generated.Wallet res = await ScutiAPI.GetWallet(false);
        }

        //[ContextMenu("Get games")]
        //public async void GetGames()
        //{
        //    var games = await MetricsAPI.GetGames();
        //    Debug.Log(games.Count);
        //    foreach(var g in games)
        //    {
        //        Debug.Log($"{g.Name} {g.Id}");
        //    }
        //}

        [ContextMenu("Get game info")]
        public async void GetGameInfo()
        {
            await ScutiAPI.GetGameInfo();
        }

        [ContextMenu("Exchange")]
        public async void Exchange()
        {
            await ScutiAPI.ExchangeRewards(100);

        }

        [ContextMenu("Metric")]
        public void Metric()
        {
            ScutiAPI.AdImpressionPriorToBuyingMetric(10);
        }

        [ContextMenu("Category Statistics")]
        public async void CategoryStatistics()
        {
            var res = await ScutiAPI.GetCategoryStatistics();
            Debug.Log($"we have {res.Total} items");
            foreach (var item in res.ByCategories)
            {
                Debug.Log($"Item: {item.Category} count = {item.Count}");
            }
        }

        [ContextMenu("Register")]
        public async void Register()
        {
            try
            {
                var response = await ScutiNetClient.Instance.RegisterUser("ashkan.saeedi.1989@gmail.com", "passpasspass123", "Ashkan Saeedi Mazdeh");
                ScutiLogger.Log(response);
            }
            catch (Exception e)
            {
                ScutiLogger.LogError(e + "" + e.InnerException);
            }
        }



        [ContextMenu("Refresh")]
        public async void RefreshToken()
        {
            await ScutiNetClient.Instance.RefreshAuthToken();
        }

        [ContextMenu("Register phone")]
        public async void RegisterPhone()
        {
            var response = await ScutiNetClient.Instance.RegisterPhone("+12092449451", 1);
            print($"{response.emailVerified} {response.phoneVerified}");
        }

        [ContextMenu("verify phone")]
        public async void VerifyPhone()
        {
            var response = await ScutiNetClient.Instance.VerifyPhone("1");
            print($"{response.emailVerified} {response.phoneVerified}");
        }


        [ContextMenu("Get Info")]
        public async void GetInfo()
        {
            // TODO: Not working now. Change to GET?
            var response = await ScutiNetClient.Instance.GetAccountInfo();
            print($"Email verified: {response.emailVerified} Phone Verified: {response.phoneVerified}");
        }
    }
}
