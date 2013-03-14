using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MongoDB.Bson;
namespace ReaktorMapGenerator.Reaktor
{
    class Sample
    {

		public string file { get; private set; }
		public string path { get; private set; }
		int _id;
		public int id {
			get {return _id; }
			set { _id = value; ConvertID (value); }
		}
        public int lKey=0,rKey=127,lVel=0,rVel=127,Root=0,Looping=0,loopStart=0,loopStop=0;
        public float tune=0, gain=0, pan=0;
        public Sample(string path, string file)
        {
            this.file= file;
            this.path = FixOSXPath(path);
        }

		public Sample(BinaryReader reader) {
			int pathLen = reader.ReadInt32 ();
			this.path = System.Text.ASCIIEncoding.ASCII.GetString(reader.ReadBytes (pathLen));
			int fileLen = reader.ReadInt32 ();
			this.file = System.Text.ASCIIEncoding.ASCII.GetString(reader.ReadBytes (fileLen));
			reader.ReadBytes(reader.HexToBytes("0400000000000000656E74725400000002000000").Length);
			lKey = reader.ReadInt32 ();
			rKey = reader.ReadInt32 ();
			lVel = reader.ReadInt32 ();
			rVel = reader.ReadInt32 ();
			Root = reader.ReadInt32 ();
			reader.ReadBytes(reader.HexToBytes("00000000").Length);
			tune = reader.ReadSingle ();
			gain = reader.ReadSingle ();
			pan = reader.ReadSingle ();
			reader.ReadBytes(reader.HexToBytes("FFFFFFFFFFFFFFFF00000000").Length);
			Looping = reader.ReadInt32 ();
			reader.ReadBytes(reader.HexToBytes("0000000001000000").Length);
			loopStart = reader.ReadInt32 ();
			loopStop = reader.ReadInt32 ();
			reader.ReadBytes(reader.HexToBytes("000000005500000006426200").Length);
			id = (lKey * 127) + lVel;
			Console.WriteLine ("Deserialised Sample {0}, ID: {1}, Key: {2}, Vel: {3}", file, id, lKey,lVel);

		}
		public Sample(BsonDocument sampleData)
		{
			file= sampleData["fileName"].AsString;
			path = FixOSXPath(sampleData["path"].AsString);
			_id = sampleData["id"].AsInt32;
			lKey = sampleData ["lKey"].AsInt32;
			rKey = sampleData ["rKey"].AsInt32;
			lVel = sampleData ["lVel"].AsInt32;
			rVel = sampleData ["rVel"].AsInt32;
			Root = sampleData ["root"].AsInt32;
			Looping = sampleData ["looping"].AsInt32;
			loopStart = sampleData ["loopStart"].AsInt32;
			loopStop = sampleData ["loopStop"].AsInt32;
			tune = (float)sampleData ["tune"].AsDouble;
			gain =  (float)sampleData ["gain"].AsDouble;
			pan =  (float)sampleData ["pan"].AsDouble;
		}
		public BsonDocument ToBson() {
			return new BsonDocument { 
				{"id",id},{"fileName",file},{"path",path},
				{"lKey",lKey}, {"rKey",rKey}, {"lVel",lVel}, {"rVel",rVel}, {"root",Root},
				{"looping",Looping},{"loopStart",loopStart}, {"loopStop",loopStop}, 
				{"tune",tune}, {"gain",gain}, {"pan",pan}
			};
		}

		public void Serialize(BinaryWriter writer)
        {
            writer.Write(path.Length);
            writer.WriteString(path);
            writer.Write(file.Length);
            writer.WriteString(file);
            writer.WriteHex("0400000000000000656E74725400000002000000");
            writer.Write(lKey);
            writer.Write(rKey);
            writer.Write(lVel);
            writer.Write(rVel);
            writer.Write(Root);
            writer.WriteHex("00000000");
            writer.Write(tune);
            writer.Write(gain);
            writer.Write(pan);
            writer.WriteHex("FFFFFFFF");
            writer.WriteHex("FFFFFFFF");
            writer.WriteHex("00000000");
            writer.Write(Looping);
            writer.WriteHex("00000000");
            writer.WriteHex("01000000");
            writer.Write(loopStart);
            writer.Write(loopStop);
            writer.WriteHex("00000000");
            writer.WriteHex("55000000");
            writer.WriteHex("06426200");
        }

		private string FixOSXPath(string path) {
			if (Program.CurrentPlatform == Platform.Mac) {
				if (path.StartsWith ("/Volumes")) {
					path = path.Substring (8);
				}
				path = path.Replace ('/', ':');
				if (path [0] == ':') {
					path = path.Substring (1);
				}
			}
			return path;
		}
		private void ConvertID(int id) {
			
			int velocity = id % 127;
			int key = id / 127;
			lKey = key;
			rKey = key;
			lVel = velocity;
			rVel = velocity;
		}
    }
}
