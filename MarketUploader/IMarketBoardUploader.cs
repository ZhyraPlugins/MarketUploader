using Dalamud.Game.ClientState;
using Dalamud.Game.Network.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketUploader
{
    public interface IMarketBoardUploader
    {
        /// <summary>
        /// Upload data about an item.
        /// </summary>
        /// <param name="item">The item request data being uploaded.</param>
        /// <returns>An async task.</returns>
        Task Upload(string baseUrl, MarketBoardItemRequest item, ClientState clientState);

        /// <summary>
        /// Upload tax rate data.
        /// </summary>
        /// <param name="taxRates">The tax rate data being uploaded.</param>
        /// <returns>An async task.</returns>
        Task UploadTax(string baseUrl, MarketTaxRates taxRates, ClientState clientState);

        /// <summary>
        /// Upload information about a purchase this client has made.
        /// </summary>
        /// <param name="purchaseHandler">The purchase handler data associated with the sale.</param>
        /// <returns>An async task.</returns>
        Task UploadPurchase(string baseUrl, MarketBoardPurchaseHandler purchaseHandler, ClientState clientState);
    }
}
