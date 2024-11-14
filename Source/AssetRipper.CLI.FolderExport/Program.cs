using AssetRipper.Export.UnityProjects;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Import.Configuration;
using AssetRipper.Primitives;
using AssetRipper.Processing;
using AssetRipper.Processing.Configuration;
using System.CommandLine;
using System.Diagnostics;

public class Program
{
	private static readonly string DefaultUnityVersion = "2022.3.22f1";

	public static void Main(string[] args)
	{
		Console.WriteLine(Launch(args) ? "Files exported successfully." : "There was an error exporting the files.");
	}

	public static bool Launch(string[] args)
	{
		RootCommand rootCommand = [];

		Option<string> folderOption = new(
			"--folder",
			"Path to the input folder")
		{
			IsRequired = true
		};
		rootCommand.AddOption(folderOption);

		Option<string> outputOption = new(
			"--output",
			"Path to the output folder")
		{
			IsRequired = true
		};
		rootCommand.AddOption(outputOption);

		Option<string> unityVersionOption = new(
			"--unity-version",
			() => DefaultUnityVersion,
			$"Unity version to use (default is {DefaultUnityVersion})");
		rootCommand.AddOption(unityVersionOption);

		bool result = false;

		rootCommand.SetHandler((string folderPath, string outputPath, string unityVersion) =>
		{
			Stopwatch stopwatch = new();
			stopwatch.Start();
			DirectoryInfo folder = new(folderPath);
			DirectoryInfo output = new(outputPath);

			if (!folder.Exists)
			{
				Console.WriteLine($"The specified input folder '{folder.FullName}' does not exist.");
				return;
			}

			if (!output.Exists)
			{
				Console.WriteLine($"The specified output folder '{output.FullName}' does not exist.");
				return;
			}

			Console.WriteLine($"Input folder: {folder.FullName}");
			Console.WriteLine($"Output folder: {output.FullName}");
			Console.WriteLine($"Unity version: {unityVersion}");
			Console.WriteLine("Initializing...");

			LibraryConfiguration settings = new();
			settings.LoadFromDefaultPath();
			ExportHandler exportHandler = new(LoadSettings(unityVersion));

			GameData gameData = exportHandler.LoadAndProcess(new string[] { folder.FullName });

			exportHandler.Export(gameData, output.FullName);

			stopwatch.Stop();
			TimeSpan ts = stopwatch.Elapsed;

			string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
				ts.Hours, ts.Minutes, ts.Seconds,
				ts.Milliseconds / 10);

			Console.WriteLine($"Process finished in {elapsedTime}");

			result = true;

		}, folderOption, outputOption, unityVersionOption);

		rootCommand.Invoke(args);

		return result;
	}

	private static LibraryConfiguration LoadSettings(string unityVersion)
	{
		LibraryConfiguration settings = new();
		settings.LoadFromDefaultPath();
		settings.ProcessingSettings.BundledAssetsExportMode = BundledAssetsExportMode.GroupByBundleName;
		settings.ImportSettings.DefaultVersion = UnityVersion.Parse(unityVersion);
		settings.ImportSettings.TargetVersion = UnityVersion.Parse(unityVersion);
		return settings;
	}
}
