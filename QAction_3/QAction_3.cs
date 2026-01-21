using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using QAction_3;
using Skyline.DataMiner.Scripting;
using Skyline.DataMiner.Utils.Protocol.Extension;
using Skyline.DataMiner.Utils.SecureCoding.SecureIO;
using Skyline.DataMiner.Utils.SecureCoding.SecureSerialization.Json.Newtonsoft;

/// <summary>
/// DataMiner QAction Class.
/// </summary>
public static class QAction
{
    /// <summary>
    /// The QAction entry point.
    /// </summary>
    /// <param name="protocol">Link with SLProtocol process.</param>
    public static void Run(SLProtocol protocol)
    {
        try
        {
            string dataFileFind = @"C:\\Skyline DataMiner\\Documents\\Excercise QActions And Tables dzenis\\Data.json";

            SecurePath securePath = SecurePath.CreateSecurePath(dataFileFind);

            if (!File.Exists(securePath))
            {
                protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|: JSON file not found at path {securePath}", LogType.Error, LogLevel.NoLogging);
                return;
            }
            var json = File.ReadAllText(securePath);
            string jsonData = Convert.ToString(json);
            TransportStreams deserializedData = SecureNewtonsoftDeserialization.DeserializeObject<TransportStreams>(jsonData);

            Dictionary<string, object[]> tableData = new Dictionary<string, object[]>();

            if (deserializedData == null || deserializedData.Transport_streams == null || deserializedData.Transport_streams.Count == 0)
            {
                protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|: No transport streams found in the JSON data.", LogType.DebugInfo, LogLevel.NoLogging);
            }
            else
            {
                foreach (var ts in deserializedData.Transport_streams)
                {
                    tableData[ts.Ts_id] = new TransportstreamsQActionRow
                    {
                        Transportstreamsid_11 = ts.Ts_id,
                        Transportstreamsname_12 = ts.Ts_name,
                        Transportstreamsmulticast_13 = ts.Multicast,
                        Transportstreamssourceip_14 = ts.SourceIp,
                        Transportstreamsnetworkid_15 = ts.Network_id.ToString(CultureInfo.InvariantCulture),
                        Transportstreamslastpolltime_16 = DateTime.Now.ToOADate(),
                    }.ToObjectArray();
                }
            }

            protocol.FillArray(Parameter.Transportstreams.tablePid, tableData.Values.ToList(), NotifyProtocol.SaveOption.Full);
        }

        catch (Exception ex)
        {
            protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
        }
    }
}
