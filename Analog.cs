using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParameterViewCSV_Generator
{
    public class Analog
    {
        public static Dictionary<string, AnalogICDEng> decodedValueAnalogEng = new Dictionary<string, AnalogICDEng>();

        public class Analog_Recorded_DataEng
        {
            public string Time { get; set; }
            public string RawData { get; set; }
            public double Time_double { get; set; }
            public bool hasEvent { get; set; }
            public string EventText { get; set; }
            public string Channel { get; set; }
            public double TimeStringToDoubleInternal()
            {
                return Program.TimeStringToDouble(this.Time);
            }
            public Analog_Recorded_DataEng()
            {
            }
            public Analog_Recorded_DataEng(string time, string channel, string rawdata)
            {
                this.Time = (time.Trim());
                this.RawData = rawdata.Trim();
                this.Time_double = TimeStringToDoubleInternal();
                this.Channel = channel.Trim();
                if (RawData.Length < 8)
                {
                    RawData = RawData.PadLeft(8, '0');
                }
                byte[] Raw_Data = new byte[] { Convert.ToByte(RawData.Substring(0, 2), 16), Convert.ToByte(RawData.Substring(2, 2), 16), Convert.ToByte(RawData.Substring(4, 2), 16), Convert.ToByte(RawData.Substring(6, 2), 16) };
              
                this.decode();
            }
            public Dictionary<string, AnalogICDEng> decode()
            {
                decodedValueAnalogEng = new Dictionary<string, AnalogICDEng>() {};
                byte[] Raw_Data = new byte[] { Convert.ToByte(RawData.Substring(0, 2), 16), Convert.ToByte(RawData.Substring(2, 2), 16), Convert.ToByte(RawData.Substring(4, 2), 16), Convert.ToByte(RawData.Substring(6, 2), 16) };

                string tempChannel = Channel;
                if (Program.icdDict_AnalogEng.ContainsKey(tempChannel))
                {
                    List<AnalogICDEng> paramss = Program.icdDict_AnalogEng[tempChannel];
                    foreach (AnalogICDEng param in paramss)
                    {
                        string t1 = Convert.ToString(Raw_Data[0], 2).PadLeft(8, '0');
                        string t2 = Convert.ToString(Raw_Data[1], 2).PadLeft(8, '0');
                        string t3 = Convert.ToString(Raw_Data[2], 2).PadLeft(8, '0');
                        string t4 = Convert.ToString(Raw_Data[3], 2).PadLeft(8, '0');
                        string bitData = t1 + t2 + t3 + t4;
                        string temp = "";
                        for (int i = 0; i < param.width; i++)
                        {
                            temp += bitData[bitData.Length - (param.startBit + (param.width - i - 1))];//21 
                        }
                        param.channel = Convert.ToInt32(tempChannel);
                        param.hexData = Convert.ToInt32(temp, 2).ToString("X");
                        param.decData = Convert.ToInt32(temp, 2).ToString();
                        string displayBitData = "";
                        for (int i = 0; i < 32; i++)
                        {
                            if ((i < bitData.Length - param.startBit + param.width) && (i >= bitData.Length - param.startBit))
                            {
                                displayBitData += bitData[i];
                            }
                            else
                            {
                                displayBitData += " ";
                            }
                        }
                        decodedValueAnalogEng[param.dictKey] = (param);
                    }
                }
                return decodedValueAnalogEng;
            }

        }

        public static List<Analog_Recorded_DataEng> readAnalogRecordedDataEng()
        {
            List<Analog_Recorded_DataEng> tempRecListAnalogEng = new List<Analog_Recorded_DataEng>();

            using (StreamReader reader = new StreamReader(SelectFileFromFolder.AnalogSelectedFilePath))
            {
                Analog_Recorded_DataEng recData = new Analog_Recorded_DataEng();
                string line;
                string header = reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    string[] values = line.Trim().Split(',');
                    if (values.Length >= 5)
                    {
                        if (values[(int)AnalogRawICDHeaders.Raw_Data].Contains("+"))
                        {
                            values[(int)AnalogRawICDHeaders.Raw_Data] = values[(int)AnalogRawICDHeaders.Raw_Data].Replace("+", "");
                            if (values[(int)AnalogRawICDHeaders.Raw_Data].Contains("."))
                            {
                                values[(int)AnalogRawICDHeaders.Raw_Data] = values[(int)AnalogRawICDHeaders.Raw_Data].Replace(".", "");
                            }
                        }
                        tempRecListAnalogEng.Add(new Analog_Recorded_DataEng(values[(int)AnalogRawICDHeaders.Time],
                            values[(int)AnalogRawICDHeaders.Channel], values[(int)AnalogRawICDHeaders.Raw_Data]));
                    }
                }
            }
            return tempRecListAnalogEng;
        }
        public static List<AnalogICDEng> readIcdFilterAnalogEng(string filePath)
        {
            List<AnalogICDEng> tempIcdListAnalogEng = new List<AnalogICDEng>();
            int i = 1;
            foreach (var line in File.ReadLines(filePath))
            {
                string lsb;
                string minVal;
                int interfaceID = -1;
                if (!(line.StartsWith("#") || line.StartsWith("$")))
                {
                    string[] trimedLine = line.Trim().Split(',');
                    if ((trimedLine[(int)ParameterICDSchema.INTERFACE] == "AINPUT" || trimedLine[(int)ParameterICDSchema.INTERFACE] == "!AINPUT" || trimedLine[(int)ParameterICDSchema.INTERFACE] == "AOUTPUT" || trimedLine[(int)ParameterICDSchema.INTERFACE] == "!AOUTPUT") && trimedLine[(int)ParameterICDSchema.START_BIT] != "NA" && trimedLine[(int)ParameterICDSchema.WIDTH] != "NA")
                    {
                        if ((trimedLine[(int)ParameterICDSchema.MSB]) == "NA")
                        {
                            lsb = "1";
                        }
                        else
                        {

                            lsb = Convert.ToString(Math.Round((Convert.ToDouble(trimedLine[(int)ParameterICDSchema.MSB]) / (Math.Pow(2, (Convert.ToInt32(trimedLine[(int)ParameterICDSchema.WIDTH]) - 1)))), 2));
                        }
                        if ((trimedLine[(int)ParameterICDSchema.MINVAL]) == "NA")
                        {
                            minVal = "0";
                        }
                        else
                        {

                            minVal = trimedLine[(int)ParameterICDSchema.MINVAL];
                        }

                        if (trimedLine[(int)ParameterICDSchema.INTERFACE] == "AINPUT" || trimedLine[(int)ParameterICDSchema.INTERFACE] == "!AINPUT")
                        {
                            interfaceID = 3;
                        }
                        else if (trimedLine[(int)ParameterICDSchema.INTERFACE] == "AOUTPUT" || trimedLine[(int)ParameterICDSchema.INTERFACE] == "!AOUTPUT")
                        {
                            interfaceID = 4;
                        }
                        tempIcdListAnalogEng.Add(new AnalogICDEng(i++, interfaceID, (trimedLine[(int)ParameterICDSchema.SID]), Convert.ToInt32(trimedLine[(int)ParameterICDSchema.SOURCE]), (trimedLine[(int)ParameterICDSchema.REG_SA_LABEL]), trimedLine[(int)ParameterICDSchema.PARAMETERS], Convert.ToInt32(trimedLine[(int)ParameterICDSchema.START_BIT]), Convert.ToInt32(trimedLine[(int)ParameterICDSchema.WIDTH]), trimedLine[(int)ParameterICDSchema.SSM_TYPE], trimedLine[(int)ParameterICDSchema.LSB], minVal, (trimedLine[(int)ParameterICDSchema.Vmin]), (trimedLine[(int)ParameterICDSchema.Vmax])));

                    }
                }
            }
            return tempIcdListAnalogEng;
        }

        public static Dictionary<string, List<AnalogICDEng>> readAnalogICDEng(string filePath)
        {
            Dictionary<string, List<AnalogICDEng>> tempIcdDictArincEng = new Dictionary<string, List<AnalogICDEng>>();
            string minVal;
            string lsb;
            foreach (var line in File.ReadLines(filePath))
            {
                if (!(line.StartsWith("#") || line.StartsWith("$")))
                {
                    string[] trimedLine = line.Trim().Split(',');
                    if ((trimedLine[(int)ParameterICDSchema.INTERFACE] == "AINPUT" || trimedLine[(int)ParameterICDSchema.INTERFACE] == "!AINPUT" || trimedLine[(int)ParameterICDSchema.INTERFACE] == "AOUTPUT" || trimedLine[(int)ParameterICDSchema.INTERFACE] == "!AOUTPUT") && trimedLine[(int)ParameterICDSchema.START_BIT] != "NA" && trimedLine[(int)ParameterICDSchema.WIDTH] != "NA")
                    {
                        //trimedLine = trimedLine;
                        //string Label = Convert.ToInt32(trimedLine[4]);
                        string Label = trimedLine[(int)ParameterICDSchema.REG_SA_LABEL];
                        if ((trimedLine[(int)ParameterICDSchema.MSB]) == "NA")
                        {
                            lsb = "1";
                        }
                        else
                        {
                            lsb = Convert.ToString(Math.Round((Convert.ToDouble(trimedLine[(int)ParameterICDSchema.MSB]) / (Math.Pow(2, (Convert.ToInt32(trimedLine[(int)ParameterICDSchema.WIDTH]) - 1)))), 2));

                            //lsb = Convert.ToString((Convert.ToInt32(trimedLine[(int)ICDHeaders.MSB]) / (Math.Pow(2, (Convert.ToInt32(trimedLine[(int)ICDHeaders.WIDTH]) - 1)))));
                        }
                        if ((trimedLine[(int)ParameterICDSchema.MINVAL]) == "NA")
                        {
                            minVal = "0";
                        }
                        else
                        {
                            minVal = trimedLine[(int)ParameterICDSchema.MINVAL];
                        }

                        var value = new AnalogICDEng(Label, Convert.ToInt32(trimedLine[(int)ParameterICDSchema.SOURCE]), trimedLine[(int)ParameterICDSchema.PARAMETERS], Convert.ToInt32(trimedLine[(int)ParameterICDSchema.START_BIT]), Convert.ToInt32(trimedLine[(int)ParameterICDSchema.WIDTH]), trimedLine[(int)ParameterICDSchema.SSM_TYPE], trimedLine[(int)ParameterICDSchema.LSB], (minVal), (trimedLine[(int)ParameterICDSchema.MAXVAL]), (trimedLine[(int)ParameterICDSchema.Vmin]), (trimedLine[(int)ParameterICDSchema.Vmax]));
                        if (tempIcdDictArincEng.ContainsKey(value.source.ToString()))
                        {
                            tempIcdDictArincEng[value.source.ToString()].Add(value);
                        }
                        else
                        {
                            tempIcdDictArincEng[value.source.ToString()] = (new List<AnalogICDEng> { new AnalogICDEng(Label, Convert.ToInt32(trimedLine[(int)ParameterICDSchema.SOURCE]), trimedLine[(int)ParameterICDSchema.PARAMETERS], Convert.ToInt32(trimedLine[(int)ParameterICDSchema.START_BIT]), Convert.ToInt32(trimedLine[(int)ParameterICDSchema.WIDTH]), trimedLine[(int)ParameterICDSchema.SSM_TYPE], trimedLine[(int)ParameterICDSchema.LSB], (minVal), (trimedLine[(int)ParameterICDSchema.MAXVAL]), (trimedLine[(int)ParameterICDSchema.Vmin]), (trimedLine[(int)ParameterICDSchema.Vmax])) });
                        }
                    }
                }

            }
            return tempIcdDictArincEng;
        }

    }
    public class AnalogICDEng
    {
        public int Sl_No { get; set; }
        public string paramName { get; set; }
        public string Label { get; set; }

        public int InterfaceID { get; set; }
        public string SID { get; set; }
        public int source { get; set; }
        public string Time { get; set; }
        public string VMin { get; set; }
        public string VMax { get; set; }

        public int startBit { get; set; }
        public int width { get; set; }
        public string hexData { get; set; }
        public string decData { get; set; }
        public string minVal { get; set; }
        public string maxVal { get; set; }
        public string ssm_Type { get; set; }
        public string ssm_Value { get; set; }
        public string LSB { get; set; }
        public string dictKey { get; set; }
        public int channel { get; set; }

        public AnalogICDEng(string label, int source, string paramName, string vmin, string vmax)
        {
            this.Label = label;
            this.source = source;
            this.paramName = paramName.Trim();
            this.VMin = vmin;
            this.VMax = vmax;
            this.dictKey = this.paramName + "|" + this.source + "-" + this.Label + "-" + this.startBit + "-" + this.width;
        }

        public AnalogICDEng(int slno, int interfaceID, string sid, int source, string label, string paramName, int startBit, int width, string vmin, string vmax)
        {
            this.Sl_No = slno;
            this.SID = sid;
            this.InterfaceID = interfaceID;
            this.source = source;
            this.Label = label;
            this.paramName = paramName.Trim();
            this.startBit = startBit;
            this.width = width;
            this.VMin = vmin;
            this.VMax = vmax;
            this.dictKey = this.paramName + "|" + this.source + "-" + this.Label + "-" + this.startBit + "-" + this.width;
        }
        public AnalogICDEng()
        {

        }
        public AnalogICDEng(string data, int source, string vmin, string vmax)
        {
            this.hexData = data;
            this.source = source;
            this.VMin = vmin;
            this.VMax = vmax;
            this.dictKey = this.paramName + "|" + this.source + "-" + this.Label + "-" + this.startBit + "-" + this.width;
        }
        public AnalogICDEng(string label, int source, string paramName, int startBit, int width, string ssmType, string lsb, string minVal, string maxVal, string VMin, string VMax)
        {
            this.Label = label;
            this.source = source;
            this.paramName = paramName.Trim();
            this.startBit = startBit;
            this.width = width;
            this.ssm_Type = ssmType;
            this.ssm_Value = ssm_Value;
            this.LSB = lsb;
            this.minVal = minVal;
            this.maxVal = maxVal;
            this.VMin = VMin;
            this.VMax = VMax;
            this.dictKey = this.paramName + "|" + this.source + "-" + this.Label + "-" + this.startBit + "-" + this.width;
        }
        public AnalogICDEng(int slno, int interfaceID, string sid, int source, string label, string paramName, int startBit, int width, string ssmType, string lsb, string minVal, string vmin, string vmax)
        {
            this.Sl_No = slno;
            this.SID = sid;
            this.InterfaceID = interfaceID;
            this.source = source;
            this.Label = label;
            this.paramName = paramName.Trim();
            this.startBit = startBit;
            this.width = width;
            this.ssm_Type = ssmType;
            this.ssm_Value = ssm_Value;
            this.LSB = lsb;
            this.minVal = minVal;
            this.VMin = vmin;
            this.VMax = vmax;
            this.dictKey = this.paramName + "|" + this.source + "-" + this.Label + "-" + this.startBit + "-" + this.width;
        }
    }
}
