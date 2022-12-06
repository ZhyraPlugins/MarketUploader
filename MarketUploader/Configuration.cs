using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace MarketUploader
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 5;

        public bool ChangedDefaultConfig = false;

        // For now just save a list of aggregators, they all must implement the api endpoints as done with the XivHub uploader implementation.
        // In the future there can be more implementations and allow to choose which for each aggregator.

        public List<string> Aggregators { get; set; } = new List<string>();

        public int UploadCount { get; set; } = 0;

        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? PluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;

            if(!this.ChangedDefaultConfig)
            {
                // Needed to have a proper default list.

                Aggregators.Add("https://market.xivhub.org/api");

                this.ChangedDefaultConfig = true;
                Save();
            }
        }

        public void Reset()
        {
            Aggregators = new List<string> { "https://market.xivhub.org/api" };
        }

        public void Save()
        {
            this.PluginInterface!.SavePluginConfig(this);
        }
    }
}
