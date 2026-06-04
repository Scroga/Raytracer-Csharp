using CommandLine;

namespace Raytracer.Systems;

public class ArgsParser
{
    [Option('o', "output", Required = false, Default = "Output/output.pfm", HelpText = "Output image name.")]
    public string OutputFileName{ get; init; } = "";

    [Option('c', "config", Required = false, Default = "Configs/default.json", HelpText = "Path to .json file with rederer algorithm configs.")]
    public string RendererConfigPath { get; init; } = "";

    [Option('s', "scene", Required = false, Default = "Configs/Scenes/Spheres.json", HelpText = "Path to .json file with scene configs.")]
    public string SceneConfigPath { get; init; } = "";

    [Option('v', "verbose", Required = false, Default = false, HelpText = "Provides detailed information.")]
    public bool Verbose { get; init; } = false;
}