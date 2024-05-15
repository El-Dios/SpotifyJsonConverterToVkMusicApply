using SpotifyPlaylistJsonParserToVkMusicFormat;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using static SpotifyPlaylistJsonParserToVkMusicFormat.PlaylistVk;


JsonSerializerOptions s_writeOptions = new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
};

JsonSerializerOptions s_readOptions = new()
{
    AllowTrailingCommas = true,
    PropertyNameCaseInsensitive = true
};

string Serialize<T>(T value)
{
    return JsonSerializer.Serialize(value, s_writeOptions);
}

T Deserialize<T>(string json)
{
    return JsonSerializer.Deserialize<T>(json, s_readOptions)!;
}

string fileName = @"Choose your filepath to SpotifyPlaylist.json";

List<SpotifyPlaylist> playlists = DeserializeSptfyJson(fileName);

while (true)
{
    Console.WriteLine("Please select next option (type 1 or 2):\n" +
    "1.Write all playlists from Spotify to 1 VkPlaylistFull.json file;\n" +
    "2.Write all playlists from Spotify to several separated json files;");

    string? userInput = Console.ReadLine();

    if (int.TryParse(userInput, out int userDecision))
    {
        switch (userDecision)
        {
            case 1:
                string writeFileName = $"VkPlaylistFull.json";
                string jsonStringWrite = SerializeSpfyListToVkPlObj(playlists);
                WritePlaylistToFile(writeFileName, jsonStringWrite);
                break;
            case 2:
                WriteSeparateVkCompPlaylists(playlists);
                break;
        }
        break;
    }
    else { Console.Clear(); Console.WriteLine("Incorrect input! Try again."); }
}

List<SpotifyPlaylist> DeserializeSptfyJson(string _fileName)
{
    string jsonString = File.ReadAllText(_fileName);

    Dictionary<string, JsonElement> dict = Deserialize<Dictionary<string, JsonElement>>(jsonString);

    return Deserialize<List<SpotifyPlaylist>>(dict["playlists"].ToString());
}

void WritePlaylistToFile(string _fileName, string _jsonStringToWrite)
{
    using StreamWriter outputFile = new(_fileName);
    outputFile.WriteLine(_jsonStringToWrite);
}

string SerializeSpfyListToVkPlObj(List<SpotifyPlaylist> _playlists)
{
    PlaylistVk playlistVk = new()
    {
        Tracks = []
    };

    foreach (var playlist in _playlists)
    {
        try
        {
            playlistVk.Tracks.AddRange(playlist.Items!.Select(itm => new VkTrack
            {
                Artist = itm.Track?.ArtistName,
                Album = itm.Track?.AlbumName,
                Track = itm.Track?.TrackName,
                Uri = itm.Track?.TrackUri
            }).ToList());
        }
        catch (Exception e) 
        {
            Console.WriteLine($"Processing Playlist \"{playlist.Name}\" failed: {e.Message}");
        }

        Console.WriteLine($"Playlist \"{playlist.Name}\" is DONE");
        Console.WriteLine(new string('-', 10));
    }
    return Serialize(playlistVk);
}

void WriteSeparateVkCompPlaylists(List<SpotifyPlaylist> _playlists)
{
    PlaylistVk playlistVk = new();

    foreach (var playlist in _playlists)
    {
        try
        {
            playlistVk.Tracks = (playlist.Items!.Select(itm => new VkTrack
            {
                Artist = itm.Track?.ArtistName,
                Album = itm.Track?.AlbumName,
                Track = itm.Track?.TrackName,
                Uri = itm.Track?.TrackUri
            }).ToList());
        }
        catch (Exception e)
        {
            Console.WriteLine($"Processing Playlist \"{playlist.Name}\" failed: {e.Message}");
        }

        Console.WriteLine(playlist.Items.Length);
        Regex reg = new("[/:*?\"\\\\<>|]");
        string writeFileName = $"VkComp {reg.Replace(playlist.Name, string.Empty)}.json";
        string jsonStringWrite = Serialize(playlistVk);

        WritePlaylistToFile(writeFileName, jsonStringWrite);

        Console.WriteLine($"Playlist \"{playlist.Name}\" is WRITTEN as \"{writeFileName}\"");
        Console.WriteLine(new string('-', 10));
    }

    
}

namespace SpotifyPlaylistJsonParserToVkMusicFormat
{
    class SpotifyPlaylist
    {
        public string? Name { get; set; }
        public DateTime LastModifiedDate { get; set; }
        internal class PlayListItem
        {
            internal class SpotifyTrack
            {
                public string? TrackName { get; set; }
                public string? ArtistName { get; set; }
                public string? AlbumName { get; set; }
                public string? TrackUri { get; set; }
            }
            public SpotifyTrack? Track { get; set; }
            public object? Episode { get; set; }
            public object? LocalTrack { get; set; }
            public DateTime? AddedDate { get; set; }
        }
        public PlayListItem[]? Items { get; set; }
        public string? Description { get; set; }
        public int? NumberOfFollowers { get; set; }
    }

    class PlaylistVk
    {
        internal class VkTrack
        {
            public string? Artist { get; set; }
            public string? Album { get; set; }
            public string? Track { get; set; }
            public string? Uri { get; set; }
        }
        public List<VkTrack>? Tracks { get; set; }
    }
}