namespace PlcStGenerator
{
    public class ActionList
    {
        public string Name { get; set; }
        public string Act { get; set; }
        public string Group { get; set; }

        public ActionList()
        {
            Name = string.Empty;
            Act = string.Empty;
            Group = string.Empty;
        }
    }
}