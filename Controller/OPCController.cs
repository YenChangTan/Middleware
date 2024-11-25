using Middleware.DataHolder;
using Middleware.Fundamental;
using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Controller
{
    public class OPCController//not used.
    {

        public async Task StatusChecker(OPCUA OPC, string NodeID, CancellationTokenSource cts, CancellationToken token)
        {
            await Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {

                    DataValue datavalue = OPC.OpcReadAsync(NodeID).Result;
                    if (datavalue.StatusCode == StatusCodes.Bad)
                    {
                        if (datavalue.Value.ToString() == "BadNotConnected" | datavalue.Value.ToString() == "BadSecureChannelClosed" | datavalue.Value.ToString() == "Object reference not set to an instance of an object.")
                        {
                            break;
                        }
                    }
                    else
                    {
                        switch (datavalue.Value.ToString())
                        {
                            case "0":
                            case "1":
                            case "2":
                                //Console.WriteLine("Free");
                                break;
                            case "3":
                                //Console.WriteLine("Error");
                                //need to post to MES
                                //Task.Run(() => MESHttpRequest.UpdateError());
                                break;
                        }
                    }
                    Task.Delay(1000).Wait();
                }
                if (!token.IsCancellationRequested)
                {
                    cts.Cancel();
                }
            }, token);
        }
    }
}
