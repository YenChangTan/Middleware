using Microsoft.AspNetCore.Http;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using Opc.Ua;
using Middleware.Model;
using Microsoft.AspNetCore.Server.Kestrel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Middleware.Fundamental
{
    public class OPCUA
    {
        public object sessionlock = new object();
        public Session session;

        private ApplicationInstance applicationInstance = new ApplicationInstance();

        public string filePath = "Opcsetting.xml";

        public async Task<JresultModel> LoadConfig()
        {
            try
            {
                // Load application configuration asynchronously
                await applicationInstance.LoadApplicationConfiguration(filePath, silent: false);
                await applicationInstance.CheckApplicationInstanceCertificate(false, 0);
                return new JresultModel { result = true, message = "Load application configuration successfully." };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("fail to load config");
                return new JresultModel { result = false, message = "Failed to load application configuration." };
            }
        }



        public JresultModel CheckStatusOpc()
        {
            try
            {
                OpcRead("ns=4;s=|var|AX-364ELA0MA1T.Application.Global.MachineStatus");
                if (session == null && !session.Connected)
                {
                    return new JresultModel { result = false };
                }
                return new JresultModel { result = true };
            }
            catch (Exception ex)
            {
                return new JresultModel { message = ex.ToString(), result = false };
            }
        }

        public async Task<JresultModel> OpcConnectProcessAsync(string ip, string port)
        {
            JresultModel jresultModel = new JresultModel();
            try
            {
                // Define endpoint URL dynamically based on parameters
                /*List<string> serverBaseAddresses= applicationInstance.ApplicationConfiguration.ServerConfiguration.BaseAddresses;
                Console.WriteLine(applicationInstance.ApplicationConfiguration.ApplicationName);
                Console.WriteLine("up");
                foreach (var address in serverBaseAddresses)
                {
                    Console.WriteLine(address);
                }
                Console.WriteLine("down");*/
                var endpointUrl = $"opc.tcp://{ip}:{port}";
                //Session session = null;

                try
                {
                    var selectedEndpoint = await SelectEndpointAsync(endpointUrl, useSecurity: false);
                    // CoreClientUtils.SelectEndpoint(endpointUrl, useSecurity: false);
                    // Create the session asynchronously
                    session = await Session.Create(
                   applicationInstance.ApplicationConfiguration,
                   new ConfiguredEndpoint(null, selectedEndpoint, Opc.Ua.EndpointConfiguration.Create(applicationInstance.ApplicationConfiguration)),
                   false, "", 10000, null, null);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error creating session: {ex.Message}");
                    return new JresultModel { result = false, message = "Failed to create OPC UA session." };
                }

                // Read Node


                return new JresultModel { result = true, message = "Connection successful." };
            }
            catch
            {
                return new JresultModel { result = false, message = "An unexpected error occurred." };
            }
        }

        public async Task<EndpointDescription> SelectEndpointAsync(string endpointUrl, bool useSecurity)
        {
            return await Task.Run(() =>
            {
                try
                {
                    return CoreClientUtils.SelectEndpoint(endpointUrl, useSecurity);
                }
                catch
                {
                    EndpointDescription description = new EndpointDescription();
                    return description;
                }
            });
        }

        public async Task<JresultModel> OpcDisConnectAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (session != null)
                    {
                        session.Close();   // Gracefully close the session.
                        session.Dispose(); // Clean up resources.
                    }

                    session = null; // Set to null to avoid reuse of a closed session.

                    return new JresultModel { result = true, message = "Session disconnected successfully." };
                }
                catch (Exception ex)
                {
                    return new JresultModel { result = false, message = "Error during disconnection: " + ex.Message };
                }
            });
        }

        public DataValue OpcRead(string SNode)
        {
            lock (sessionlock)
            {
                try
                {
                    Console.WriteLine(SNode);
                    var value = session.ReadValue(new NodeId(SNode));
                    return value;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error now is {ex.Message}");
                    if (ex.Message == "BadNotConnected" | ex.Message == "BadSecureChannelClosed" | ex.Message == "Object reference not set to an instance of an object.")
                    {
                        if (session != null)
                        {
                            session.Dispose();
                            session.Close();
                        }
                        session = null;
                    }
                    DataValue dataValue = new DataValue();
                    dataValue.Value = new Variant(ex.Message);
                    dataValue.StatusCode = Opc.Ua.StatusCodes.Bad;
                    return dataValue;
                }
            }
        }

        public async Task<DataValue> OpcReadAsync(string SNode)
        {
            return await Task.Run(() => OpcRead(SNode));
        }
        public JresultModel OpcWrite(string nodeId, object data)
        {
            lock (sessionlock)
            {
                try
                {
                    // Create NodeId object
                    var node = new NodeId(nodeId);

                    // Read the data type of the node
                    var nodeAttributes = session.ReadValue(node);
                    var expectedType = nodeAttributes.WrappedValue.TypeInfo.BuiltInType;

                    // Convert data if needed
                    object valueToWrite = data;

                    switch (expectedType)
                    {
                        case BuiltInType.Int32:
                            valueToWrite = Convert.ToInt32(data);
                            break;
                        case BuiltInType.String:
                            valueToWrite = data.ToString();
                            break;
                        case BuiltInType.Float:
                            valueToWrite = Convert.ToSingle(data);
                            break;
                        case BuiltInType.Int16:
                            valueToWrite = Convert.ToInt16(data);
                            break;
                        // Add more cases for different types
                        case BuiltInType.Boolean:
                            valueToWrite = Convert.ToBoolean(data);
                            break;
                        default:
                            throw new Exception($"Unsupported data type: {expectedType}");
                    }

                    // Create the DataValue object
                    var writeValue = new DataValue
                    {
                        Value = new Variant(valueToWrite),
                        StatusCode = Opc.Ua.StatusCodes.Good,
                        SourceTimestamp = DateTime.UtcNow
                    };

                    // Write to the node
                    StatusCodeCollection results = null;
                    DiagnosticInfoCollection diagnosticInfos = null;
                    session.Write(null, new WriteValueCollection { new WriteValue { NodeId = node, AttributeId = Attributes.Value, Value = writeValue } }, out results, out diagnosticInfos);

                    // Check write result
                    if (results != null && results[0] == Opc.Ua.StatusCodes.Good)
                    {
                        return new JresultModel { result = true, message = "Write operation successful." };
                    }
                    else
                    {
                        return new JresultModel { result = false, message = "Write operation failed." };
                    }
                }
                catch (Exception ex)
                {
                    return new JresultModel { result = false, message = $"Exception: {ex.Message}" };
                }
            }
        }

        public async Task<JresultModel> OpcWriteAsync(string nodeId, object data)
        {
            return await Task.Run(() => OpcWrite(nodeId, data));
        }
    }
}
