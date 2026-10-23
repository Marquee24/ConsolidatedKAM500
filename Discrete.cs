using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParameterViewCSV_Generator
{
    public class Discrete
    {
        public static Dictionary<string, DiscreteICDEng> decodedValueDiscreteEng = new Dictionary<string, DiscreteICDEng>();

        public class Discrete_Recorded_DataEng
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
            public Discrete_Recorded_DataEng()
            {
            }
            public Discrete_Recorded_DataEng(string time, string channel, string rawdata)
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
            public Dictionary<string, DiscreteICDEng> decode()
            {
                decodedValueDiscreteEng = new Dictionary<string, DiscreteICDEng>();
                byte[] Raw_Data = new byte[] { Convert.ToByte(RawData.Substring(0, 2), 16), Convert.ToByte(RawData.Substring(2, 2), 16), Convert.ToByte(RawData.Substring(4, 2), 16), Convert.ToByte(RawData.Substring(6, 2), 16) };

                string tempChannel = Channel;
                if (Program.icdDict_DiscreteEng.ContainsKey(tempChannel))
                {
                    List<DiscreteICDEng> paramss = Program.icdDict_DiscreteEng[tempChannel];
                    foreach (DiscreteICDEng param in paramss)
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
                        decodedValueDiscreteEng[param.dictKey] = (param);
                    }
                }
                return decodedValueDiscreteEng;
            }

        }

        public static List<Discrete_Recorded_DataEng> readDiscreteRecordedDataEng()
        {
            List<Discrete_Recorded_DataEng> tempRecListDiscreteEng = new List<Discrete_Recorded_DataEng>();

            using (StreamReader reader = new StreamReader(SelectFileFromFolder.DiscreteSelectedFilePath))
            {
                Discrete_Recorded_DataEng recData = new Discrete_Recorded_DataEng();
                string line;
                string header = reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    string[] values = line.Trim().Split(',');
                    if (values.Length >= 5)
                    {
                        if (values[(int)DiscreteRawICDHeaders.Raw_Data].Contains("+"))
                        {
                            values[(int)DiscreteRawICDHeaders.Raw_Data] = values[(int)DiscreteRawICDHeaders.Raw_Data].Replace("+", "");
                            if (values[(int)DiscreteRawICDHeaders.Raw_Data].Contains("."))
                            {
                                values[(int)DiscreteRawICDHeaders.Raw_Data] = values[(int)DiscreteRawICDHeaders.Raw_Data].Replace(".", "");
                            }
                        }
                        tempRecListDiscreteEng.Add(new Discrete_Recorded_DataEng(values[(int)DiscreteRawICDHeaders.Time],
                            values[(int)DiscreteRawICDHeaders.Channel], values[(int)DiscreteRawICDHeaders.Raw_Data]));
                    }
                }
            }
            return tempRecListDiscreteEng;
        }

        public static Dictionary<string, List<DiscreteICDEng>> readDiscreteICDEng(string filePath)
        {
            Dictionary<string, List<DiscreteICDEng>> tempIcdDictArincEng = new Dictionary<string, List<DiscreteICDEng>>();
            string minVal;
            string lsb;
            foreach (var line in File.ReadLines(filePath))
            {
                if (!(line.StartsWith("#") || line.StartsWith("$")))
                {
                    string[] trimedLine = line.Trim().Split(',');
                    if ((trimedLine[(int)ParameterICDSchema.INTERFACE] == "DISCRETE_RX" || trimedLine[(int)ParameterICDSchema.INTERFACE] == "DISCRETE_TX" || trimedLine[(int)ParameterICDSchema.INTERFACE] == "!DISCRETE_RX") && trimedLine[(int)ParameterICDSchema.START_BIT] != "NA" && trimedLine[(int)ParameterICDSchema.WIDTH] != "NA")
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
                        var value = new DiscreteICDEng(Label, Convert.ToInt32(trimedLine[(int)ParameterICDSchema.SOURCE]), trimedLine[(int)ParameterICDSchema.PARAMETERS], Convert.ToInt32(trimedLine[(int)ParameterICDSchema.START_BIT]), Convert.ToInt32(trimedLine[(int)ParameterICDSchema.WIDTH]), trimedLine[(int)ParameterICDSchema.SSM_TYPE], trimedLine[(int)ParameterICDSchema.LSB], (minVal), (trimedLine[(int)ParameterICDSchema.MAXVAL]));
                        if (tempIcdDictArincEng.ContainsKey(value.source.ToString()))
                        {
                            tempIcdDictArincEng[value.source.ToString()].Add(value);
                        }
                        else
                        {
                            tempIcdDictArincEng[value.source.ToString()] = (new List<DiscreteICDEng> { new DiscreteICDEng(Label, Convert.ToInt32(trimedLine[(int)ParameterICDSchema.SOURCE]), trimedLine[(int)ParameterICDSchema.PARAMETERS], Convert.ToInt32(trimedLine[(int)ParameterICDSchema.START_BIT]), Convert.ToInt32(trimedLine[(int)ParameterICDSchema.WIDTH]), trimedLine[(int)ParameterICDSchema.SSM_TYPE], trimedLine[(int)ParameterICDSchema.LSB], (minVal), (trimedLine[(int)ParameterICDSchema.MAXVAL])) });
                        }
                    }
                }

            }
            return tempIcdDictArincEng;
        }
        public static List<DiscreteICDEng> readIcdFilterDiscreteEng(string filePath)
        {
            List<DiscreteICDEng> tempIcdListDiscreteEng = new List<DiscreteICDEng>();
            int i = 1;
            foreach (var line in File.ReadLines(filePath))
            {
                string lsb;
                string minVal;
                int interfaceID = -1;
                if (!(line.StartsWith("#") || line.StartsWith("$")))
                {
                    string[] trimedLine = line.Trim().Split(',');
                    if ((trimedLine[(int)ParameterICDSchema.INTERFACE] == "DISCRETE_RX" || trimedLine[(int)ParameterICDSchema.INTERFACE] == "DISCRETE_TX" || trimedLine[(int)ParameterICDSchema.INTERFACE] == "!DISCRETE_RX") && trimedLine[(int)ParameterICDSchema.START_BIT] != "NA" && trimedLine[(int)ParameterICDSchema.WIDTH] != "NA")
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

                        if (trimedLine[(int)ParameterICDSchema.INTERFACE] == "DISCRETE_RX" || trimedLine[(int)ParameterICDSchema.INTERFACE] == "!DISCRETE_RX")
                        {
                            interfaceID = 2;
                        }
                        else if (trimedLine[(int)ParameterICDSchema.INTERFACE] == "DISCRETE_TX" || trimedLine[(int)ParameterICDSchema.INTERFACE] == "!DISCRETE_TX")
                        {
                            interfaceID = 1;
                        }
                        tempIcdListDiscreteEng.Add(new DiscreteICDEng(i++, interfaceID, (trimedLine[(int)ParameterICDSchema.SID]), Convert.ToInt32(trimedLine[(int)ParameterICDSchema.SOURCE]), (trimedLine[(int)ParameterICDSchema.REG_SA_LABEL]), trimedLine[(int)ParameterICDSchema.PARAMETERS], Convert.ToInt32(trimedLine[(int)ParameterICDSchema.START_BIT]), Convert.ToInt32(trimedLine[(int)ParameterICDSchema.WIDTH]), trimedLine[(int)ParameterICDSchema.SSM_TYPE], trimedLine[(int)ParameterICDSchema.LSB], minVal));

                    }
                }
            }
            return tempIcdListDiscreteEng;
        }

    }
    public class DiscreteICDEng
    {
        public int Sl_No { get; set; }
        public string paramName { get; set; }
        public string Label { get; set; }

        public int InterfaceID { get; set; }
        public string SID { get; set; }
        public int source { get; set; }
        public string Time { get; set; }

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

        public DiscreteICDEng(string label, int source, string paramName)
        {
            this.Label = label;
            this.source = source;
            this.paramName = paramName.Trim();
            this.dictKey = this.paramName + "|" + this.source + "-" + this.Label + "-" + this.startBit + "-" + this.width;
        }

        public DiscreteICDEng(int slno, int interfaceID, string sid, int source, string label, string paramName, int startBit, int width)
        {
            this.Sl_No = slno;
            this.SID = sid;
            this.InterfaceID = interfaceID;
            this.source = source;
            this.Label = label;
            this.paramName = paramName.Trim();
            this.startBit = startBit;
            this.width = width;
            this.dictKey = this.paramName + "|" + this.source + "-" + this.Label + "-" + this.startBit + "-" + this.width;
        }
        public DiscreteICDEng()
        {

        }
        public DiscreteICDEng(string data, int source)
        {
            this.hexData = data;
            this.source = source;
            this.dictKey = this.paramName + "|" + this.source + "-" + this.Label + "-" + this.startBit + "-" + this.width;
        }
        public DiscreteICDEng(string label, int source, string paramName, int startBit, int width, string ssmType, string lsb, string minVal, string maxVal)
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
            this.dictKey = this.paramName + "|" + this.source + "-" + this.Label + "-" + this.startBit + "-" + this.width;
        }
        public DiscreteICDEng(int slno, int interfaceID, string sid, int source, string label, string paramName, int startBit, int width, string ssmType, string lsb, string minVal)
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
            this.dictKey = this.paramName + "|" + this.source + "-" + this.Label + "-" + this.startBit + "-" + this.width;
        }
    }
}
