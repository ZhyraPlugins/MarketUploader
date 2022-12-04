using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketUploader.Uploaders.XivHub.Types
{
    internal class ItemMateria
    {
        /// <summary>
        /// Gets or sets the item slot ID.
        /// </summary>
        [JsonProperty("slot_id")]
        public int SlotId { get; set; }

        /// <summary>
        /// Gets or sets the materia ID.
        /// </summary>
        [JsonProperty("materia_id")]
        public int MateriaId { get; set; }
    }
}
