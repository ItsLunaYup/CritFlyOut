using System.Diagnostics.CodeAnalysis;
using Dalamud.Plugin;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using Dalamud.Configuration;
using Dalamud.Game.Command;
using Dalamud.IoC;
using ImGuiNET;

namespace CritFlyOut;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public sealed class CritFlyOutPlugin : IDalamudPlugin
{
    [PluginService] public static ICommandManager Commands { get; private set; } = null!;

    private const string CommandName = "/crittext";
    
    public string Name => "Crit Fly Out";

    private IDalamudPluginInterface PluginInterface { get; init; }
    private IFlyTextGui FlyTextGui { get; init; }

    private PluginConfig Config { get; init; }

    private bool showConfig = false;

    public CritFlyOutPlugin(IDalamudPluginInterface pluginInterface, IFlyTextGui flyTextGui)
    {
        PluginInterface = pluginInterface;
        FlyTextGui = flyTextGui;

        Config = PluginInterface.GetPluginConfig() as PluginConfig ?? new PluginConfig();
        Config.Initialize(PluginInterface);

        FlyTextGui.FlyTextCreated += FlyTextGUI_FlyTextCreated;
        PluginInterface.UiBuilder.Draw += DrawUi;
        PluginInterface.UiBuilder.OpenConfigUi += () => showConfig = true;
        PluginInterface.UiBuilder.OpenMainUi += OpenMainUi;
        
        
        Commands.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens the CritFlyOut plugin main UI"
        });
    }

    private void FlyTextGUI_FlyTextCreated(
        ref FlyTextKind kind, ref int val1, ref int val2, ref SeString text1, ref SeString text2, ref uint color,
        ref uint icon, ref uint damageTypeIcon, ref float yOffset, ref bool handled)
    {
        if (!Config.EnableReplacement)
            return;
        
        text2 = kind switch
        {
            FlyTextKind.DamageCrit => new SeStringBuilder().AddText(Config.CritText).Build(),
            FlyTextKind.DamageDh => new SeStringBuilder().AddText(Config.DhText).Build(),
            FlyTextKind.DamageCritDh => new SeStringBuilder().AddText(Config.CritDhText).Build(),
            _ => text2
        };
        
        
    }

    private void DrawUi()
    {
        if (!showConfig)
            return;

        ImGui.Begin("Crit Fly Out Config", ref showConfig, ImGuiWindowFlags.AlwaysAutoResize);

        ImGui.Text("Crit Fly Out Settings");

        var enabled = Config.EnableReplacement;
        if (ImGui.Checkbox("Enable Text Replacement", ref enabled))
        {
            Config.EnableReplacement = enabled;
            Config.Save();
        }

        var crit = Config.CritText;
        if (ImGui.InputText("Crit Text", ref crit, 64))
        {
            Config.CritText = crit;
            Config.Save();
        }

        var dh = Config.DhText;
        if (ImGui.InputText("Direct Hit Text", ref dh, 64))
        {
            Config.DhText = dh;
            Config.Save();
        }

        var critdh = Config.CritDhText;
        if (ImGui.InputText("Crit+DH Text", ref critdh, 64))
        {
            Config.CritDhText = critdh;
            Config.Save();
        }
        
        ImGui.End();
    }

    public void Dispose()
    {
        FlyTextGui.FlyTextCreated -= FlyTextGUI_FlyTextCreated;
        PluginInterface.UiBuilder.Draw -= DrawUi;
    }
    
    private void OpenMainUi()
    {
        showConfig = !showConfig; 
    }
    
    private void OnCommand(string command, string args)
    {
        showConfig = !showConfig;
    }
}

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class PluginConfig : IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public bool EnableReplacement = true;
    public string CritText = "CRIT";
    public string DhText = "Mini-CRIT";
    public string CritDhText = "MASSIVE CRIT";

    [NonSerialized]
    private IDalamudPluginInterface? pluginInterface;

    public void Initialize(IDalamudPluginInterface pi)
    {
        pluginInterface = pi;
    }

    public void Save()
    {
        pluginInterface?.SavePluginConfig(this);
    }
}
