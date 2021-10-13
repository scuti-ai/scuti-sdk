using System;
using UnityEngine;
using UnityEngine.UI;
using Scuti;
using System.Threading.Tasks;
using Scuti.Net;
using Scuti.GraphQL;
using Scuti.GraphQL.Generated;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Collections.Generic;

namespace Scuti.UI
{
    public class CardManager : MonoBehaviour /*Form<CardManager.Model>*/
    {
       /* [Serializable]
        public class Model : Form.Model
        {
            public CreditCardData Card;
            public AddressData Address;
        }

        protected override void Awake()
        {

        }

        public override void Refresh()
        {
            base.Open();
            Refresh();
        }

        public override void Bind()
        {

        }

        public override void Open()
        {

        }


        public override Model GetDefaultDataObject()
        {
            var model = new Model() { Card = new CreditCardData() { ExpirationMonth = DateTime.Now.Month, ExpirationYear = DateTime.Now.Year }, Address = new AddressData() { Line2 = "" } };
            model.Card.Reset();

            return model;
        }*/

    }
}
