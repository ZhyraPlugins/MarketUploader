using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketUploader.Uploaders.XivHub.Types
{
    internal class HistoryEntry
    {
        /// <summary>
        /// Gets or sets a value indicating whether the item is HQ or not.
        /// </summary>
        [JsonProperty("hq")]
        public bool Hq { get; set; }

        /// <summary>
        /// Gets or sets the item price per unit.
        /// </summary>
        [JsonProperty("price_per_unit")]
        public uint PricePerUnit { get; set; }

        /// <summary>
        /// Gets or sets the quantity of items available.
        /// </summary>
        [JsonProperty("quantity")]
        public uint Quantity { get; set; }

        /// <summary>
        /// Gets or sets the name of the buyer.
        /// </summary>
        [JsonProperty("buyer_name")]
        public string BuyerName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this item was on a mannequin.
        /// </summary>
        [JsonProperty("on_mannequin")]
        public bool OnMannequin { get; set; }

        /// <summary>
        /// Gets or sets the seller ID.
        /// </summary>
        [JsonProperty("seller_id")]
        public string SellerId { get; set; }

        /// <summary>
        /// Gets or sets the buyer ID.
        /// </summary>
        [JsonProperty("buyer_id")]
        public string BuyerId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the transaction.
        /// </summary>
        [JsonProperty("purchase_time")]
        public long PurchaseTime { get; set; }
    }
}
