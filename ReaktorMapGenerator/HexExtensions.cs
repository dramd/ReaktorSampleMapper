using System;
using System.IO;
using MongoDB.Bson;
namespace ReaktorMapGenerator
{
	public static class HexExtensions
	{
		public static string ToPrettyJson(this BsonValue value) {
			return value.ToJson(new MongoDB.Bson.IO.JsonWriterSettings() { Indent = true, IndentChars = "\t", NewLineChars = Environment.NewLine } );
		}
		public static void WriteHex(this BinaryWriter writer, string hex)
		{
			writer.Write(writer.HexToBytes(hex));
		}
		public static void WriteString(this BinaryWriter writer, string input)
		{
			writer.Write(System.Text.ASCIIEncoding.ASCII.GetBytes(input));
		}
		public static byte[] HexToBytes(this BinaryWriter writer, string hex)
		{
			if (hex.Length % 2 == 1)
				throw new Exception("The binary key cannot have an odd number of digits");
			
			byte[] arr = new byte[hex.Length >> 1];
			
			for (int i = 0; i < hex.Length >> 1; ++i)
			{
				arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
			}
			
			return arr;
		}
		public static byte[] HexToBytes(this BinaryReader writer, string hex)
		{
			if (hex.Length % 2 == 1)
				throw new Exception("The binary key cannot have an odd number of digits");
			
			byte[] arr = new byte[hex.Length >> 1];
			
			for (int i = 0; i < hex.Length >> 1; ++i)
			{
				arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
			}
			
			return arr;
		}
		
		public static int GetHexVal(char hex)
		{
			int val = (int)hex;
			//For uppercase A-F letters:
			return val - (val < 58 ? 48 : 55);
			//For lowercase a-f letters:
			//return val - (val < 58 ? 48 : 87);
			//Or the two combined, but a bit slower:
			//return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
		}
	}
}

