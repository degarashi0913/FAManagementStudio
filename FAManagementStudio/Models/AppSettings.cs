using System;
using System.Collections.Generic;
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
            PreviousActivation = _settings.PreviousActivation ?? DateTime.Now;
            _settings.PreviousActivation = StartTime;
            _settings.Save();
        }

        public static DateTime StartTime { get; set; }
        public static DateTime PreviousActivation { get; set; }
    }

    internal sealed class AppSettings : ApplicationSettingsBase
    {
        [UserScopedSetting]
        public DateTime? PreviousActivation
        {
            get { return (DateTime?)this[nameof(PreviousActivation)]; }
            set { this[nameof(PreviousActivation)] = value; }
        }
    }
}
