namespace FAManagementStudio.Models
{
    public class SequenceInfo
    {
        public SequenceInfo(string name, long currentValue)
        {
            Name = name;
            CurrentValue = currentValue;
        }
        public string Name { get; set; }
        public long CurrentValue { get; set; }
    }
}
