using Dalamud.Game.ClientState;
using Dalamud.Game.Network.Structures;
using Dalamud.Logging;
using Dalamud.Utility;
using Lumina;
using MarketUploader.Uploaders.XivHub.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MarketUploader.Uploaders.XivHub
{
    public class XivHubUploader : IMarketBoardUploader
    {
        private HttpClient httpClient = new();

        public XivHubUploader()
        {
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MarketUploader/1.0.5");
            httpClient.Timeout = TimeSpan.FromSeconds(4);
        }

        public async Task Upload(string baseUrl, MarketBoardItemRequest request, ClientState clientState)
        {
            PluginLog.Verbose($"Starting XivHub based upload to {baseUrl}");

            SHA256 hasher = SHA256.Create();
            var uploader = hasher.ComputeHash(BitConverter.GetBytes(clientState.LocalContentId));
            hasher.Clear();
            var uploaderId = Convert.ToHexString(uploader);

            // ====================================================================================

            var listingsUploadObject = new ItemListingUpload
            {
                WorldId = clientState.LocalPlayer?.CurrentWorld.Id ?? 0,
                UploaderId = uploaderId,
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

            // ==========

            var historyUploadObject = new HistoryUpload
            {
                WorldId = clientState.LocalPlayer?.CurrentWorld.Id ?? 0,
                UploaderId = uploaderId,
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

            // Upload
            var listingUpload = JsonConvert.SerializeObject(listingsUploadObject);
            PluginLog.Verbose($"Uploading ({baseUrl}): {listingUpload}");
            var res = await httpClient.PostAsync($"{baseUrl}/upload", new StringContent(listingUpload, Encoding.UTF8, "application/json"));
            PluginLog.Verbose(res.ToString());
            var resText = await res.Content.ReadAsStringAsync();
            PluginLog.Verbose(resText);

            var historyUpload = JsonConvert.SerializeObject(historyUploadObject);
            PluginLog.Verbose($"Upload history ({baseUrl}): {historyUpload}");
            var resHistory = await httpClient.PostAsync($"{baseUrl}/history", new StringContent(historyUpload, Encoding.UTF8, "application/json"));
            PluginLog.Verbose(resHistory.ToString());
        }

        public async Task UploadPurchase(string baseUrl, MarketBoardPurchaseHandler purchaseHandler, ClientState clientState)
        {
            //throw new NotImplementedException();
        }

        public async Task UploadTax(string baseUrl, MarketTaxRates taxRates, ClientState clientState)
        {
            //throw new NotImplementedException();
        }
    }
}
