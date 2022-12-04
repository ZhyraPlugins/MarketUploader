using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketUploader.Uploaders.XivHub.Types
{
    internal class TaxUploadRequest
    {
        /// <summary>
        /// Gets or sets the uploader's ID.
        /// </summary>
        [JsonProperty("uploader_id")]
        public string UploaderId { get; set; }

        /// <summary>
        /// Gets or sets the world to retrieve data from.
        /// </summary>
        [JsonProperty("world_id")]
        public uint WorldId { get; set; }

        /// <summary>
        /// Gets or sets tax data for each city's market.
        /// </summary>
        [JsonProperty("market_tax_rates")]
        public TaxData TaxData { get; set; }
    }
}
