using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Opc.Ua;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Middleware.Fundamental
{
    public class TCP
    {

        public int RecipeId = 0; //recipe id
        public string SentString;
        public string ReceivedString;

        public class PCBResult
        {
            public int resultCode = 0;
            public bool isDefected = false;
        }

        public Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public bool ConnectTcp(string IP, string Port)
        {
            try
            {
                using (Socket testSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    var result = testSock.BeginConnect(IP, Convert.ToInt32(Port.Trim()), null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
                    if (!success | !testSock.Connected)
                    {
                        return false;
                    }
                    testSock.EndConnect(result);
                }
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sock.Connect(IPAddress.Parse(IP), Convert.ToInt32(Port.Trim()));
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public int SendRecipe(int recipeId)
        {
            int resultCode = 0;
            try
            {
                RecipeId = recipeId;
                resultCode = Send("RECIPE");
                if (resultCode != 1)
                {
                    return resultCode;
                }
                resultCode = Receive();
                return resultCode;
            }
            catch
            {
                resultCode = 3;
                return 3;
            }
        }
        public int Send(string command)//return 0 means data error, 1: no error, 2: connection aborted
        {
            int DataLength = 7;
            byte[] bytesToSend = new byte[DataLength + 2];
            byte[] bytesToSendWithoutCRC = new byte[DataLength];
            try
            {
                if (command == "RECIPE")
                {
                    Array.Copy(Encoding.ASCII.GetBytes(command), bytesToSendWithoutCRC, DataLength - 1);
                    bytesToSendWithoutCRC[DataLength - 1] = (byte)( RecipeId + '0');
                    RecipeId = 0;
                    sock.Send(bytesToSendWithoutCRC);
                    SentString = Encoding.ASCII.GetString(bytesToSendWithoutCRC);
                    return 1;//success
                }
                else
                {
                    return 0;
                }
            }
            catch (SocketException ex)
            {
                return 2;//fail to send due to the connection aborted.
            }
            catch (Exception e)
            {
                return 3;//unexpected error.
            }
        }

        public async Task<string> ReceiveString()
        {
            byte[] buffer = new byte[10];
            
            int bytesRead = await sock.ReceiveAsync(buffer);
            Console.WriteLine(ByteArrayToHex(buffer));
            return Encoding.ASCII.GetString(buffer, 0, bytesRead);
        }

        public async Task<int> ReceiveFromMagazineLoader()
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await sock.ReceiveAsync(buffer);
                Logger.LogMessage(ByteArrayToHex(buffer), "tcp");
                if (Encoding.ASCII.GetString(buffer, 0, bytesRead).Contains("11"))
                {
                    //try
                    //{
                    //    var receiveTask = Task.Run(async () =>
                    //    {
                    //        for (int i = 0; i < 5; i++)
                    //        {
                    //            buffer = new byte[50];
                    //            bytesRead = await sock.ReceiveAsync(buffer);
                    //        }
                    //    });
                    //    var timeOutTask = await Task.WhenAny(receiveTask, Task.Delay(200));
                    //    return 1;
                    //}
                    //catch
                    //{
                    //    return 1;
                    //}
                    return 1;
                }
                else if (Encoding.ASCII.GetString(buffer, 0, bytesRead).Contains("GJ"))
                {
                    //try
                    //{
                    //    var receiveTask = Task.Run(async () =>
                    //    {
                    //        for (int i = 0; i < 5; i++)
                    //        {
                    //            buffer = new byte[50];
                    //            bytesRead = await sock.ReceiveAsync(buffer);
                    //        }
                    //    });
                    //    var timeOutTask = await Task.WhenAny(receiveTask, Task.Delay(200));
                    //    return 2;
                    //}
                    //catch
                    //{
                    //    return 2;
                    //}
                    return 2;
                }
                else
                {
                    return 0;
                }
            }
            catch(SocketException ex)
            {
                return 3;
            }
            catch(Exception ex)
            {
                return 0;
            }
        }

        public int Receive()
        {
            int DataLength = 7;
            int resultCode = 0; // 0: data error, 1: success, 2: connection aborted, 3: unexpected error
            byte[] buffer = new byte[1024];
            byte[] receiveByteWithoutCRC = new byte[DataLength];
            try
            {
                int bytesRead = sock.Receive(buffer);
                if (bytesRead == DataLength)
                {
                    if (Encoding.ASCII.GetString(buffer, 0, DataLength) == SentString)
                    {
                        resultCode = 1;
                        return resultCode;
                    }
                    else
                    {
                        resultCode = 0;
                        return resultCode;
                    }
                }
                else
                {
                    resultCode = 0;
                    return resultCode;
                }
            }
            catch (SocketException ex)
            {
                resultCode = 2;
                return resultCode;
            }
            catch (Exception ex)
            {
                resultCode = 3;
                return resultCode;
            }
        }
        public byte[] CRCCal(byte[] data)
        {
            ushort crc = 0xFFFF;
            foreach (byte element in data)
            {
                crc ^= element;
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x0001) == 1)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            byte[] CRC = { (byte)(crc & 0xFF), (byte)((crc >> 8) & 0xFF) };
            return CRC;
        }

        public string ByteArrayToHex(byte[] bytes)
        {
            string separator = " ";
            StringBuilder hex = new StringBuilder(bytes.Length * (2 + separator.Length));
            for (int i = 0; i < bytes.Length; i++)
            {
                if (i > 0)
                {
                    hex.Append(separator);
                }
                hex.AppendFormat("{0:X2}", bytes[i]);
            }
            return hex.ToString();
        }
    }
}
