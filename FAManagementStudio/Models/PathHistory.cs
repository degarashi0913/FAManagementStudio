using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;

namespace FAManagementStudio.Models;

[DataContract]
class PathHistory
{
    [DataMember]
    public required string[] Path { get; set; }
}

public class PathHistoryRepository
{
    public ObservableCollection<string> History { get; set; } = [];
    private const string _fileName = "his";
    public PathHistoryRepository()
    {
    }

    public void LoadData(string entryPath)
    {
        var path = Path.Combine(entryPath, _fileName);
        if (!File.Exists(path)) return;
        try
        {
            using var stream = new FileStream(path, FileMode.Open);
            var ser = new DataContractSerializer(typeof(PathHistory));

            if (ser.ReadObject(stream) is PathHistory loadedHistory)
            {
                for (int i = 0; i < loadedHistory.Path.Length; i++)
                {
                    History.Add(loadedHistory.Path[i]);
                }
            }
        }
        //失敗したらそのまま
        catch
        {
        }
    }

    public void DataAdd(string newPath)
    {
        int index = History.IndexOf(newPath);

        if (0 <= index)
        {
            if (0 == index) return;
            History.RemoveAt(index);
        }

        if (10 <= History.Count)
        {
            History.RemoveAt(History.Count - 1);
        }
        History.Insert(0, newPath);
    }

    public void SaveData(string entryPath)
    {
        var path = Path.Combine(entryPath, _fileName);
        var data = new PathHistory
        {
            Path = [.. History]
        };

        using var stream = new FileStream(path, FileMode.OpenOrCreate);
        var ser = new DataContractSerializer(typeof(PathHistory));
        ser.WriteObject(stream, data);
    }
}
