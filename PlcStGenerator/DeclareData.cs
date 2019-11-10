namespace PlcStGenerator
{
    public class DeclareData
    {
        public string Name { get; set; }
        public string Group { get; set; }

        public PlcVarType Type { get; set; }
        public string Prefix { get; set; }
        public string Postfix { get; set; }

        public DeclareData()
        {
            Name = string.Empty;
            Group = string.Empty;
            Type = PlcVarType.Bit;
            Prefix = string.Empty;
            Postfix = string.Empty;
        }
    };
}
