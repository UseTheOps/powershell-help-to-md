using System;
using System.Collections.Generic;
using System.Text;

namespace PowershellHelpToMd
{
    public class CmdletData
    {
        public CmdletData()
        {
            ParameterSets = new List<CmdletParameterSetData>();
        }
        public string Name { get; set; }
        public string Description { get; set; }

        public List<CmdletParameterSetData> ParameterSets { get; set; }
    }

    public class CmdletParameterSetData
    {
        public CmdletParameterSetData()
        {
            Parameters = new List<CmdletParameterData>();
        }
        public string Name { get; set; }
        public List<CmdletParameterData> Parameters { get; set; }
    }

    public class CmdletParameterData
    {
        public string Name { get; set; }
        public string Description { get; set; }

    }
}
