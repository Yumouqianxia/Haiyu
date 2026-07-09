using Waves.Core.Models;

namespace Project.Test;

[TestClass]
public class GameLocalConfigTests
{
    [TestMethod]
    public async Task SaveConfigsAsync_PreservesExistingKeys()
    {
        var path = CreateTempSettingsPath();
        var config = new GameLocalConfig(path);

        await config.SaveConfigAsync(GameLocalSettingName.GameLauncherBassFolder, "D:\\Games\\Waves");
        await config.SaveConfigsAsync(
            new Dictionary<string, string>
            {
                [GameLocalSettingName.LocalGameVersion] = "3.5",
                [GameLocalSettingName.ProdIsAdvance] = "True",
            }
        );

        Assert.AreEqual("D:\\Games\\Waves", await config.GetConfigAsync(GameLocalSettingName.GameLauncherBassFolder));
        Assert.AreEqual("3.5", await config.GetConfigAsync(GameLocalSettingName.LocalGameVersion));
        Assert.AreEqual("True", await config.GetConfigAsync(GameLocalSettingName.ProdIsAdvance));
    }

    [TestMethod]
    public async Task SaveConfigsAsync_DoesNotRollBackWhenReadsAreConcurrent()
    {
        var path = CreateTempSettingsPath();
        var config = new GameLocalConfig(path);
        await config.SaveConfigAsync(GameLocalSettingName.LocalGameVersion, "3.4.1");

        var readers = Enumerable
            .Range(0, 100)
            .Select(async index =>
            {
                for (var i = 0; i < 20; i++)
                {
                    var ignored = await config.GetConfigAsync(GameLocalSettingName.LocalGameVersion);
                }
            });

        await Task.WhenAll(
            readers.Append(
                config.SaveConfigsAsync(
                    new Dictionary<string, string>
                    {
                        [GameLocalSettingName.LocalGameVersion] = "3.5",
                        [GameLocalSettingName.ProdIsAdvance] = "True",
                        [GameLocalSettingName.ProdDownloadFolderDone] = "False",
                    }
                )
            )
        );

        Assert.AreEqual("3.5", await config.GetConfigAsync(GameLocalSettingName.LocalGameVersion));
        Assert.AreEqual("True", await config.GetConfigAsync(GameLocalSettingName.ProdIsAdvance));
        Assert.AreEqual("False", await config.GetConfigAsync(GameLocalSettingName.ProdDownloadFolderDone));
    }

    [TestMethod]
    public async Task GetConfigAsync_ReloadsWhenLocalFileChanges()
    {
        var path = CreateTempSettingsPath();
        var config = new GameLocalConfig(path);
        await config.SaveConfigAsync(GameLocalSettingName.LocalGameVersion, "3.5");

        await File.WriteAllTextAsync(
            path,
            """
            {
              "LocalGameVersion": "3.6.0"
            }
            """
        );

        Assert.AreEqual("3.6.0", await config.GetConfigAsync(GameLocalSettingName.LocalGameVersion));
    }

    private static string CreateTempSettingsPath()
    {
        var directory = Path.Combine(Path.GetTempPath(), "Haiyu.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "Settings.bat");
    }
}
