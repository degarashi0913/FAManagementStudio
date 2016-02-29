using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly string _dataPath;
        public PathHistoryRepository()
        {
            _dataPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "his");
            DataLoad(_dataPath, History);
        }

        private void DataLoad(string path, ObservableCollection<string> history)
        {
            if (!File.Exists(path)) return;

            using (var stream = new FileStream(path, FileMode.Open))
            {
                var ser = new DataContractSerializer(typeof(PathHistory));
                var loadedHistory = (PathHistory)ser.ReadObject(stream);
                for (int i = 0; i < loadedHistory.Path.Length; i++)
                {
                    history.Add(loadedHistory.Path[i]);
                }
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
            History.Add(newPath);
        }

        public void SaveData()
        {
            InnnerSaveData(_dataPath, History);
        }
        private void InnnerSaveData(string path, ObservableCollection<string> history)
        {
            var data = new PathHistory();
            data.Path = history.ToArray();

            using (var stream = new FileStream(path, FileMode.OpenOrCreate))
            {
                var ser = new DataContractSerializer(typeof(PathHistory));
                ser.WriteObject(stream, data);
            }
        }
    }
}
