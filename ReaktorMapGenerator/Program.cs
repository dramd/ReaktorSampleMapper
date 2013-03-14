using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ReaktorMapGenerator.Reaktor;
using System.Collections.Specialized;
using MongoDB.Bson;
namespace ReaktorMapGenerator
{
	public enum Platform
	{
		Windows,
		Unix,
		Mac
	}

    public static class Program
    {

		static BsonDocument Config;
        static void Main(string[] args)
        {
			SetupConfig ();
			SampleMap map = new SampleMap(Config["filename"].AsString,Config["append"].AsBoolean);
			long totalSize = 0;
			if (Config ["tasks"].AsBsonArray.Contains ("add")) {
				totalSize =  AddFilesToMap (map);
			}

			if(Config ["tasks"].AsBsonArray.Contains ("map")) {
				map.Serialize();
				long hrts = (totalSize / 1024)/1024;
				Console.WriteLine("Built Map with {0} samples, added {1}mb", map.samples.Count, hrts);
			}
			if (Config ["tasks"].AsBsonArray.Contains ("json")) {
				File.WriteAllText(Path.ChangeExtension(Config["filename"].AsString,"json"),map.ToBson().ToPrettyJson());
				Console.WriteLine ("Wrote JSON Map to {0}",Path.ChangeExtension(Config["filename"].AsString,"json"));
			}
			System.Threading.Thread.Sleep(2000);
        }


		static void SetupConfig() {
			try {
				Config = BsonDocument.Parse(File.ReadAllText(Environment.CurrentDirectory+Path.DirectorySeparatorChar+"config.json"));
			} catch (Exception e) {
				Console.WriteLine ("Writing default config.json");
				Config = new BsonDocument() {
					{"filename","/Users/ash/Music/example.map"},
					{"tasks",new BsonArray { "add","sort","json","map" } },
					{"append",false},
					{"sampleDir", "/Users/ash/Music/Samples/"},
					{"recurse", true},
					{"searchTerms","*.wav"},
					{"filters",new BsonDocument {
							{"caseInsensitive", true},
							{"excludes",new BsonArray {"fill","combo","_","rock","nokick","session","add on","no kick","kick loops","fx","stick","toms","6-8"} },
							{"requires",new BsonArray {"Drum"} },
					}
					}
				};
				File.WriteAllText(Environment.CurrentDirectory+Path.DirectorySeparatorChar+"config.json",Config.ToPrettyJson());
			
			}
		
		}
		static long AddFilesToMap(SampleMap map) {

			

			BsonDocument filters = Config ["filters"].AsBsonDocument;
			BsonArray tasks = Config ["tasks"].AsBsonArray;
			List<KeyValuePair<string,string>> fileList = new List<KeyValuePair<string,string>> ();
			SearchOption searchMode = SearchOption.TopDirectoryOnly;
			try {
				if (Config ["recurse"].AsBoolean) {
					searchMode = SearchOption.AllDirectories;
				}
			} catch (Exception) {
			}
			IEnumerable<string> filepathList = Directory.EnumerateFiles (Config ["sampleDir"].AsString, Config ["searchTerms"].AsString, searchMode);
			List<string> fileArr = filepathList.ToList<string> ();
			//fileArr.Sort();
			Console.WriteLine ("Found {0} Samples", fileArr.Count);
			int i = map.samples.Count;
			foreach (string file in fileArr) {
				BsonArray requires = filters ["requires"].AsBsonArray;
				BsonArray excludes = filters ["excludes"].AsBsonArray;
				if (filters ["caseInsensitive"].AsBoolean) {
					if (requires.Count > 0) {
						var results = from require in requires where file.ToLower ().Contains (require.AsString.ToLower ()) select file;
						if (results.FirstOrDefault () == null) {
							continue;
						}
					}
					if (excludes.Count > 0) {
						var results = from exclude in excludes where file.ToLower ().Contains (exclude.AsString.ToLower ()) select file;
						if (results.FirstOrDefault () != null) {
							continue;
						}
					}
				} else {
					if (requires.Count > 0) {
						var results = from require in requires where file.Contains (require.AsString) select file;
						if (results.FirstOrDefault () == null) {
							continue;
						}
					}
					if (excludes.Count > 0) {
						var results = from exclude in excludes where file.Contains (exclude.AsString) select file;
						if (results.FirstOrDefault () != null) {
							continue;
						}
					}
				}
	
				string filename = file.Substring (file.LastIndexOf ("/") + 1);
				string path = file.Substring (0, file.LastIndexOf ("/"));
					
				KeyValuePair<string, string> kvp = new KeyValuePair<string, string> (filename, path);
					
				fileList.Add (kvp);

			}
			long totalSize = 0;
			Console.WriteLine ("{0} Samples Matched Criteria", fileList.Count);
			//fileList.Sort((KeyValuePair<string, string> firstPair,KeyValuePair<string, string> nextPair) =>{	return (firstPair.Value+"/"+firstPair.Key).CompareTo(firstPair.Value+"/"+firstPair.Key); });
			if (tasks.Contains ("sort")) {
				fileList.Sort ((KeyValuePair<string, string> firstPair,KeyValuePair<string, string> nextPair) => {
					return firstPair.Key.CompareTo (nextPair.Key); });
			}
		
			foreach (KeyValuePair<string,string> kvp in fileList) {
				string path = kvp.Value;
				string filename = kvp.Key;
				Sample s = new Sample (path, filename) { id = i };
				long fileSize = new FileInfo(path+"/"+filename).Length;
				totalSize+=fileSize;
				long hrfs = (fileSize / 1024);
				map.samples.Add (s);
				Console.WriteLine ("Added {5}kb Sample {0} Key: {3}, Vel: {4}, {2} - {1}", i, path, filename,s.lKey,s.lVel, hrfs);
				i++;
			}
			return totalSize;
		}
		public static Platform CurrentPlatform 
		{  get {
				switch (Environment.OSVersion.Platform) {
				case PlatformID.Unix:
					// Well, there are chances MacOSX is reported as Unix instead of MacOSX.
					// Instead of platform check, we'll do a feature checks (Mac specific root folders)
					if (Directory.Exists ("/Applications")
					    & Directory.Exists ("/System")
					    & Directory.Exists ("/Users")
					    & Directory.Exists ("/Volumes"))
						return Platform.Mac;
					else
						return Platform.Unix;
					
				case PlatformID.MacOSX:
					return Platform.Mac;
					
				default:
					return Platform.Windows;
				}
			}
		}


    }
}
