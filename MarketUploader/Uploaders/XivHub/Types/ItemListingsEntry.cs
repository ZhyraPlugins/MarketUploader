using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketUploader.Uploaders.XivHub.Types
{
    internal class ItemListingsEntry
    {
        /// <summary>
        /// Gets or sets the listing ID.
        /// </summary>
        [JsonProperty("listing_id")]
        public ulong ListingId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the item is HQ.
        /// </summary>
        [JsonProperty("hq")]
        public bool Hq { get; set; }

        /// <summary>
        /// Gets or sets the item price per unit.
        /// </summary>
        [JsonProperty("price_per_unit")]
        public uint PricePerUnit { get; set; }

        /// <summary>
        /// Gets or sets the item quantity.
        /// </summary>
        [JsonProperty("quantity")]
        public uint Quantity { get; set; }

        /// <summary>
        /// Gets or sets the name of the retainer selling the item.
        /// </summary>
        [JsonProperty("retainer_name")]
        public string RetainerName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the retainer selling the item.
        /// </summary>
        [JsonProperty("retainer_id")]
        public string RetainerId { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who created the entry.
        /// </summary>
        [JsonProperty("creator_name")]
        public string CreatorName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the item is on a mannequin.
        /// </summary>
        [JsonProperty("on_mannequin")]
        public bool OnMannequin { get; set; }

        /// <summary>
        /// Gets or sets the seller ID.
        /// </summary>
        [JsonProperty("seller_id")]
        public string SellerId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created the entry.
        /// </summary>
        [JsonProperty("creator_id")]
        public string CreatorId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the dye on the item.
        /// </summary>
        [JsonProperty("stain_id")]
        public int StainId { get; set; }

        /// <summary>
        /// Gets or sets the city where the selling retainer resides.
        /// </summary>
        [JsonProperty("retainer_city")]
        public int RetainerCity { get; set; }

        /// <summary>
        /// Gets or sets the last time the entry was reviewed.
        /// </summary>
        [JsonProperty("last_review_time")]
        public long LastReviewTime { get; set; }

        /// <summary>
        /// Gets or sets the materia attached to the item.
        /// </summary>
        [JsonProperty("materia")]
        public List<ItemMateria> Materia { get; set; }
    }
}
