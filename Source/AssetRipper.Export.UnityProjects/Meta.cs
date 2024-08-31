using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.SourceGenerated.Classes.ClassID_130;
using AssetRipper.Yaml;


namespace AssetRipper.Export.UnityProjects
{
	public readonly struct Meta
	{
		public Meta(UnityGuid guid, IUnityObjectBase importer) : this(guid, importer, true) { }

		public Meta(UnityGuid guid, IUnityObjectBase importer, bool hasLicense) : this(guid, importer, hasLicense, false) { }

		public Meta(UnityGuid guid, IUnityObjectBase importer, IUnityObjectBase? unityObject) : this(guid, importer, true, false, unityObject) { }

		public Meta(UnityGuid guid, IUnityObjectBase importer, bool hasLicense, bool isFolder, IUnityObjectBase? unityObject = null)
		{
			if (guid.IsZero)
			{
				throw new ArgumentNullException(nameof(guid));
			}

			GUID = guid;
			IsFolderAsset = isFolder;
			HasLicenseData = hasLicense;
			Importer = importer ?? throw new ArgumentNullException(nameof(importer));
			UnityObject = unityObject;
		}

		private static int ToFileFormatVersion()
		{
			//This has been 2 for a long time, but probably not forever.
			//If Unity 3 usesd version 1, we need to find out when 2 started.
			return 2;
		}

		public YamlDocument ExportYamlDocument(IExportContainer container)
		{
			YamlDocument document = new();
			YamlMappingNode root = document.CreateMappingRoot();
			root.Add(FileFormatVersionName, ToFileFormatVersion());
			root.Add(GuidName, GUID.ToString());
			if (IsFolderAsset)
			{
				root.Add(FolderAssetName, true);
			}
			if (HasLicenseData)
			{
				root.Add(LicenseTypeName, "Free");
			}
			if (UnityObject is not null)
			{
				if (UnityObject.OriginalName is not null)
				{
					root.Add(CatalogHashRefName, UnityObject.OriginalName);
				}
				if (UnityObject is INamedObject namedObject)
				{
					root.Add(AssetObjectName, namedObject.Name);
				}
			}
			//if (Importer.IncludesImporter(container.ExportVersion)) //For now, assume true
			{
				root.Add(Importer.ClassName, new ProjectYamlWalker(container){ ExportingAssetImporter = true }.ExportYamlNode(Importer));
			}
			return document;
		}

		public UnityGuid GUID { get; }
		public bool IsFolderAsset { get; }
		public bool HasLicenseData { get; }
		public IUnityObjectBase Importer { get; }
		public IUnityObjectBase? UnityObject { get; }

		public const string FileFormatVersionName = "fileFormatVersion";
		public const string GuidName = "guid";
		public const string FolderAssetName = "folderAsset";
		public const string LicenseTypeName = "licenseType";
		public const string CatalogHashRefName = "catalogHashRef";
		public const string AssetObjectName = "assetObject";
	}
}
