using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ParameterViewCSV_Generator
{
    enum ParameterICDSchema { INTERFACE = 0, SID, SOURCE, DESTINATION, REG_SA_LABEL, PARAM, PARAMETERS, START_BIT, WIDTH, SSM_TYPE, LSB, MINVAL, MAXVAL, MSB, DEFVAL, UNIT, KGU_CARDNUM, KGU_CHNUM, UUT_CARDNUM, UUT_CHNUM, Vmin, Vmax, OFFSET_MEM, REFERENCES, DATATYPE, DESIGNATION, SNTs, RPKM, Refresh_Rate, DESCRIPTION }
    enum ArincRawICDHeaders { SlNo = 0, Packet_Count, Time, Channel, Label, SSM, Parity, Data, Raw_Data, TimestampHi, TimeStampLo }
    enum DiscreteRawICDHeaders { SlNo = 0, Time, Channel, Raw_Data, TimestampHi, TimeStampLo }
    enum AnalogRawICDHeaders { SlNo = 0, Time, Channel, Raw_Data, TimestampHi, TimeStampLo }

    public class Program
    {
        public static string groundRun = "_Ground_Run";
        public static string icdKnownFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ICD\\LSS_ICD_KAM500"+ groundRun + ".csv");
        public static string parameterDataFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\..\\Data\\03_ConsolidatedData\\");
        public static string time = DateTime.Now.ToString("yyyy-mm-dd_HH-mm-ss");
        public static string ParameterDataPath = "";
        public static string ParameterDataPathEng = "";
        public static string ParameterDataPathHexa = "";
        public static string SessionId = "LSS-23 October 2026-17-21-54";

        public static List<Arinc.Arinc_Recorded_DataEng> recList_ArincEng = new List<Arinc.Arinc_Recorded_DataEng>();
        public static Dictionary<string, List<ArincICDEng>> icdDict_ArincEng = new Dictionary<string, List<ArincICDEng>>();
        public static List<ArincICDEng> icdList_ArincEng = new List<ArincICDEng>();

        public static List<Discrete.Discrete_Recorded_DataEng> recList_DiscreteEng = new List<Discrete.Discrete_Recorded_DataEng>();
        public static Dictionary<string, List<DiscreteICDEng>> icdDict_DiscreteEng = new Dictionary<string, List<DiscreteICDEng>>();
        public static List<DiscreteICDEng> icdList_DiscreteEng = new List<DiscreteICDEng>();

        public static List<Analog.Analog_Recorded_DataEng> recList_AnalogEng = new List<Analog.Analog_Recorded_DataEng>();
        public static Dictionary<string, List<AnalogICDEng>> icdDict_AnalogEng = new Dictionary<string, List<AnalogICDEng>>();
        public static List<AnalogICDEng> icdList_AnalogEng = new List<AnalogICDEng>();

        public static List<string> CombinedParamList = new List<string>();
        public static List<string> CombinedInterfaceEngParametrs = new List<string>();

        //public static List<Dictionary<string, string>> rowData_Interface = new List<Dictionary<string, string>>();
        //public static List<Dictionary<string, string>> rowData_InterfaceRaw = new List<Dictionary<string, string>>();
        //public static List<Dictionary<string, string>> rowData_InterfaceHexa = new List<Dictionary<string, string>>();
        public static List<Dictionary<string, string>> rowData_InterfaceEng = new List<Dictionary<string, string>>();
        public static List<ICDEng> interfaceSelectedParametrs = new List<ICDEng>();

        public static Dictionary<string, string> previousValuesCombinedInterface = new Dictionary<string, string>();
        public static Dictionary<string, string> previousValuesCombinedInterfaceEng = new Dictionary<string, string>();
        public static Dictionary<string, string> previousValuesCombinedInterfaceHexa = new Dictionary<string, string>();

        public static List<string> parametersCompareString = new List<string>();

        public static List<string> parametersArincEng = new List<string>();
        public static List<string> parametersDiscreteEng = new List<string>();
        public static List<string> parametersAnalogEng = new List<string>();

        public static double arincLastRecordedtime = 0;
        public static double discreteLastRecordedtime = 0;
        public static double analogLastRecordedtime = 0;

        public static void Main(string[] args)
        {
            bool arincData = false;
            bool discreteData = false;
            bool analogData = false;

            if (!Directory.Exists(parameterDataFolder))
            {
                Directory.CreateDirectory(parameterDataFolder);
            }
            string SessionId = Program.groundRun + SelectFileFromFolder.ArincSelectedFilePath.Split('\\')[SelectFileFromFolder.ArincSelectedFilePath.Split('\\').Length - 1].Split('_')[2];
            ParameterDataPath = Path.Combine(parameterDataFolder, "ConsolidatedData_Dec_" + SessionId);
            ParameterDataPathEng = Path.Combine(parameterDataFolder, "ConsolidatedData_Eng_" + SessionId);
            ParameterDataPathHexa = Path.Combine(parameterDataFolder, "ConsolidatedData_Hexa_" + SessionId);

            Console.WriteLine("Reading Arinc Files Started");
            recList_ArincEng = Arinc.readArincRecordedDataEng();
            icdDict_ArincEng = Arinc.readArincICDEng(icdKnownFilePath);
            icdList_ArincEng = Arinc.readIcdFilterArincEng(icdKnownFilePath);
            Console.WriteLine("Reading Arinc Files Stopped....\n");

            Console.WriteLine("Reading Discrete Files Started");
            recList_DiscreteEng = Discrete.readDiscreteRecordedDataEng();
            icdDict_DiscreteEng = Discrete.readDiscreteICDEng(icdKnownFilePath);
            icdList_DiscreteEng = Discrete.readIcdFilterDiscreteEng(icdKnownFilePath);
            Console.WriteLine("Reading DIscrete Files Stopped....\n");

            Console.WriteLine("Reading Analog Files Started");
            recList_AnalogEng = Analog.readAnalogRecordedDataEng();
            icdDict_AnalogEng = Analog.readAnalogICDEng(icdKnownFilePath);
            icdList_AnalogEng = Analog.readIcdFilterAnalogEng(icdKnownFilePath);
            Console.WriteLine("Reading Analog Files Stopped....\n");


            Console.WriteLine("Deocding Arinc Files Started");
            getArincData(recList_ArincEng, icdDict_ArincEng);
            Console.WriteLine("Deocding Arinc Files Stopped....\n");
            Console.WriteLine("Deocding Discrete Files Started");
            getDiscreteData(recList_DiscreteEng, icdDict_DiscreteEng);
            Console.WriteLine("Deocding Discrete Files Stopped....\n");
            Console.WriteLine("Deocding Analog Files Started");
            getAnalogData(recList_AnalogEng, icdDict_AnalogEng);
            Console.WriteLine("Deocding Analog Files Stopped....\n");

            //rowData_Interface = rowData_Interface.OrderBy(x => TimeStringToDouble(x["Time"])).ToList();
            //rowData_InterfaceRaw = rowData_InterfaceRaw.OrderBy(x => TimeStringToDouble(x["Time"])).ToList();
            rowData_InterfaceEng = rowData_InterfaceEng.OrderBy(x => TimeStringToDouble(x["Time"])).ToList();
            //rowData_InterfaceHexa = rowData_InterfaceHexa.OrderBy(x => TimeStringToDouble(x["Time"])).ToList();

            ////combine all the parameters
            //Console.WriteLine("Combining All Data Started");
            //for (int j = 0; j < rowData_Interface.Count; j++)
            //{
            //    Console.WriteLine("Combining_1 -> " + j.ToString() + " | " + Math.Round((double)((j + 1) * 100.0 / rowData_Interface.Count), 4).ToString() + " %" + " | " + rowData_Interface[j]["Time"]);
            //    var data = rowData_Interface[j];
            //    var dataRaw = rowData_InterfaceRaw[j];
            //    var dataEng = rowData_InterfaceEng[j];
            //    var dataHexa = rowData_InterfaceHexa[j];
            //    foreach (var param in parametersCompareString)
            //    {
            //        if (!data.ContainsKey(param))
            //        {
            //            data[param] = "--";
            //            dataRaw[param] = "--";
            //            dataEng[param] = "--";
            //            dataHexa[param] = "--";
            //        }
            //    }
            //}

            for (int j = rowData_InterfaceEng.Count - 1; j > 1; j--)
            {
                Console.WriteLine("Rem -> " + Math.Round((double)((rowData_InterfaceEng.Count - j - 1) * 100.0 / rowData_InterfaceEng.Count), 4).ToString() + " %");

                //var dataPrev = rowData_Interface[j - 1];
                var dataEngPrev = rowData_InterfaceEng[j - 1];
                //var dataHexaPrec = rowData_InterfaceHexa[j - 1];

                //var dataCur = rowData_Interface[j];
                var dataEngCur = rowData_InterfaceEng[j];
                //var dataHexaCur = rowData_InterfaceHexa[j];

                if (dataEngPrev["Time"] == dataEngCur["Time"])
                {
                    foreach (var key in dataEngCur.Keys.ToArray())
                    {
                        if (key != "Time")
                        {
                            if (dataEngCur.ContainsKey(key))
                            {
                                if (!dataEngPrev.ContainsKey(key))
                                {
                                    //rowData_Interface[j - 1][key] = dataCur[key];
                                    //rowData_InterfaceHexa[j - 1][key] = dataHexaCur[key];
                                    rowData_InterfaceEng[j - 1][key] = dataEngCur[key];
                                }
                            }
                            //if (!((dataCur[key] == "--") && (dataPrev[key] == "--")))
                            //{
                            //    if (dataCur[key] != "--")
                            //    {
                            //        rowData_Interface[j - 1][key] = dataCur[key];
                            //        rowData_InterfaceHexa[j - 1][key] = dataHexaCur[key];
                            //        rowData_InterfaceEng[j - 1][key] = dataEngCur[key];
                            //    }
                            //}
                        }
                    }
                    //rowData_Interface.RemoveAt(j);
                    //rowData_InterfaceHexa.RemoveAt(j);
                    rowData_InterfaceEng.RemoveAt(j);
                }
            }
            Console.WriteLine("Removing Duplicate TimeStamps Stopped....\n");

            Console.WriteLine("Duplicating Previous Data Started");
            for (int j = 1; j < rowData_InterfaceEng.Count; j++)
            {
                double currentTime = TimeStringToDouble(rowData_InterfaceEng[j]["Time"]);
                Console.WriteLine("Dup -> " + Math.Round((double)((j) * 100.0 / rowData_InterfaceEng.Count), 4).ToString() + " %");
                {
                    foreach (var param in interfaceSelectedParametrs)
                    {
                        if (!rowData_InterfaceEng[j].ContainsKey(param.dictKey))
                        {
                            if (rowData_InterfaceEng[j - 1].ContainsKey(param.dictKey))
                            {
                                if ((parametersArincEng.Contains(param.dictKey) && arincLastRecordedtime >= currentTime) ||
                              (parametersDiscreteEng.Contains(param.dictKey) && discreteLastRecordedtime >= currentTime) ||
                              (parametersAnalogEng.Contains(param.dictKey) && analogLastRecordedtime >= currentTime))
                                {
                                    rowData_InterfaceEng[j][param.dictKey] = rowData_InterfaceEng[j - 1][param.dictKey];
                                }
                                else
                                {
                                    rowData_InterfaceEng[j][param.dictKey] = "  ";
                                }
                            }
                            else
                            {
                                rowData_InterfaceEng[j][param.dictKey] = "  ";
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Duplicating Previous Data Stopped....\n");

            Console.WriteLine("Generating File Started");
            //generateFileForRGVS(rowData_InterfaceHexa, rowData_InterfaceEng, rowData_Interface, interfaceSelectedParametrs);
            generateFileForRGVS(rowData_InterfaceEng, interfaceSelectedParametrs);
            Console.WriteLine("Generating File Finished");

        }

        public static void getArincData(List<Arinc.Arinc_Recorded_DataEng> recList_ArincEng, Dictionary<string, List<ArincICDEng>> icdDict_ArincEng)
        {
            List<string> labelsArincEng = new List<string>();
            if (recList_ArincEng.Count > 0)
                arincLastRecordedtime = recList_ArincEng[recList_ArincEng.Count - 1].Time_double;

            foreach (string label in icdDict_ArincEng.Keys.ToArray())
            {
                foreach (ArincICDEng param in icdDict_ArincEng[label])
                {
                    parametersCompareString.Add(param.dictKey);
                    parametersArincEng.Add(param.dictKey);
                    if (!labelsArincEng.Contains(param.Label))
                    {
                        labelsArincEng.Add(param.Label);
                    }
                    interfaceSelectedParametrs.Add(new ICDEng(param.InterfaceID, param.SID, param.paramName, Convert.ToString(param.source), param.startBit, param.width, param.Label));
                }
            }

            for (int i = 0; i < recList_ArincEng.Count; i++)
            {
                string label = recList_ArincEng[i].Label;
                Console.WriteLine("Arinc Decoding -> " + Math.Round((double)((i + 1) * 100.0 / recList_ArincEng.Count), 4).ToString() + " %");

                Dictionary<string, string> row = new Dictionary<string, string>();
                Dictionary<string, string> rowRaw = new Dictionary<string, string>();
                Dictionary<string, string> rowEng = new Dictionary<string, string>();
                Dictionary<string, string> rowHexa = new Dictionary<string, string>();
                Dictionary<string, string> previousValuesArinc = new Dictionary<string, string>();
                Dictionary<string, string> previousValuesArincEng = new Dictionary<string, string>();
                Dictionary<string, string> previousValuesArincHexa = new Dictionary<string, string>();

                row["Time"] = recList_ArincEng[i].Time;
                rowEng["Time"] = recList_ArincEng[i].Time;
                rowRaw["Time"] = recList_ArincEng[i].Time;
                rowHexa["Time"] = recList_ArincEng[i].Time;

                Dictionary<string, ArincICDEng> decoded_params = new Dictionary<string, ArincICDEng>();
                decoded_params = recList_ArincEng[i].decode();

                foreach (var param in parametersArincEng)
                {
                    if (decoded_params.ContainsKey(param))
                    {
                        ArincICDEng decodedParam = decoded_params[param];
                        row[param] = decoded_params[param].decData;
                        rowRaw[param] = decoded_params[param].decData;
                        rowHexa[param] = (Convert.ToString(Convert.ToUInt32(decoded_params[param].decData, 10), 16));
                        rowEng[param] = getEngValue_Arinc(decodedParam);
                    }
                    else
                    {
                        //row[param] = "--";
                        //rowRaw[param] = "--";
                        //rowEng[param] = "--";
                        //rowHexa[param] = "--";

                    }
                }
                //rowData_Interface.Add(row);
                //rowData_InterfaceRaw.Add(rowRaw);
                //rowData_InterfaceHexa.Add(rowHexa);
                rowData_InterfaceEng.Add(rowEng);
            }
        }
        public static void getDiscreteData(List<Discrete.Discrete_Recorded_DataEng> recList_DiscreteEng, Dictionary<string, List<DiscreteICDEng>> icdList_DiscreteEng)
        {
            List<string> labelsDiscreteEng = new List<string>();
            if (recList_DiscreteEng.Count > 0)
                discreteLastRecordedtime = recList_DiscreteEng[recList_DiscreteEng.Count - 1].Time_double;
            foreach (string label in icdDict_DiscreteEng.Keys.ToArray())
            {
                foreach (DiscreteICDEng param in icdDict_DiscreteEng[label])
                {
                    parametersDiscreteEng.Add(param.dictKey);
                    parametersCompareString.Add(param.dictKey);
                    if (!labelsDiscreteEng.Contains(Convert.ToString(param.source)))
                    {
                        labelsDiscreteEng.Add(Convert.ToString(param.source));
                    }
                    interfaceSelectedParametrs.Add(new ICDEng(param.InterfaceID, param.SID, param.paramName, Convert.ToString(param.source), param.startBit, param.width, param.Label));
                }
            }

            for (int i = 0; i < recList_DiscreteEng.Count; i++)
            {
                string label = recList_DiscreteEng[i].Channel;
                Dictionary<string, string> row = new Dictionary<string, string>();
                Dictionary<string, string> rowRaw = new Dictionary<string, string>();
                Dictionary<string, string> rowEng = new Dictionary<string, string>();
                Dictionary<string, string> rowHexa = new Dictionary<string, string>();

                Dictionary<string, string> previousValuesDiscrete = new Dictionary<string, string>();
                Dictionary<string, string> previousValuesDiscreteEng = new Dictionary<string, string>();
                Dictionary<string, string> previousValuesDiscreteHexa = new Dictionary<string, string>();

                row["Time"] = recList_DiscreteEng[i].Time;
                rowEng["Time"] = recList_DiscreteEng[i].Time;
                rowRaw["Time"] = recList_DiscreteEng[i].Time;
                rowHexa["Time"] = recList_DiscreteEng[i].Time;


                Dictionary<string, DiscreteICDEng> decoded_params = new Dictionary<string, DiscreteICDEng>();
                decoded_params = recList_DiscreteEng[i].decode();

                foreach (var param in parametersDiscreteEng)
                {
                    if (decoded_params.ContainsKey(param))
                    {
                        DiscreteICDEng decodedParam = decoded_params[param];
                        row[param] = decoded_params[param].decData;
                        rowRaw[param] = decoded_params[param].decData;
                        rowHexa[param] = (Convert.ToString(Convert.ToUInt32(decoded_params[param].decData, 10), 16));
                        rowEng[param] = getEngValue_Discrete(decodedParam);
                    }
                    else
                    {
                        //row[param] = "--";
                        //rowRaw[param] = "--";
                        //rowEng[param] = "--";
                        //rowHexa[param] = "--";

                    }
                }
                //rowData_Interface.Add(row);
                //rowData_InterfaceRaw.Add(rowRaw);
                //rowData_InterfaceHexa.Add(rowHexa);
                rowData_InterfaceEng.Add(rowEng);
            }
        }

        public static void getAnalogData(List<Analog.Analog_Recorded_DataEng> recList_AnalogEng, Dictionary<string, List<AnalogICDEng>> icdList_AnalogEng)
        {
            List<string> labelsAnalogEng = new List<string>();
            if (recList_AnalogEng.Count > 0)
                analogLastRecordedtime = recList_AnalogEng[recList_AnalogEng.Count - 1].Time_double;

            foreach (string label in icdList_AnalogEng.Keys.ToList())
            {
                foreach (AnalogICDEng param in icdList_AnalogEng[label])
                {
                    parametersAnalogEng.Add(param.dictKey);
                    parametersCompareString.Add(param.dictKey);
                    if (!labelsAnalogEng.Contains(Convert.ToString(param.source)))
                    {
                        labelsAnalogEng.Add(Convert.ToString(param.source));
                    }
                    interfaceSelectedParametrs.Add(new ICDEng(param.InterfaceID, param.SID, param.paramName, Convert.ToString(param.source), param.startBit, param.width, param.Label));
                }
            }

            for (int i = 0; i < recList_AnalogEng.Count; i++)
            {
                Console.WriteLine("Analog Decoding -> " + Math.Round((double)((i + 1) * 100.0 / recList_AnalogEng.Count), 4).ToString() + " %");
                string label = recList_AnalogEng[i].Channel;
                Dictionary<string, string> row = new Dictionary<string, string>();
                Dictionary<string, string> rowRaw = new Dictionary<string, string>();
                Dictionary<string, string> rowEng = new Dictionary<string, string>();
                Dictionary<string, string> rowHexa = new Dictionary<string, string>();

                Dictionary<string, string> previousValuesAnalog = new Dictionary<string, string>();
                Dictionary<string, string> previousValuesAnalogEng = new Dictionary<string, string>();
                Dictionary<string, string> previousValuesAnalogHexa = new Dictionary<string, string>();

                row["Time"] = recList_AnalogEng[i].Time;
                rowEng["Time"] = recList_AnalogEng[i].Time;
                rowRaw["Time"] = recList_AnalogEng[i].Time;
                rowHexa["Time"] = recList_AnalogEng[i].Time;


                Dictionary<string, AnalogICDEng> decoded_params = new Dictionary<string, AnalogICDEng>();
                decoded_params = recList_AnalogEng[i].decode();


                foreach (var param in parametersAnalogEng)
                {
                    if (decoded_params.ContainsKey(param))
                    {
                        AnalogICDEng decodedParam = decoded_params[param];
                        row[param] = decoded_params[param].decData;
                        rowRaw[param] = decoded_params[param].decData;
                        rowHexa[param] = (Convert.ToString(Convert.ToUInt32(decoded_params[param].decData, 10), 16));
                        rowEng[param] = getEngValue_Analog(decodedParam);
                    }
                    else
                    {
                        //row[param] = "--";
                        //rowRaw[param] = "--";
                        //rowEng[param] = "--";
                        //rowHexa[param] = "--";
                    }
                }
                //rowData_Interface.Add(row);
                //rowData_InterfaceRaw.Add(rowRaw);
                //rowData_InterfaceHexa.Add(rowHexa);
                rowData_InterfaceEng.Add(rowEng);
            }
        }

        public static string getEngValue_Arinc(ArincICDEng decoded_param)
        {
            string paramData = "";
            if (decoded_param.decData != "--")
            {
                if (decoded_param.ssm_Type == "Type_1")
                {
                    if (decoded_param.ssm_Value == "0")
                    {
                        paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                    else if (decoded_param.ssm_Value == "3")
                    {
                        paramData = ((((((~Convert.ToInt32(decoded_param.decData)) + 1) & ((1 << decoded_param.width) - 1)) * Convert.ToDouble(decoded_param.LSB)) * (-1.0)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                    else
                    {
                        //Heighlight with red and use   rowEng[param] = (Convert.ToInt32(decoded_params[j].decData) * Convert.ToDouble(decoded_params[j].LSB)) + Convert.ToDouble(decoded_params[j].minVal).ToString();

                        paramData = "INVALID" + "_" + ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                        //paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                }

                else if (decoded_param.ssm_Type == "Type_2")
                {
                    if (decoded_param.ssm_Value == "0" || decoded_param.ssm_Value == "3")
                    {
                        paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                    else
                    {
                        paramData = "INVALID" + "_" + ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                        //paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                }
                else if (decoded_param.ssm_Type == "Type_3")
                {
                    if (decoded_param.ssm_Value == "0")
                    {
                        paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                    else
                    {
                        paramData = "INVALID" + "_" + ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                        //paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                }

                else if (decoded_param.ssm_Type == "Type_5")
                {
                    if (decoded_param.ssm_Value == "0" || decoded_param.ssm_Value == "2")
                    {
                        paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                    else if (decoded_param.ssm_Value == "3")
                    {
                        paramData = ((((((~Convert.ToInt32(decoded_param.decData)) + 1) & ((1 << decoded_param.width) - 1)) * Convert.ToDouble(decoded_param.LSB)) * (-1.0)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                    else
                    {
                        //Heighlight with red and use   rowEng[param] = (Convert.ToInt32(decoded_params[j].decData) * Convert.ToDouble(decoded_params[j].LSB)) + Convert.ToDouble(decoded_params[j].minVal).ToString();

                        paramData = "INVALID" + "_" + ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                        //paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                }

                else if (decoded_param.ssm_Type == "Type_6")
                {
                    if (decoded_param.ssm_Value == "0" || decoded_param.ssm_Value == "2" || decoded_param.ssm_Value == "1")
                    {
                        paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                    else if (decoded_param.ssm_Value == "3")
                    {
                        paramData = ((((((~Convert.ToInt32(decoded_param.decData)) + 1) & ((1 << decoded_param.width) - 1)) * Convert.ToDouble(decoded_param.LSB)) * (-1.0)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                    else
                    {
                        //Heighlight with red and use   rowEng[param] = (Convert.ToInt32(decoded_params[j].decData) * Convert.ToDouble(decoded_params[j].LSB)) + Convert.ToDouble(decoded_params[j].minVal).ToString();

                        paramData = "INVALID" + "_" + ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                        //paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                }
                else
                {
                    paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                }
                //rowEng[param] = Math.Round((Convert.ToInt32(decoded_params.decData) * Convert.ToDouble(decoded_params.LSB)) + Convert.ToDouble(decoded_params.minVal), 2).ToString();
            }
            else
            {
                paramData = decoded_param.decData;
            }
            return paramData;
        }
        public static string getEngValue_Discrete(DiscreteICDEng decoded_param)
        {
            string paramData = "";
            if (decoded_param.decData != "--")
            {
                if (decoded_param.ssm_Type == "Type_1")
                {
                    if (decoded_param.ssm_Value == "0")
                    {
                        paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                    else if (decoded_param.ssm_Value == "3")
                    {
                        paramData = (((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) * (-1.0)) + Convert.ToDouble(decoded_param.minVal)).ToString();

                    }
                    else
                    {
                        //Heighlight with red and use   rowEng[param] = (Convert.ToInt32(decoded_params[j].decData) * Convert.ToDouble(decoded_params[j].LSB)) + Convert.ToDouble(decoded_params[j].minVal).ToString();

                        paramData = "INVALID" + "_" + ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                        //paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                }

                else if (decoded_param.ssm_Type == "Type_2")
                {
                    if (decoded_param.ssm_Value == "0" || decoded_param.ssm_Value == "3")
                    {
                        paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                    else
                    {
                        paramData = "INVALID" + "_" + ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                        //paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                }
                else if (decoded_param.ssm_Type == "Type_3")
                {
                    if (decoded_param.ssm_Value == "0")
                    {
                        paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                    else
                    {
                        paramData = "INVALID" + "_" + ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                        //paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                }

                else if (decoded_param.ssm_Type == "Type_5")
                {
                    if (decoded_param.ssm_Value == "0" || decoded_param.ssm_Value == "2")
                    {
                        paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                    else if (decoded_param.ssm_Value == "3")
                    {
                        paramData = (((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) * (-1.0)) + Convert.ToDouble(decoded_param.minVal)).ToString();

                    }
                    else
                    {
                        //Heighlight with red and use   rowEng[param] = (Convert.ToInt32(decoded_params[j].decData) * Convert.ToDouble(decoded_params[j].LSB)) + Convert.ToDouble(decoded_params[j].minVal).ToString();

                        paramData = "INVALID" + "_" + ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                        //paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                }
                else
                {
                    paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                }
                //rowEng[param] = Math.Round((Convert.ToInt32(decoded_params.decData) * Convert.ToDouble(decoded_params.LSB)) + Convert.ToDouble(decoded_params.minVal), 2).ToString();
            }
            else
            {
                paramData = decoded_param.decData;
            }
            return paramData;
        }
        public static string getEngValue_Analog(AnalogICDEng decoded_param)
        {
            string paramData = "";
            if (decoded_param.decData != "--")
            {
                if (decoded_param.ssm_Type == "Type_1")
                {
                    if (decoded_param.ssm_Value == "0")
                    {
                        paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                    else if (decoded_param.ssm_Value == "3")
                    {
                        paramData = (((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) * (-1.0)) + Convert.ToDouble(decoded_param.minVal)).ToString();

                    }
                    else
                    {
                        //Heighlight with red and use   rowEng[param] = (Convert.ToInt32(decoded_params[j].decData) * Convert.ToDouble(decoded_params[j].LSB)) + Convert.ToDouble(decoded_params[j].minVal).ToString();

                        paramData = "INVALID" + "_" + ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                        //paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                }

                else if (decoded_param.ssm_Type == "Type_2")
                {
                    if (decoded_param.ssm_Value == "0" || decoded_param.ssm_Value == "3")
                    {
                        paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                    else
                    {
                        paramData = "INVALID" + "_" + ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                        //paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                }
                else if (decoded_param.ssm_Type == "Type_3")
                {
                    if (decoded_param.ssm_Value == "0")
                    {
                        paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                    else
                    {
                        paramData = "INVALID" + "_" + ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                        //paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                }
                else if (decoded_param.ssm_Type == "Type_5")
                {
                    if (decoded_param.ssm_Value == "0" || decoded_param.ssm_Value == "2")
                    {
                        paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                    else if (decoded_param.ssm_Value == "3")
                    {
                        paramData = (((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) * (-1.0)) + Convert.ToDouble(decoded_param.minVal)).ToString();

                    }
                    else
                    {
                        //Heighlight with red and use   rowEng[param] = (Convert.ToInt32(decoded_params[j].decData) * Convert.ToDouble(decoded_params[j].LSB)) + Convert.ToDouble(decoded_params[j].minVal).ToString();

                        paramData = "INVALID" + "_" + ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                        //paramData = ((Convert.ToInt32(decoded_param.decData) * Convert.ToDouble(decoded_param.LSB)) + Convert.ToDouble(decoded_param.minVal)).ToString();
                    }
                }
                else
                {
                    double rawVal = Convert.ToInt32(decoded_param.decData);
                    double LSB = Convert.ToDouble(decoded_param.LSB);
                    double MinVal = Convert.ToDouble(decoded_param.minVal);
                    paramData = ((LSB * rawVal) + MinVal).ToString();
                }
                //rowEng[param] = Math.Round((Convert.ToInt32(decoded_params.decData) * Convert.ToDouble(decoded_params.LSB)) + Convert.ToDouble(decoded_params.minVal), 2).ToString();
            }
            else
            {
                paramData = decoded_param.decData;
            }
            return paramData;
        }

        //public static void generateFileForRGVS(List<Dictionary<string, string>> selectedParamDataHexa, List<Dictionary<string, string>> selectedParamDataEng, List<Dictionary<string, string>> selectedParamData, List<ICDEng> selectedParametrs)
        public static void generateFileForRGVS(List<Dictionary<string, string>> selectedParamDataEng, List<ICDEng> selectedParametrs)
        {
            //Decimal Data
            //using (StreamWriter writerDecimal = new StreamWriter(ParameterDataPath))
            //{
            //    using (StreamWriter writerHexa = new StreamWriter(ParameterDataPathHexa))
            //    {
            using (StreamWriter writerEng = new StreamWriter(ParameterDataPathEng))
            {
                // writer.WriteLine($"A0, A1={selectedParametrs.Count},86, D1={selectedParamData.Count},calculate size of D1 ");
                //writerDecimal.Write("Time,");
                writerEng.Write("Time,");
                //writerHexa.Write("Time,");
                foreach (var param in selectedParametrs)
                {
                    //writerDecimal.Write(param.paramName + ",");
                    writerEng.Write(param.dictKey + ",");
                    //writerHexa.Write(param.paramName + ",");
                }
                //writerDecimal.WriteLine();
                writerEng.WriteLine();
                //writerHexa.WriteLine();
                //string Time = "";
                for (int i = 0; i < selectedParamDataEng.Count; i++)
                {
                    //Dictionary<string, string> dataDec = selectedParamData[i];
                    Dictionary<string, string> dataEng = selectedParamDataEng[i];
                    //Dictionary<string, string> dataHex = selectedParamDataHexa[i];

                    //writerDecimal.Write(dataDec["Time"] + ",");
                    writerEng.Write(dataEng["Time"] + ",");
                    //writerHexa.Write(dataHex["Time"] + ",");
                    foreach (var param in selectedParametrs)
                    {
                        try
                        {
                            //writerDecimal.Write(dataDec[param.dictKey] + ",");
                            writerEng.Write(dataEng[param.dictKey] + ",");
                            //writerHexa.Write(dataHex[param.dictKey] + ",");
                        }
                        catch
                        {
                            //writerDecimal.Write("--" + ",");
                            writerEng.Write("  " + ",");
                            //writerHexa.Write("--" + ",");
                            Console.WriteLine(i);
                            Console.WriteLine(param.dictKey);
                        }
                    }
                    //writerDecimal.WriteLine();
                    writerEng.WriteLine();
                    //writerHexa.WriteLine();
                }
            }
            //    }
            //}
        }
        public static double TimeStringToDouble(string Time)
        {
            double timeInDouble = 0;
            if (Time != null || !string.IsNullOrEmpty(Time))
            {
                var temp_str = Time.Split(':');
                var hrs_str = temp_str[0];
                var min_str = temp_str[1];
                var sec_str = temp_str[2].Split('.');
                timeInDouble = (Convert.ToDouble(hrs_str) * 3600000) + (Convert.ToDouble(min_str) * 60000) + (Convert.ToDouble(sec_str[0]) * 1000) + (Convert.ToDouble(sec_str[1] + "." + sec_str[2]) / 1);
            }
            return timeInDouble;
        }
    }
    public class ICDEng
    {
        public string paramName { get; set; }
        public int startBit { get; set; }
        public int width { get; set; }
        public string source { get; set; }
        public int InterfaceID { get; set; }
        public string Label { get; set; }
        public string SID { get; set; }
        public string dictKey { get; set; }
        public ICDEng()
        {

        }
        public ICDEng(int interfaceID, string sid, string paramName, string source, int fromBit, int toBit, string reg_sa_label)
        {
            this.InterfaceID = interfaceID;
            this.SID = sid;
            this.paramName = paramName;
            this.source = source;
            this.startBit = fromBit;
            this.width = toBit;
            this.Label = reg_sa_label;
            this.dictKey = this.paramName + "|" + this.source + "-" + this.Label + "-" + this.startBit + "-" + this.width;
        }
    }
    public class SelectFileFromFolder
    {
        public static string MilBusSelectedFilePath;
        public static string ArincSelectedFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\..\\Data\\02_PatsFormatData\\Arinc_Log_" + Program.SessionId + ".csv");
        public static string DiscreteSelectedFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\..\\Data\\02_PatsFormatData\\Discrete_Log_" + Program.SessionId + ".csv");
        public static string AnalogSelectedFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\..\\Data\\02_PatsFormatData\\Analog_Log_" + Program.SessionId + ".csv");
    }
}