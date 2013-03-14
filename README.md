ReaktorSampleMapper
===================

Reaktor Sample Map Generator, based on http://relivethefuture.com/choronzon/software/reaktor-sample-map-generator/

Written in C#, Compatible with mono/.NET

Allows a theoretical maximum of up to 16384 per sample map.

Can read/write (non embedded) reaktor sample maps, aswell as convert to/from JSON. 

mono ./ReaktorMapGenerator.exe


	Example config.json 
	{
		"filename" : "/Users/ash/Music/example.map",
		"tasks" : ["add", "sort", "json", "map"],
		"append" : true,
		"sampleDir" : "/Users/ash/Music/Samples/",
		"recurse" : true,
		"searchTerms" : "*.wav",
		"filters" : {
			"caseInsensitive" : true,
			"excludes" : ["fill", "combo", "_", "rock", "nokick", "session", "add on", "no kick", "kick loops", "fx", "stick", "toms"],
			"requires" : ["Drum"]
		}
	}

Filename - Can be either map or json file.

Tasks
-----

add:

This will add all wav files found in to match the criteria set with the searchterms and the filters object.


sort:

This will alphanumerically sort the order of the files in the searched directory


json:

output map as json file


map:

output reaktor map


Append - true/false, if filename exists and is a valid reaktor map/json file, append new samples found to the end of the map 

sampleDir - Directory to search for samples

recurse - true/false, Recurse through subdirectories

searchTerms - filename search terms

Filters
-------

caseInsensitive - true/false, filter case sensitivity

excludes - filenames that match this do not get added to the map

requires - filenames that match this get added to the map
