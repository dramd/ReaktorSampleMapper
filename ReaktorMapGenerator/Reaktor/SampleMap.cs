using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MongoDB.Bson;
namespace ReaktorMapGenerator.Reaktor
{
   partial class SampleMap
    {
        public string path;
		public string mapPath;
        public string file;
        public List<Sample> samples;
        Stream _mapStream;
		BinaryReader _reader;
        BinaryWriter _mapWriter;
        public SampleMap(string path, bool append = true)
        {
			this.path = path.Substring (0,path.LastIndexOf ("/")+1);
            this.file = path.Substring (path.LastIndexOf ("/")+1);
            this.samples = new List<Sample>();
			FileMode fMode = FileMode.OpenOrCreate;
			if (File.Exists (path) && !append) {
				 fMode = FileMode.Truncate;
			}

			mapPath = Path.ChangeExtension (path, "map");
			_mapStream = new FileStream(mapPath,fMode);
			if (append) {
				Deserialize(path);
			} 
            _mapWriter = new BinaryWriter(_mapStream); 
        }

		public BsonDocument ToBson() {
			BsonArray sampleArray = new BsonArray ();
			foreach (Sample sample in samples) {
				sampleArray.Add(sample.ToBson ());
			}
			return new BsonDocument{ {"path",this.path },{"fileName",this.file }, { "samples", sampleArray } };
		}
		public void Deserialize(string path) {
			try {
				switch (Path.GetExtension(path)) {
				default:
				case "map":
					DeserializeR5Map (path);
					break;
				case "json":
					DeserializeJSON (path);
					break;
				}
			}catch (Exception e) {
				Console.WriteLine (e);
			}
		}
		public void DeserializeJSON(string path) {
			BsonDocument bs = BsonDocument.Parse (File.ReadAllText (path));
			this.path = bs ["path"].AsString;
			this.file = bs ["fileName"].AsString;
			foreach (BsonDocument sampleInfo in bs["samples"].AsBsonArray) {
				samples.Add (new Sample(sampleInfo));
			}
		}
		public void Serialize()
		{
			_mapWriter.WriteHex("00000000");
			_mapWriter.WriteHex("AB010000");
			_mapWriter.WriteString("NIMapFile");
			_mapWriter.Write(path.Length);
			_mapWriter.WriteString(path);
			_mapWriter.Write(file.Length);
			_mapWriter.WriteString(file);
			_mapWriter.WriteHex("070000006D6170700C00000001000000010000000000000000000000");
			_mapWriter.Write(samples.Count);
			foreach (Sample sample in samples)
			{
				sample.Serialize(_mapWriter);
			}
			_mapWriter.Flush();
			_mapWriter.Close();
		}

		public void DeserializeR5Map(string path) {
			Console.WriteLine("Parsing NI Map File {0}",path);
			_reader = new BinaryReader (_mapStream);
			_mapStream.Position =0;
			_reader.ReadBytes(8);
			char[] ident = _reader.ReadChars(9);
			string NIMapFile = new string(ident);
			if (NIMapFile != "NIMapFile") {
				Console.WriteLine ("Error: Does not appear to be a valid Reaktor Map File. Aborting.");
				return;
			}
			int pathLen = _reader.ReadInt32 ();
			string fPath =  System.Text.ASCIIEncoding.ASCII.GetString(_reader.ReadBytes(pathLen));
			int fileLen = _reader.ReadInt32 ();
			string fName = System.Text.ASCIIEncoding.ASCII.GetString(_reader.ReadBytes(fileLen));
			_reader.ReadBytes(_reader.HexToBytes("070000006D6170700C00000001000000010000000000000000000000").Length);
			int sampleCount = _reader.ReadInt32();
			this.path = fPath;
			this.file = fName;

			Console.WriteLine ("{0} Samples in map {1}", sampleCount ,this,path,this.file);

			for (int i = 0; i < sampleCount; i++) {
				Sample s = new Sample (_reader);
				samples.Add (s);
			}
			Console.WriteLine ("Read {0}Kb Sample Map",(_mapStream.Position/1024));
			_mapStream.Position = 0;
		}

    }
}
