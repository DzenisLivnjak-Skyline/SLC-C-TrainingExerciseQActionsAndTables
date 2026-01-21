using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
			
        }

		catch (Exception ex)
		{
			protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
		}
	}
}
