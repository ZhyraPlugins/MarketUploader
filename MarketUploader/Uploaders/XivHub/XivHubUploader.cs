using Dalamud.Game.ClientState;
using Dalamud.Game.Network.Structures;
using Dalamud.Logging;
using Dalamud.Utility;
using MarketUploader.Uploaders.XivHub.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MarketUploader.Uploaders.XivHub
{
    public class XivHubUploader : IMarketBoardUploader
    {
        private const string ApiBase = "http://localhost:3000";

        public XivHubUploader()
        {

        }

        public async Task Upload(MarketBoardItemRequest request, ClientState clientState)
        {
            PluginLog.Verbose("Starting XivHub upload.");
            var uploader = clientState.LocalContentId;

            // ====================================================================================

            var listingsUploadObject = new ItemListingUpload
            {
                WorldId = clientState.LocalPlayer?.CurrentWorld.Id ?? 0,
                UploaderId = uploader.ToString(),
                ItemId = request.CatalogId,
                Listings = new List<ItemListingsEntry>(),
            };

            foreach (var marketBoardItemListing in request.Listings)
            {
                
                var listing = new ItemListingsEntry
                {
                    ListingId = marketBoardItemListing.ListingId,
                    Hq = marketBoardItemListing.IsHq,
                    SellerId = marketBoardItemListing.RetainerOwnerId.ToString(),
                    RetainerName = marketBoardItemListing.RetainerName,
                    RetainerId = marketBoardItemListing.RetainerId.ToString(),
                    CreatorId = marketBoardItemListing.ArtisanId.ToString(),
                    CreatorName = marketBoardItemListing.PlayerName,
                    OnMannequin = marketBoardItemListing.OnMannequin,
                    LastReviewTime = ((DateTimeOffset)marketBoardItemListing.LastReviewTime).ToUnixTimeSeconds(),
                    PricePerUnit = marketBoardItemListing.PricePerUnit,
                    Quantity = marketBoardItemListing.ItemQuantity,
                    RetainerCity = marketBoardItemListing.RetainerCityId,
                    Materia = new List<ItemMateria>(),
                };

                foreach (var itemMateria in marketBoardItemListing.Materia)
                {
                    listing.Materia.Add(new ItemMateria
                    {
                        MateriaId = itemMateria.MateriaId,
                        SlotId = itemMateria.Index,
                    });
                }

                listingsUploadObject.Listings.Add(listing);
            }

            var listingUpload = JsonConvert.SerializeObject(listingsUploadObject);
            PluginLog.Verbose($"Uploading: {listingUpload}");
            await Util.HttpClient.PostAsync($"{ApiBase}/upload", new StringContent(listingUpload, Encoding.UTF8, "application/json"));

            // ==========

            var historyUploadObject = new HistoryUpload
            {
                WorldId = clientState.LocalPlayer?.CurrentWorld.Id ?? 0,
                UploaderId = uploader.ToString(),
                ItemId = request.CatalogId,
                Listings = new List<HistoryEntry>(),
            };

            foreach (var marketBoardHistoryListing in request.History)
            {
                historyUploadObject.Listings.Add(new HistoryEntry
                {
                    BuyerName = marketBoardHistoryListing.BuyerName,
                    Hq = marketBoardHistoryListing.IsHq,
                    OnMannequin = marketBoardHistoryListing.OnMannequin,
                    PricePerUnit = marketBoardHistoryListing.SalePrice,
                    Quantity = marketBoardHistoryListing.Quantity,
                    PurchaseTime = ((DateTimeOffset)marketBoardHistoryListing.PurchaseTime).ToUnixTimeSeconds(),
                });
            }

            var historyUpload = JsonConvert.SerializeObject(historyUploadObject);
            PluginLog.Verbose($"Upload history: {historyUpload}");
            await Util.HttpClient.PostAsync($"{ApiBase}/history", new StringContent(historyUpload, Encoding.UTF8, "application/json"));
        }

        public async Task UploadPurchase(MarketBoardPurchaseHandler purchaseHandler, ClientState clientState)
        {
            //throw new NotImplementedException();
        }

        public async Task UploadTax(MarketTaxRates taxRates, ClientState clientState)
        {
            //throw new NotImplementedException();
        }
    }
}
