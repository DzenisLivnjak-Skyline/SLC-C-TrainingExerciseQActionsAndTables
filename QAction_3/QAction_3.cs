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
            string dataFileFind = Convert.ToString(protocol.GetParameter(1));

            if (String.IsNullOrWhiteSpace(dataFileFind))
            {
                protocol.Log(
                	$"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|: Path has not been entered (Param 500).",
                	LogType.Error,
                	LogLevel.NoLogging);
                    protocol.ClearAllKeys(10);
                    protocol.ClearAllKeys(20);
                return;

            }


            SecurePath securePath = SecurePath.CreateSecurePath(dataFileFind);

            if (!File.Exists(securePath))
            {
                protocol.Log(
                	$"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|: JSON file not found at path {securePath}",
                	LogType.Error,
                	LogLevel.NoLogging);
                    protocol.ClearAllKeys(10);
                    protocol.ClearAllKeys(20);
                return;
            }

            var json = File.ReadAllText(securePath);
            string jsonData = Convert.ToString(json);
            TransportStreams deserializedData = SecureNewtonsoftDeserialization.DeserializeObject<TransportStreams>(jsonData);

            double pollTimestamp = DateTime.UtcNow.ToOADate();

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
                        Transportstreamslastpolltime_16 = pollTimestamp,
                    }.ToObjectArray();
                }
            }
            protocol.FillArray(Parameter.Transportstreams.tablePid, tableData.Values.ToList(), NotifyProtocol.SaveOption.Full);
          
            if (deserializedData != null && deserializedData.Transport_streams != null && deserializedData.Transport_streams.Count > 0)
            {
                var setColumnsData = new Dictionary<int, List<object>>();

                var serviceKeys = new List<object>();
                var serviceNames = new List<object>();
                var serviceTypes = new List<object>();
                var serviceProviders = new List<object>();
                var lastPollTimes = new List<object>();
                var transportStreamIds = new List<object>();

                foreach (var ts in deserializedData.Transport_streams)
                {
                    if (ts == null || ts.Services == null || ts.Services.Count == 0)
                        continue;

                    foreach (var svc in ts.Services)
                    {
                        if (svc == null)
                            continue;

                        serviceKeys.Add(svc.Service_id);
                        serviceNames.Add(svc.Service_name);
                        serviceTypes.Add(svc.Service_type);
                        serviceProviders.Add(svc.Service_provider);
                        lastPollTimes.Add(pollTimestamp);

                        transportStreamIds.Add(ts.Ts_id);
                    }
                }

                if (serviceKeys.Count > 0)
                {
                    setColumnsData[20] = serviceKeys;
                    setColumnsData[Parameter.Services.Pid.servicesname_22] = serviceNames;
                    setColumnsData[23] = serviceTypes;
                    setColumnsData[24] = serviceProviders;
                    setColumnsData[25] = lastPollTimes;
                    setColumnsData[26] = transportStreamIds;

                    protocol.SetColumns(setColumnsData);
                }
            }


        }

        catch (Exception ex)
        {
            protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
        }
    }
}
