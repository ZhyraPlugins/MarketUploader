using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using Dalamud.Interface.Windowing;
using MarketUploader.Windows;
using Dalamud.Game.Network;
using Dalamud.Logging;
using Dalamud.Data;
using Dalamud.Game.Network.Structures;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState;
using MarketUploader.Uploaders.XivHub;

namespace MarketUploader
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "XivHubMarket";
        private const string CommandName = "/xivmarket";

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("XivHubMarket");

        public GameNetwork Network;
        public DataManager DataManager;
        private ClientState ClientState;

        private readonly List<MarketBoardItemRequest> marketBoardRequests = new();
        private MarketBoardPurchaseHandler? marketBoardPurchaseHandler;

        private readonly List<IMarketBoardUploader> marketBoardUploaders= new();

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager,
            [RequiredVersion("1.0")] GameNetwork network,
            [RequiredVersion("1.0")] DataManager dataManager,
            [RequiredVersion("1.0")] ClientState clientState
           )
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            this.Network = network;
            this.DataManager = dataManager;
            this.ClientState = clientState;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            // you might normally want to embed resources and load them from the manifest stream
            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");
            var goatImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);

            WindowSystem.AddWindow(new ConfigWindow(this));
            WindowSystem.AddWindow(new MainWindow(this, goatImage));

            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "A useful message to display in /xlhelp"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            this.marketBoardUploaders.Add(new XivHubUploader());

            this.Network.NetworkMessage += OnNetworkMessage;
            this.marketBoardPurchaseHandler = null;
        }

        private void OnNetworkMessage(System.IntPtr dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction)
        {
            // up is from client to server.

            if (DataManager.IsDataReady != true)
                return;

            if (direction == NetworkMessageDirection.ZoneUp)
            {
                if (opCode == DataManager.ClientOpCodes["MarketBoardPurchaseHandler"])
                {
                    this.marketBoardPurchaseHandler = MarketBoardPurchaseHandler.Read(dataPtr);
                    return;
                }

                return;
            }

            if (opCode == DataManager.ServerOpCodes["MarketBoardItemRequestStart"])
            {
                var data = MarketBoardItemRequest.Read(dataPtr);
                this.marketBoardRequests.Add(data);

                PluginLog.Verbose($"NEW MB REQUEST START: item#{data.CatalogId} amount#{data.AmountToArrive}");
                return;

            }
            if (opCode == DataManager.ServerOpCodes["MarketBoardOfferings"])
            {
                var listing = MarketBoardCurrentOfferings.Read(dataPtr);

                var request = this.marketBoardRequests.LastOrDefault(r => r.CatalogId == listing.ItemListings[0].CatalogId && !r.IsDone);

                if (request == default)
                {
                    PluginLog.Error($"Market Board data arrived without a corresponding request: item#{listing.ItemListings[0].CatalogId}");
                    return;
                }

                if (request.Listings.Count + listing.ItemListings.Count > request.AmountToArrive)
                {
                    PluginLog.Error($"Too many Market Board listings received for request: {request.Listings.Count + listing.ItemListings.Count} > {request.AmountToArrive} item#{listing.ItemListings[0].CatalogId}");
                    return;
                }

                if (request.ListingsRequestId != -1 && request.ListingsRequestId != listing.RequestId)
                {
                    PluginLog.Error($"Non-matching RequestIds for Market Board data request: {request.ListingsRequestId}, {listing.RequestId}");
                    return;
                }

                if (request.ListingsRequestId == -1 && request.Listings.Count > 0)
                {
                    PluginLog.Error($"Market Board data request sequence break: {request.ListingsRequestId}, {request.Listings.Count}");
                    return;
                }

                if (request.ListingsRequestId == -1)
                {
                    request.ListingsRequestId = listing.RequestId;
                    PluginLog.Verbose($"First Market Board packet in sequence: {listing.RequestId}");
                }

                request.Listings.AddRange(listing.ItemListings);

                PluginLog.Verbose(
                    "Added {0} ItemListings to request#{1}, now {2}/{3}, item#{4}",
                    listing.ItemListings.Count,
                    request.ListingsRequestId,
                    request.Listings.Count,
                    request.AmountToArrive,
                    request.CatalogId);

                if (request.IsDone)
                {
                    PluginLog.Verbose(
                        "Market Board request finished, starting upload: request#{0} item#{1} amount#{2}",
                        request.ListingsRequestId,
                        request.CatalogId,
                        request.AmountToArrive);

                    
                    foreach(IMarketBoardUploader uploader in marketBoardUploaders) {
                        Task.Run(() => uploader.Upload(request, ClientState))
                        .ContinueWith((task) => PluginLog.Error(task.Exception, "Market Board offerings data upload failed."), TaskContinuationOptions.OnlyOnFaulted);
                    }
                   
                    
                }
            }

            if (opCode == DataManager.ServerOpCodes["MarketBoardHistory"])
            {
                var listing = MarketBoardHistory.Read(dataPtr);

                var request = this.marketBoardRequests.LastOrDefault(r => r.CatalogId == listing.CatalogId);

                if (request == default)
                {
                    PluginLog.Error($"Market Board data arrived without a corresponding request: item#{listing.CatalogId}");
                    return;
                }

                if (request.ListingsRequestId != -1)
                {
                    PluginLog.Error($"Market Board data history sequence break: {request.ListingsRequestId}, {request.Listings.Count}");
                    return;
                }

                request.History.AddRange(listing.HistoryListings);

                PluginLog.Verbose("Added history for item#{0}", listing.CatalogId);

                if (request.AmountToArrive == 0)
                {
                    PluginLog.Verbose("Request had 0 amount, uploading now");

                    foreach (IMarketBoardUploader uploader in marketBoardUploaders)
                    {
                        Task.Run(() => uploader.Upload(request, ClientState))
                        .ContinueWith((task) => PluginLog.Error(task.Exception, "Market Board history data upload failed."), TaskContinuationOptions.OnlyOnFaulted);
                    }
                }
            }

            if (opCode == DataManager.ServerOpCodes["MarketTaxRates"])
            {
                var category = (uint)Marshal.ReadInt32(dataPtr);

                // Result dialog packet does not contain market tax rates
                if (category != 720905)
                {
                    return;
                }

                var taxes = MarketTaxRates.Read(dataPtr);

                if (taxes.Category != 0xb0009)
                    return;

                PluginLog.Verbose(
                    "MarketTaxRates: limsa#{0} grid#{1} uldah#{2} ish#{3} kugane#{4} cr#{5} sh#{6}",
                    taxes.LimsaLominsaTax,
                    taxes.GridaniaTax,
                    taxes.UldahTax,
                    taxes.IshgardTax,
                    taxes.KuganeTax,
                    taxes.CrystariumTax,
                    taxes.SharlayanTax);

                /*
                Task.Run(() => this.uploader.UploadTax(taxes))
                    .ContinueWith((task) => Log.Error(task.Exception, "Market Board tax data upload failed."), TaskContinuationOptions.OnlyOnFaulted);
                */

                return;
            }

            if (opCode == DataManager.ServerOpCodes["MarketBoardPurchase"])
            {
                if (this.marketBoardPurchaseHandler == null)
                    return;

                var purchase = MarketBoardPurchase.Read(dataPtr);

                var sameQty = purchase.ItemQuantity == this.marketBoardPurchaseHandler.ItemQuantity;
                var itemMatch = purchase.CatalogId == this.marketBoardPurchaseHandler.CatalogId;
                var itemMatchHq = purchase.CatalogId == this.marketBoardPurchaseHandler.CatalogId + 1_000_000;

                // Transaction succeeded
                if (sameQty && (itemMatch || itemMatchHq))
                {
                    PluginLog.Verbose($"Bought {purchase.ItemQuantity}x {this.marketBoardPurchaseHandler.CatalogId} for {this.marketBoardPurchaseHandler.PricePerUnit * purchase.ItemQuantity} gils, listing id is {this.marketBoardPurchaseHandler.ListingId}");

                    var handler = this.marketBoardPurchaseHandler; // Capture the object so that we don't pass in a null one when the task starts.

                    foreach (IMarketBoardUploader uploader in marketBoardUploaders)
                    {
                        Task.Run(() => uploader.UploadPurchase(handler, ClientState))
                        .ContinueWith((task) => PluginLog.Error(task.Exception, "Market Board purchase data upload failed."), TaskContinuationOptions.OnlyOnFaulted);
                    }
                }

                this.marketBoardPurchaseHandler = null;
                return;
            }
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            this.CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            WindowSystem.GetWindow("My Amazing Window").IsOpen = true;
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            WindowSystem.GetWindow("A Wonderful Configuration Window").IsOpen = true;
        }
    }
}
