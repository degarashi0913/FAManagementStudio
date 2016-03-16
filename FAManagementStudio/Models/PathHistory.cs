using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace FAManagementStudio.Models
{
    [DataContract]
    class PathHistory
    {
        [DataMember]
        public string[] Path { get; set; }
    }

    public class PathHistoryRepository
    {
        public ObservableCollection<string> History { get; set; } = new ObservableCollection<string>();
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
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    var ser = new DataContractSerializer(typeof(PathHistory));
                    var loadedHistory = (PathHistory)ser.ReadObject(stream);
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
            if (History.Contains(newPath))
            {
                History.Remove(newPath);
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
            var data = new PathHistory();
            data.Path = History.ToArray();

            using (var stream = new FileStream(path, FileMode.OpenOrCreate))
            {
                var ser = new DataContractSerializer(typeof(PathHistory));
                ser.WriteObject(stream, data);
            }
        }
    }
}
