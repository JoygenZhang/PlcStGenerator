namespace PlcStGenerator
{
    public enum PlcVarType
    {
        Bit,
        Word,
        Dword,
        Float,
        String,
        Time,
        Timer,

        FbAxisAbs,
        FbAxisZero,

        FbCylinder0x1y,
        FbCylinder1x1y,
        FbCylinder2x1y,

        FbRotateTable,
        FbPartsFeeder,

        //FbCylinder0x1y,
        //FbCylinder1x1y,     // sensor at Origin point
        //FbCylinder1x1y_1,   // sensor at Act point
        //FbCylinder2x1y,
        //FbCylinder2x2y
    }
}