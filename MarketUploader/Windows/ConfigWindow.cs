using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;

namespace MarketUploader.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration configuration;
    private string tempUrl = "";

    public ConfigWindow(Plugin plugin) : base(
        "MarketUploader Configuration")
    {
        this.Size = new Vector2(400, 400);
        this.SizeCondition = ImGuiCond.FirstUseEver;

        this.configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text("Aggregators:");

        for (var i = 0; i < this.configuration.Aggregators.Count; i++)
        {
            ImGui.PushID(i);

            var text = this.configuration.Aggregators[i];
            ImGui.Text(text);
            ImGui.SameLine();
            if(ImGui.Button("Delete"))
            {
                this.configuration.Aggregators.RemoveAt(i);
                this.configuration.Save();
            }

            ImGui.PopID();
        }

        var localTempUrl = this.tempUrl;

        ImGui.Spacing();
        ImGui.Text("Add aggregator:");
        ImGui.Spacing();
        if (ImGui.InputTextWithHint("API URL", "https://somedomain.com/api", ref localTempUrl, 1000))
        {
            this.tempUrl = localTempUrl;
        }
        ImGui.SameLine();

        if (ImGui.SmallButton("+") && !this.tempUrl.IsNullOrEmpty())
        {
            if(this.tempUrl.StartsWith("https://") || this.tempUrl.StartsWith("http://"))
            {
                var found = false;

                foreach (string url in this.configuration.Aggregators)
                {
                    if (this.tempUrl.Equals(url))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    this.configuration.Aggregators.Add(this.tempUrl);
                    this.tempUrl = "";
                    this.configuration.Save();
                }
            }
        }

        ImGui.Spacing();

        if (ImGui.Button("Reset defaults"))
        {
            this.configuration.Reset();
            this.configuration.Save();
        }
    }
}
