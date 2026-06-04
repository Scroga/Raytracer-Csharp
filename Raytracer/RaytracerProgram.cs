using System.Diagnostics;
using System.Globalization;
using CommandLine;
using Raytracer.Environment.Scene;
using Raytracer.Systems;

namespace Raytracer;
internal class RaytracerProgram
{
    static void Main(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        try
        {
            Parser.Default.ParseArguments<ArgsParser>(args)
                .WithParsed<ArgsParser>(argsParser =>
                {
                    var rendererConfig = JsonFileReader.Deserialize<RaytracerAlgorithmConfig>(argsParser.RendererConfigPath)!;
                    var sceneConfig = JsonFileReader.Deserialize<SceneConfig>(argsParser.SceneConfigPath)!;

                    var renderer = new RayTracingRenderer(rendererConfig, argsParser.Verbose);
                    var scene = new Scene(sceneConfig);

                    if (argsParser.Verbose)
                    {
                        Console.WriteLine("------ Renderer Stats  ------");
                        Console.WriteLine(renderer.ToString());

                        Console.WriteLine("------ Scene Hierarchy ------");
                        Console.WriteLine(scene.RootNode.ToString());
                    }

                    var image = new FloatImage(scene.Camera.Width, scene.Camera.Height, 3);

                    var stopwatch = Stopwatch.StartNew();
                    renderer.Render(scene, image);
                    stopwatch.Stop();

                    if (argsParser.Verbose) {
                        Console.WriteLine("------ Render Time  ------");
                        Console.WriteLine($"RenderScene took {stopwatch.ElapsedMilliseconds} ms");
                    }

                    string? directory = Path.GetDirectoryName(argsParser.OutputFileName);
                    if (!string.IsNullOrEmpty(directory))
                        Directory.CreateDirectory(directory);

                    if (argsParser.OutputFileName.EndsWith(".hdr"))
                        image.SaveHDR(argsParser.OutputFileName);
                    else
                        image.SavePFM(argsParser.OutputFileName);

                    if (argsParser.Verbose)
                    {
                        Console.WriteLine($"\nImage '{argsParser.OutputFileName}' is finished.");
                    }

                });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
