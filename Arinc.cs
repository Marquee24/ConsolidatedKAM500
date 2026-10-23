using System;
using System.Collections.Generic;
using System.IO;

namespace ParameterViewCSV_Generator
{

    public class Arinc
    {

        public static Dictionary<string, ArincICDEng> decodedValueArincEng = new Dictionary<string, ArincICDEng>();

        public static int SSM_StartBit = 30;
        public static int SSM_Width = 2;

        public class Arinc_Recorded_DataEng
        {
            public string Time { get; set; }
            public string Label { get; set; }
            public string RawData { get; set; }
            public double Time_double { get; set; }
            public bool hasEvent { get; set; }
            public string EventText { get; set; }
            public string Channel { get; set; }
            public string ID { get; set; }
            public double TimeStringToDoubleInternal()
            {
                return Program.TimeStringToDouble(this.Time);
            }
            public Arinc_Recorded_DataEng()
            {
            }
            public Arinc_Recorded_DataEng(string time, string channel, string label, string rawdata)
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

                Label = (Convert.ToString(Raw_Data[3], 8));
                ID = this.Channel + "|" + this.Label;
                this.decode();
            }

            public Dictionary<string, ArincICDEng> decode()
            {
                decodedValueArincEng = new Dictionary<string,ArincICDEng>();
                byte[] Raw_Data = new byte[] { Convert.ToByte(RawData.Substring(0, 2), 16), Convert.ToByte(RawData.Substring(2, 2), 16), Convert.ToByte(RawData.Substring(4, 2), 16), Convert.ToByte(RawData.Substring(6, 2), 16) };

                if (Program.icdDict_ArincEng.ContainsKey(ID))
                {
                    List<ArincICDEng> paramss = Program.icdDict_ArincEng[ID];
                    foreach (ArincICDEng param in paramss)
                    {
                        string t1 = Convert.ToString(Raw_Data[0], 2).PadLeft(8, '0');
                        string t2 = Convert.ToString(Raw_Data[1], 2).PadLeft(8, '0');
                        string t3 = Convert.ToString(Raw_Data[2], 2).PadLeft(8, '0');
                        string t4 = Convert.ToString(Raw_Data[3], 2).PadLeft(8, '0');
                        string bitData = t1 + t2 + t3 + t4;
                        string temp = "";
                        string ssmBitData = "";
                        for (int i = 0; i < param.width; i++)
                        {
                            temp += bitData[bitData.Length - (param.startBit + (param.width - i - 1))];//21 
                        }
                        for (int i = 0; i < SSM_Width; i++)
                        {
                            ssmBitData += bitData[bitData.Length - (SSM_StartBit + (SSM_Width - i - 1))];//21 
                        }
                        param.Label = Label;
                        param.hexData =  Convert.ToInt32(temp, 2).ToString("X");
                        param.decData = Convert.ToInt32(temp, 2).ToString();
                        param.ssm_Value = Convert.ToInt32(ssmBitData, 2).ToString();
                        decodedValueArincEng[param.dictKey] = (param);
                    }
                }
                return decodedValueArincEng;
            }
        }


