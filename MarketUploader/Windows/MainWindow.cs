using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using ImGuiScene;

namespace MarketUploader.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin plugin;

    public MainWindow(Plugin plugin) : base(
        "Market Uploader", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(100, 100),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        this.plugin = plugin;
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        ImGui.TextWrapped("While having this plugin enabled, you will automatically upload data from the market board to the configured aggregators.");

        ImGui.Spacing();
        ImGui.Text($"Total uploads: {this.plugin.Configuration.UploadCount}");

        ImGui.Spacing();
        ImGui.Spacing();

        if (ImGui.Button("XivHub Discord"))
        {
            Util.OpenLink("https://discord.gg/GZK9aME8wN");
        }

        if (ImGui.Button("market.xivhub.com"))
        {
            Util.OpenLink("https://market.xivhub.org");
        }

        ImGui.Text("Made by XivHub developers.");
    }
}
