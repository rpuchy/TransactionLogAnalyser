using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionLog_Analyser
{
    public class Transaction
    {
        public string StateChange { get; set; }
        public string TransactionType { get; set; }
        public long priority { get; set; }
        public string Action { get; set; }
        public string Portfolio { get; set; }
        public string TaxWrapper { get; set; }
        public string Product { get; set; }
        public string Asset_Liability { get; set; }
        public string StateCounter { get; set; }
        public string TransactionCounter { get; set; }
        public long Scenario { get; set; }
        public long TimeStep { get; set; }
        public double Sell { get; set; }
        public double Buy { get; set; }
        public double Price { get; set; }
        public bool UseSellPriceToBuy { get; set; }
        public double Holdings { get; set; }
        public double SellValue { get; set; }
    }
}
