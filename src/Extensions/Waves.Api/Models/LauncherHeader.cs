using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Waves.Api.Models;

[JsonSerializable(typeof(LauncherHeader))]
[JsonSerializable(typeof(Social))]
public partial class LauncherHeaderContext : JsonSerializerContext { }

public class LauncherHeader
{
    [JsonPropertyName("switch")]
    public int? Switch { get; set; }

    [JsonPropertyName("social")]
    public List<Social> Social { get; set; }
}

public class Social
{
    public ICommand JumpCommand =>
        new RelayCommand(() =>
        {
            System.Diagnostics.Process.Start("explorer.exe", JumpUrl);
        });

    [JsonPropertyName("switch")]
    public int? Switch { get; set; }

    [JsonPropertyName("buttonSrc")]
    public string ButtonSrc { get; set; }

    [JsonPropertyName("buttonMd5")]
    public string ButtonMd5 { get; set; }

    [JsonPropertyName("jumpUrl")]
    public string JumpUrl { get; set; }

    [JsonPropertyName("jumpText")]
    public string JumpText { get; set; }

    [JsonPropertyName("qrCodeSrc")]
    public string QrCodeSrc { get; set; }

    [JsonPropertyName("qrCodeMd5")]
    public string QrCodeMd5 { get; set; }

    [JsonPropertyName("qrCodeText")]
    public string QrCodeText { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}
