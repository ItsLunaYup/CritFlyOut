using Dalamud.Configuration;
using Dalamud.Plugin;
using System.Diagnostics.CodeAnalysis;

namespace SamplePlugin;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    [NonSerialized]
    private IDalamudPluginInterface? pluginInterface;

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
    }

    public void Save()
    {
        this.pluginInterface!.SavePluginConfig(this);
    }
}

