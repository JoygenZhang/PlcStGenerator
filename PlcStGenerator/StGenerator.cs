using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlcStGenerator
{
    public class StGenerator
    {
        public static bool IsWorks3 { get; set; }
        public static bool BooleanAssignStyle { get; set; }

        private static StringBuilder _sb = new StringBuilder();

        public static StringBuilder BuildProgs(List<DeclareData> datas)
        {
            _sb.Clear();
            const string comma = ",";

            _sb.AppendLine();
            _sb.AppendLine("// ui command");

            var groups = datas
                .OrderBy(x => x.Group)
                .GroupBy(x => x.Group);

            foreach (var group in groups)
            {
                if (IsWorks3)
                    _sb.AppendLine("IF not " + group.Key + "Cycle THEN");
                else
                    _sb.AppendLine("IF " + group.Key + "Cycle = FALSE THEN");

                foreach (var item in group)
                {
                    var uiVar = "ui" + item.Group + item.Name;
                    var gVar = "g" + item.Group + item.Name;
                    _sb.AppendLine("    ALTP(" + uiVar + comma + " " + gVar + ");");

                }
                _sb.AppendLine("END_IF;");
                _sb.AppendLine();
            }

            foreach (var group in groups)
            {
                var uiVar = "ui" + group.Key + "Reset";
                _sb.AppendLine("IF " + uiVar + " THEN");
                foreach (var item in group)
                {
                    var gVar = "g" + item.Group + item.Name;
                    _sb.Append("    ").AppendLine(GenBooleanAssign(gVar, false));
                }

                _sb.AppendLine();
                _sb.AppendLine("    " + GenBooleanAssign(uiVar, false));
                _sb.AppendLine("END_IF;");
                _sb.AppendLine();
            }

            _sb.AppendLine();
            _sb.AppendLine("// error handling");
            foreach (var item in datas)
            {
                var cyVar = "cy" + item.Group + item.Name;
                if (item.Type != PlcVarType.FbCylinder2x1y)
                    continue;

                _sb.AppendLine("IF " + cyVar + ".oErr THEN");
                _sb.AppendLine("    SET(" + cyVar + ".oErrId = ErrCyOn, F50);");
                _sb.AppendLine("    SET(" + cyVar + ".oErrId = ErrCyOff, F50);");
                _sb.AppendLine("    SET(" + cyVar + ".oErrId = ErrCyBoth, F50);");
                _sb.AppendLine("END_IF;");

                _sb.AppendLine();
            }

            _sb.AppendLine();
            _sb.AppendLine("(* error messages");
            foreach (var item in datas)
            {
                switch (item.Type)
                {
                    case PlcVarType.FbCylinder1x1y:
                        {
                            var str = item.Group.ToUpper() + " \"" + item.Name + "\" on state abnormal.";
                            _sb.AppendLine(str);
                            break;
                        }
                    case PlcVarType.FbCylinder2x1y:
                        {
                            var str = item.Group.ToUpper() + " \"" + item.Name + "\" on state abnormal.";
                            var str2 = item.Group.ToUpper() + " \"" + item.Name + "\" off state abnormal.";
                            var str3 = item.Group.ToUpper() + " \"" + item.Name + "\" sensor state abnormal.";

                            _sb.AppendLine(str);
                            _sb.AppendLine(str2);
                            _sb.AppendLine(str3);
                            break;
                        }
                }
            }
            _sb.AppendLine("error messages *)");

            _sb.AppendLine();
            _sb.AppendLine("// cylinder function block control");

            foreach (var item in datas)
            {
                var gVar = "g" + item.Group + item.Name;

                var cyVar = "cy" + item.Group + item.Name;
                var yVar = "y" + item.Group + item.Name;
                var x1Var = "x" + item.Group + item.Name + "Org";
                var x2Var = "x" + item.Group + item.Name + "Act";
                var onVar = "w" + item.Group + item.Name + "OnT";
                var offVar = "w" + item.Group + item.Name + "OffT";
                var timeoutVar = "w" + item.Group + item.Name + "TO";

                _sb.AppendLine("IF gSys.ManMode THEN");
                _sb.AppendLine("    " + cyVar + ".iOnDelay := " + onVar + ";");
                _sb.AppendLine("    " + cyVar + ".iOffDelay := " + offVar + ";");

                switch (item.Type)
                {
                    case PlcVarType.FbCylinder0x1y:
                        _sb.AppendLine("END_IF;");
                        _sb.AppendLine();

                        _sb.AppendLine(cyVar + "(");
                        _sb.AppendLine("    iAct := " + gVar + comma);
                        if (IsWorks3)
                            _sb.AppendLine("    oDevice =>" + yVar + ");");
                        else
                            _sb.AppendLine("    oDevice := " + yVar + ");");

                        //_sb.AppendLine("    iOnDelay := " + onVar + comma);
                        //_sb.AppendLine("    iOffDelay := " + offVar + ");");
                        break;
                    case PlcVarType.FbCylinder1x1y:
                        _sb.AppendLine("    " + cyVar + ".iOnTO := " + timeoutVar + ";");
                        _sb.AppendLine("    " + cyVar + ".iOffTO := " + timeoutVar + ";");
                        _sb.AppendLine("END_IF;");
                        _sb.AppendLine();

                        _sb.AppendLine(cyVar + "(");
                        _sb.AppendLine("    iAct := " + gVar + comma);
                        _sb.AppendLine("    iRst := gResetAlarm,");
                        _sb.AppendLine("    iLimit1 := " + x1Var + comma);
                        if (IsWorks3)
                            _sb.AppendLine("    oDevice =>" + yVar + ");");
                        else
                            _sb.AppendLine("    oDevice := " + yVar + ");");

                        //_sb.AppendLine("    iOnTO := " + timeoutVar + comma);
                        //_sb.AppendLine("    iOffTO := " + timeoutVar + comma);
                        //_sb.AppendLine("    iOnDelay := " + onVar + comma);
                        //_sb.AppendLine("    iOffDelay := " + offVar + ");");
                        break;
                    case PlcVarType.FbCylinder2x1y:
                        _sb.AppendLine("    " + cyVar + ".iOnTO := " + timeoutVar + ";");
                        _sb.AppendLine("    " + cyVar + ".iOffTO := " + timeoutVar + ";");
                        _sb.AppendLine("END_IF;");
                        _sb.AppendLine();

                        _sb.AppendLine(cyVar + "(");
                        _sb.AppendLine("    iAct := " + gVar + comma);
                        _sb.AppendLine("    iRst := gResetAlarm,");
                        _sb.AppendLine("    iLimit1 := " + x1Var + comma);
                        _sb.AppendLine("    iLimit2 := " + x2Var + comma);

                        if (IsWorks3)
                            _sb.AppendLine("    oDevice =>" + yVar + ");");
                        else
                            _sb.AppendLine("    oDevice := " + yVar + ");");

                        //_sb.AppendLine("    iOnTO := " + timeoutVar + comma);
                        //_sb.AppendLine("    iOffTO := " + timeoutVar + comma);
                        //_sb.AppendLine("    iOnDelay := " + onVar + comma);
                        //_sb.AppendLine("    iOffDelay := " + offVar + ");");
                        break;
                }

                _sb.AppendLine();
            }

            return _sb;
        }

        public static StringBuilder BuildDeclares(List<DeclareData> datas)
        {
            _sb.Clear();

            var uiDeclares = new List<DeclareData>();
            var xOrgDeclares = new List<DeclareData>();
            var xActDeclares = new List<DeclareData>();
            var yDeclares = new List<DeclareData>();
            var cyDeclares = new List<DeclareData>();
            var onDeclares = new List<DeclareData>();
            var offDeclares = new List<DeclareData>();
            var timeOutDeclares = new List<DeclareData>();
            var controlDeclares = new List<DeclareData>();

            foreach (var data in datas)
            {
                controlDeclares.Add(GenControlDeclare(data));

                switch (data.Type)
                {
                    case PlcVarType.FbCylinder0x1y:
                        uiDeclares.Add(GenUiDeclare(data));
                        yDeclares.Add(GenYDeclare(data));
                        cyDeclares.Add(GenCyDeclare(data));
                        onDeclares.Add(GenOnDeclare(data));
                        offDeclares.Add(GenOffDeclare(data));
                        break;

                    case PlcVarType.FbCylinder1x1y:
                        uiDeclares.Add(GenUiDeclare(data));
                        yDeclares.Add(GenYDeclare(data));
                        cyDeclares.Add(GenCyDeclare(data));

                        onDeclares.Add(GenOnDeclare(data));
                        offDeclares.Add(GenOffDeclare(data));
                        timeOutDeclares.Add(GenTimeOutDeclare(data));

                        xOrgDeclares.Add(GenXOrgDeclare(data));
                        break;

                    case PlcVarType.FbCylinder2x1y:
                        uiDeclares.Add(GenUiDeclare(data));
                        yDeclares.Add(GenYDeclare(data));
                        cyDeclares.Add(GenCyDeclare(data));

                        onDeclares.Add(GenOnDeclare(data));
                        offDeclares.Add(GenOffDeclare(data));
                        timeOutDeclares.Add(GenTimeOutDeclare(data));

                        xOrgDeclares.Add(GenXOrgDeclare(data));
                        xActDeclares.Add(GenXActDeclare(data));
                        break;
                }
            }

            foreach (var item in controlDeclares)
            {
                _sb.AppendLine(GenDeclareString(item));
            }

            foreach (var item in uiDeclares)
            {
                _sb.AppendLine(GenDeclareString(item));
            }

            foreach (var item in yDeclares)
            {
                _sb.AppendLine(GenDeclareString(item));
            }

            foreach (var item in cyDeclares)
            {
                _sb.AppendLine(GenDeclareString(item));
            }

            foreach (var item in xOrgDeclares)
            {
                _sb.AppendLine(GenDeclareString(item));
            }

            foreach (var item in xActDeclares)
            {
                _sb.AppendLine(GenDeclareString(item));
            }

            foreach (var item in onDeclares)
            {
                _sb.AppendLine(GenDeclareString(item));
            }

            foreach (var item in offDeclares)
            {
                _sb.AppendLine(GenDeclareString(item));
            }

            foreach (var item in timeOutDeclares)
            {
                _sb.AppendLine(GenDeclareString(item));
            }

            return _sb;
        }

        private static string GenDeclareString(DeclareData item)
        {
            const string tabChar = "\t";
            const string varGlobal = "VAR_GLOBAL";

            string result = string.Empty;

            if (IsWorks3)
            {
                result =
                    item.Prefix +
                    item.Group +
                    item.Name +
                    item.Postfix +
                    tabChar +
                    //item.Type +
                    GenRealDataType(item.Type) +
                    tabChar +
                    varGlobal;
            }
            else
            {
                result =
                    varGlobal +
                    tabChar +
                    item.Prefix +
                    item.Group +
                    item.Name +
                    item.Postfix +
                    tabChar +
                    //item.Type;
                    GenRealDataType(item.Type)
                    ;
            }
            return result;
        }

        private static string GenRealDataType(PlcVarType src)
        {
            var result = string.Empty;

            if (IsWorks3)
            {
                result = src.ToString();
            }
            else
            {
                switch (src)
                {
                    case PlcVarType.Word:
                        result = "Word[Signed]";
                        break;
                    //case PlcVarType.Dword:
                    //    break;
                    //case PlcVarType.Float:
                    //    break;
                    default:
                        result = src.ToString();
                        break;
                }

            }
            return result;
        }

        public static StringBuilder BuildGroupDeclares(List<string> datas)
        {
            _sb.Clear();

            DeclareData tmp = null;

            // uiSt1Cycle
            foreach (var item in datas)
            {
                tmp = new DeclareData { Group = item, Prefix = "ui", Postfix = "Cycle" };
                _sb.AppendLine(GenGroupDeclareString(tmp));
            }

            // St1Cycle
            foreach (var item in datas)
            {
                tmp = new DeclareData { Group = item, Postfix = "Cycle" };
                _sb.AppendLine(GenGroupDeclareString(tmp));
            }

            // autoSt1Cycle
            foreach (var item in datas)
            {
                tmp = new DeclareData { Group = item, Prefix = "auto", Postfix = "Cycle" };
                _sb.AppendLine(GenGroupDeclareString(tmp));
            }

            // wSt1Cycle
            foreach (var item in datas)
            {
                tmp = new DeclareData { Group = item, Prefix = "w", Postfix = "Index", Type = PlcVarType.Word };
                _sb.AppendLine(GenGroupDeclareString(tmp));
            }

            // uiSt1Reset
            foreach (var item in datas)
            {
                tmp = new DeclareData { Group = item, Prefix = "ui", Postfix = "Reset" };
                _sb.AppendLine(GenGroupDeclareString(tmp));
            }

            // St1InOrg
            foreach (var item in datas)
            {
                tmp = new DeclareData { Group = item, Postfix = "InOrg" };
                _sb.AppendLine(GenGroupDeclareString(tmp));
            }

            // St1CanGoNext
            foreach (var item in datas)
            {
                tmp = new DeclareData { Group = item, Postfix = "CanGoNext" };
                _sb.AppendLine(GenGroupDeclareString(tmp));
            }

            // St1GoNext
            foreach (var item in datas)
            {
                tmp = new DeclareData { Group = item, Postfix = "GoNext" };
                _sb.AppendLine(GenGroupDeclareString(tmp));
            }

            // St1GoNg
            foreach (var item in datas)
            {
                tmp = new DeclareData { Group = item, Postfix = "GoNg" };
                _sb.AppendLine(GenGroupDeclareString(tmp));
            }
            return _sb;
        }

        private static string GenGroupDeclareString(DeclareData item)
        {
            const string tabChar = "\t";
            const string varGlobal = "VAR_GLOBAL";

            return
                    item.Prefix +
                    item.Group +
                    item.Postfix +
                    tabChar +
                    item.Type +
                    tabChar +
                    varGlobal;
        }

        private static string GenBooleanAssign(string source, bool type)
        {
            if (!BooleanAssignStyle)
            {
                return source + " := " + type.ToString().ToUpper() + ";";
            }
            else
            {
                if (type == false)
                {
                    return "RST(TRUE, " + source + ");";
                }
                else
                {
                    return "SET(TRUE, " + source + ");";
                }
            }
        }

        public static StringBuilder BuildGroupProgs(List<DeclareData> datas)
        {
            var groups =
                from item in datas
                group item by item.Group into newGroup
                select new { value = newGroup.Key };

            // all groups
            var srcGroups = groups.Select(item => item.value).ToList();

            _sb.Clear();
            _sb.AppendLine();

            foreach (var item in srcGroups)
            {
                _sb.AppendLine();
                _sb.AppendLine("IF auto" + item + "Cycle THEN");

                _sb.AppendLine("    " + GenBooleanAssign(item + "Cycle", true));
                _sb.AppendLine("    w" + item + "Index := START_STEP;");

                _sb.AppendLine("    " + GenBooleanAssign("auto" + item + "Cycle", false));
                _sb.AppendLine("END_IF;");
            }

            foreach (var item in srcGroups)
            {
                _sb.AppendLine();

                // all cylinder in 1 group
                var cys =
                    from x in datas
                    where
                        (x.Type == PlcVarType.FbCylinder0x1y ||
                        x.Type == PlcVarType.FbCylinder1x1y ||
                        x.Type == PlcVarType.FbCylinder2x1y) && (x.Group == item)
                    select x.Name;

                _sb.AppendLine(item + "InOrg := ");
                foreach (var name in cys)
                {
                    _sb.AppendLine("    cy" + item + name + ".oOff AND");
                }
                if (IsWorks3)
                    _sb.AppendLine("    SM8000;");
                else
                    _sb.AppendLine("    M8000;");

                _sb.AppendLine();
                _sb.AppendLine(item + "CanGoNext := ");
                _sb.AppendLine("    NOT gSys.HasError AND");
                foreach (var name in cys)
                {
                    _sb.AppendLine("    NOT cy" + item + name + ".oErr AND");
                }

                if (IsWorks3)
                    _sb.AppendLine("    SM8000;");
                else
                    _sb.AppendLine("    M8000;");
            }

            _sb.AppendLine();
            foreach (var item in srcGroups)
            {
                _sb.AppendLine();
                _sb.AppendLine("IF " + item + "Cycle THEN");
                _sb.AppendLine("    IF " + item + "GoNext AND " + item + "CanGoNext THEN");
                _sb.AppendLine("        w" + item + "Index := w" + item + "Index + 10;");
                _sb.AppendLine("        " + GenBooleanAssign(item + "GoNext", false));
                _sb.AppendLine("    END_IF;");
                _sb.AppendLine();
                _sb.AppendLine("    IF " + item + "GoNg THEN");
                _sb.AppendLine("        w" + item + "LastIndex := w" + item + "Index;");
                _sb.AppendLine("        w" + item + "Index := NG_STEP;");
                _sb.AppendLine("        " + GenBooleanAssign(item + "GoNg", false));
                _sb.AppendLine("    END_IF;");
                _sb.AppendLine("END_IF;");
            }
            _sb.AppendLine();

            return _sb;
        }

        public static StringBuilder BuildFlowProcess(List<ActionList> actList)
        {
            _sb.Clear();
            const string tabChar = "\t";

            if (actList.Count < 1)
            {
                return _sb;
            }

            var group = actList[0].Group;

            _sb.AppendLine();
            _sb.AppendLine("IF " + group + "Cycle THEN");
            _sb.AppendLine("    IF w" + group + "Index = OK_STEP THEN");
            _sb.AppendLine("        " + GenBooleanAssign(group + "Cycle", false));
            _sb.AppendLine("        w" + group + "Index := 0;");
            _sb.AppendLine();
            _sb.AppendLine("    ELSIF w" + group + "Index = NG_STEP THEN");
            _sb.AppendLine("        IF " + group + "CanGoNext THEN");
            _sb.AppendLine("            w" + group + "Index := w" + group + "LastIndex;");
            _sb.AppendLine("            w" + group + "LastIndex := 0;");
            _sb.AppendLine("        END_IF;");
            _sb.AppendLine("    END_IF;");
            _sb.AppendLine();

            var idx = 10;
            _sb.AppendLine("    CASE w" + group + "Index OF");
            foreach (var item in actList)
            {
                _sb.AppendLine("    " + idx.ToString() + ":");
                if (string.Equals(item.Act, "ON", StringComparison.OrdinalIgnoreCase))
                {
                    _sb.Append(tabChar);
                    _sb.AppendLine("    " + GenBooleanAssign("g" + group + item.Name, true));

                    _sb.Append(tabChar);
                    _sb.AppendLine("    IF cy" + group + item.Name + ".oOn THEN");
                    _sb.Append(tabChar);
                    _sb.AppendLine("        " + GenBooleanAssign(group + "GoNext", true));

                    _sb.Append(tabChar);
                    _sb.AppendLine("    ELSIF cy" + group + item.Name + ".oErr THEN");
                    _sb.Append(tabChar);
                    _sb.AppendLine("        " + GenBooleanAssign(group + "GoNg", true));
                    _sb.Append(tabChar);
                    _sb.AppendLine("    END_IF;");
                }
                else if (string.Equals(item.Act, "OFF", StringComparison.OrdinalIgnoreCase))
                {
                    _sb.Append(tabChar);
                    _sb.AppendLine("    " + GenBooleanAssign("g" + group + item.Name, false));
                    _sb.Append(tabChar);
                    _sb.AppendLine("    IF cy" + group + item.Name + ".oOff THEN");
                    _sb.Append(tabChar);
                    _sb.AppendLine("        " + GenBooleanAssign(group + "GoNext", true));
                    _sb.Append(tabChar);
                    _sb.AppendLine("    ELSIF cy" + group + item.Name + ".oErr THEN");
                    _sb.Append(tabChar);
                    _sb.AppendLine("        " + GenBooleanAssign(group + "GoNg", true));
                    _sb.Append(tabChar);
                    _sb.AppendLine("    END_IF;");
                }

                idx = idx + 10;
            }

            _sb.AppendLine("    END_CASE;");
            _sb.AppendLine("END_IF;");
            return _sb;
        }

        private static DeclareData GenXOrgDeclare(DeclareData data)
        {
            var tmp = new DeclareData
            {
                Prefix = "x",
                Type = PlcVarType.Bit,
                Name = data.Name,
                Group = data.Group,
                Postfix = "Org"
            };
            return tmp;
        }

        private static DeclareData GenXActDeclare(DeclareData data)
        {
            var tmp = new DeclareData
            {
                Prefix = "x",
                Type = PlcVarType.Bit,
                Name = data.Name,
                Group = data.Group,
                Postfix = "Act"
            };
            return tmp;
        }

        private static DeclareData GenTimeOutDeclare(DeclareData data)
        {
            var tmp = new DeclareData
            {
                Prefix = "w",
                Type = PlcVarType.Word,
                Name = data.Name,
                Group = data.Group,
                Postfix = "TO"
            };
            return tmp;
        }

        private static DeclareData GenOffDeclare(DeclareData data)
        {
            var tmp = new DeclareData
            {
                Prefix = "w",
                Type = PlcVarType.Word,
                Name = data.Name,
                Group = data.Group,
                Postfix = "OffT"
            };
            return tmp;
        }

        private static DeclareData GenOnDeclare(DeclareData data)
        {
            var tmp = new DeclareData
            {
                Prefix = "w",
                Type = PlcVarType.Word,
                Name = data.Name,
                Group = data.Group,
                Postfix = "OnT"
            };
            return tmp;
        }

        private static DeclareData GenCyDeclare(DeclareData data)
        {
            var tmp = new DeclareData { Prefix = "cy", Type = data.Type };
            tmp.Name = data.Name;
            tmp.Group = data.Group;
            return tmp;
        }

        private static DeclareData GenYDeclare(DeclareData data)
        {
            var tmp = new DeclareData { Prefix = "y", Type = PlcVarType.Bit };
            tmp.Name = data.Name;
            tmp.Group = data.Group;
            return tmp;
        }

        private static DeclareData GenUiDeclare(DeclareData data)
        {
            var tmp = new DeclareData { Prefix = "ui", Type = PlcVarType.Bit };
            tmp.Name = data.Name;
            tmp.Group = data.Group;
            return tmp;
        }

        private static DeclareData GenControlDeclare(DeclareData data)
        {
            var tmp = new DeclareData { Prefix = "g", Type = PlcVarType.Bit };
            tmp.Name = data.Name;
            tmp.Group = data.Group;
            return tmp;
        }


    }
}
