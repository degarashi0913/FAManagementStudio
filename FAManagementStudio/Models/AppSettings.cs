using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;

namespace FAManagementStudio.Models
{
    public static class AppSettingsManager
    {
        private static AppSettings _settings;
        static AppSettingsManager()
        {
            StartTime = DateTime.Now;
            _settings = new AppSettings();

            if (!_settings.IsUpgrade)
            {
                _settings.Upgrade();
                _settings.IsUpgrade = true;
            }

            PreviousActivation = _settings.PreviousActivation ?? DateTime.Now;
            _settings.PreviousActivation = StartTime;
            Save();
        }

        public static DateTime StartTime { get; set; }
        public static DateTime PreviousActivation { get; set; }

        public static string Version
        {
            get { return _settings.Version; }
            set
            {
                _settings.Version = value;
                _settings.Save();
            }
        }
        public static List<string> QueryProjectBasePaths
        {
            get
            {
                if (_settings.QueryProjectBasePaths == null) _settings.QueryProjectBasePaths = new List<string>();
                return _settings.QueryProjectBasePaths;
            }
        }
        public static void Save()
        {
            _settings.Save();
        }
    }

    internal sealed class AppSettings : ApplicationSettingsBase
    {
        [UserScopedSetting]
        public DateTime? PreviousActivation
        {
            get { return (DateTime?)this[nameof(PreviousActivation)]; }
            set { this[nameof(PreviousActivation)] = value; }
        }
        [UserScopedSetting]
        public string Version
        {
            get { return (string)this[nameof(Version)]; }
            set { this[nameof(Version)] = value; }
        }
        [UserScopedSetting]
        public List<string> QueryProjectBasePaths
        {
            get { return (List<string>)this[nameof(QueryProjectBasePaths)]; }
            set { this[nameof(QueryProjectBasePaths)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("false")]
        public bool IsUpgrade
        {
            get { return (bool)this[nameof(IsUpgrade)]; }
            set { this[nameof(IsUpgrade)] = value; }
        }
    }
}