        public static List<Arinc_Recorded_DataEng> readArincRecordedDataEng()
        {
            List<Arinc_Recorded_DataEng> tempRecListArincEng = new List<Arinc_Recorded_DataEng>();

            using (StreamReader reader = new StreamReader(SelectFileFromFolder.ArincSelectedFilePath))
            {
                Arinc_Recorded_DataEng recData = new Arinc_Recorded_DataEng();
                string line;
                string header = reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    string[] values = line.Trim().Split(',');
                    if (values.Length >= 11)
                    {
                        string Label = values[(int)ArincRawICDHeaders.Label];
                        if (values[(int)ArincRawICDHeaders.Raw_Data].Contains("+"))
                        {
                            values[(int)ArincRawICDHeaders.Raw_Data] = values[(int)ArincRawICDHeaders.Raw_Data].Replace("+", "");
                            if (values[(int)ArincRawICDHeaders.Raw_Data].Contains("."))
                            {
                                values[(int)ArincRawICDHeaders.Raw_Data] = values[(int)ArincRawICDHeaders.Raw_Data].Replace(".", "");
                            }
                        }
                        tempRecListArincEng.Add(new Arinc_Recorded_DataEng(values[(int)ArincRawICDHeaders.Time], values[(int)ArincRawICDHeaders.Channel], Label, values[(int)ArincRawICDHeaders.Raw_Data]));
                    }
                }
            }
            return tempRecListArincEng;
        }
        public static Dictionary<string, List<ArincICDEng>> readArincICDEng(string filePath)
        {
            Dictionary<string, List<ArincICDEng>> tempIcdDictArincEng = new Dictionary<string, List<ArincICDEng>>();
            string minVal;
            string lsb;
            foreach (var line in File.ReadLines(filePath))
            {
                if (!(line.StartsWith("#") || line.StartsWith("$")))
                {
                    string[] trimedLine = line.Trim().Split(',');
                    if ((trimedLine[(int)ParameterICDSchema.INTERFACE] == "ARINC_RX" || trimedLine[(int)ParameterICDSchema.INTERFACE] == "ARINC_TX") && trimedLine[(int)ParameterICDSchema.START_BIT] != "NA" && trimedLine[(int)ParameterICDSchema.WIDTH] != "NA" || trimedLine[(int)ParameterICDSchema.INTERFACE] == "!ARINC_RX")
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
                        var value = new ArincICDEng(Label, Convert.ToInt32(trimedLine[(int)ParameterICDSchema.SOURCE]), trimedLine[(int)ParameterICDSchema.PARAMETERS], Convert.ToInt32(trimedLine[(int)ParameterICDSchema.START_BIT]), Convert.ToInt32(trimedLine[(int)ParameterICDSchema.WIDTH]), trimedLine[(int)ParameterICDSchema.SSM_TYPE], trimedLine[(int)ParameterICDSchema.LSB], (minVal), (trimedLine[(int)ParameterICDSchema.MAXVAL]));
                        if (tempIcdDictArincEng.ContainsKey((value.source + "|" + value.Label)))
                        {
                            tempIcdDictArincEng[(value.source + "|" + value.Label)].Add(value);
                        }
                        else
                        {
                            tempIcdDictArincEng[(value.source + "|" + value.Label)] = (new List<ArincICDEng> { new ArincICDEng(Label, Convert.ToInt32(trimedLine[(int)ParameterICDSchema.SOURCE]), trimedLine[(int)ParameterICDSchema.PARAMETERS], Convert.ToInt32(trimedLine[(int)ParameterICDSchema.START_BIT]), Convert.ToInt32(trimedLine[(int)ParameterICDSchema.WIDTH]), trimedLine[(int)ParameterICDSchema.SSM_TYPE], trimedLine[(int)ParameterICDSchema.LSB], (minVal), (trimedLine[(int)ParameterICDSchema.MAXVAL])) });
                        }
                    }
                }

            }
            return tempIcdDictArincEng;
        }
        public static List<ArincICDEng> readIcdFilterArincEng(string filePath)
        {
            List<ArincICDEng> tempIcdListArincEng = new List<ArincICDEng>();
            int i = 1;
            foreach (var line in File.ReadLines(filePath))
            {
                string lsb;
                string minVal;
                int interfaceID = -1;
                if (!(line.StartsWith("#") || line.StartsWith("$")))
                {
                    string[] trimedLine = line.Trim().Split(',');
                    if ((trimedLine[(int)ParameterICDSchema.INTERFACE] == "ARINC_RX" || trimedLine[(int)ParameterICDSchema.INTERFACE] == "ARINC_TX") && trimedLine[(int)ParameterICDSchema.START_BIT] != "NA" && trimedLine[(int)ParameterICDSchema.WIDTH] != "NA")
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
                        if (trimedLine[(int)ParameterICDSchema.INTERFACE] == "ARINC_RX" || trimedLine[(int)ParameterICDSchema.INTERFACE] == "!ARINC_RX")
                        {
                            interfaceID = 5;
                        }
                        else if (trimedLine[(int)ParameterICDSchema.INTERFACE] == "ARINC_TX" || trimedLine[(int)ParameterICDSchema.INTERFACE] == "!ARINC_TX")
                        {
                            interfaceID = 6;
                        }

                        tempIcdListArincEng.Add(new ArincICDEng(i++, interfaceID, (trimedLine[(int)ParameterICDSchema.SID]), Convert.ToInt32(trimedLine[(int)ParameterICDSchema.SOURCE]), (trimedLine[(int)ParameterICDSchema.REG_SA_LABEL]), trimedLine[(int)ParameterICDSchema.PARAMETERS], Convert.ToInt32(trimedLine[(int)ParameterICDSchema.START_BIT]), Convert.ToInt32(trimedLine[(int)ParameterICDSchema.WIDTH])));

                    }
                }
            }
            return tempIcdListArincEng;
        }
    }
    public class ArincICDEng
    {
        public int Sl_No { get; set; }
        public string paramName { get; set; }
        public string Label { get; set; }
        public string SID { get; set; }
        public string A1_Size_arinc = "86";
        public int InterfaceID { get; set; }
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

        public ArincICDEng(int slno, int interfaceID, string sid, int source, string label, string paramName, int startBit, int width)
        {
            this.Sl_No = slno;
            this.SID = sid;
            this.InterfaceID = interfaceID;
            this.source = source;
            this.Label = label;
            this.paramName = paramName;
            this.startBit = startBit;
            this.width = width;
            this.dictKey = this.paramName + "|" + this.source + "-" + this.Label + "-" + this.startBit + "-" + this.width;
        }
        public ArincICDEng()
        {

        }
        public ArincICDEng(string label, int source, string paramName, int startBit, int width, string ssmType, string lsb, string minVal, string maxVal)
        {
            this.Label = label;
            this.source = source;
            this.paramName = paramName;
            this.startBit = startBit;
            this.width = width;
            this.ssm_Type = ssmType;
            this.ssm_Value = ssm_Value;
            this.LSB = lsb;
            this.minVal = minVal;
            this.maxVal = maxVal;
            this.dictKey = this.paramName + "|" + this.source + "-" + this.Label + "-" + this.startBit + "-" + this.width;
        }
    }
}
