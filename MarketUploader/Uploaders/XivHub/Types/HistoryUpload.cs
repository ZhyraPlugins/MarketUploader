using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketUploader.Uploaders.XivHub.Types
{
    internal class HistoryUpload
    {
        /// <summary>
        /// Gets or sets the world ID.
        /// </summary>
        [JsonProperty("world_id")]
        public uint WorldId { get; set; }

        /// <summary>
        /// Gets or sets the item ID.
        /// </summary>
        [JsonProperty("item_id")]
        public uint ItemId { get; set; }

        /// <summary>
        /// Gets or sets the list of available entries.
        /// </summary>
        [JsonProperty("listings")]
        public List<HistoryEntry> Listings { get; set; }

        /// <summary>
        /// Gets or sets the uploader ID.
        /// </summary>
        [JsonProperty("uploader_id")]
        public string UploaderId { get; set; }
    }
}
