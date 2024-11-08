using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Fundamental
{
    public class TCP
    {
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
            catch
            {
                return false;
            }
        }
        public PCBResult getPCBInfo(string UUID)
        {
            PCBResult pcbResult = new PCBResult();
            int resultCode = 0;
            bool isDefected = false;
            resultCode = Send(UUID);
            if (resultCode != 0)
            {
                pcbResult.resultCode = resultCode;
                return pcbResult;
            }
            resultCode = Receive(UUID, isDefected);
            if (resultCode != 0)
            {
                pcbResult.resultCode = resultCode;
                return pcbResult;
            }
            pcbResult.isDefected = isDefected;
            return pcbResult;
        }
        public int Send(string UUID)
        {
            int resultCode = 0;
            try
            {
                int byteLength = UUID.Length + 3;
                byte[] bytesToSend = new byte[byteLength];
                byte[] bytesToSendWithoutCRC = new byte[byteLength - 2];
                Buffer.BlockCopy(Encoding.ASCII.GetBytes(UUID), 0, bytesToSend, 0, byteLength - 3);
                Array.Copy(Encoding.ASCII.GetBytes(UUID), bytesToSendWithoutCRC, byteLength - 3);
                CRCCal(bytesToSendWithoutCRC).CopyTo(bytesToSend, byteLength - 2);
                Console.WriteLine(ByteArrayToHex(bytesToSend));
                sock.Send(bytesToSend);
                return resultCode;
            }
            catch (SocketException ex)
            {
                resultCode = 5;
                return resultCode;
            }
            catch (Exception e)
            {
                resultCode = 6;
                return resultCode;
            }
        }

        public int Receive(string UUID, bool isDefected)
        {
            byte[] buffer = new byte[1024];
            byte[] CRC = new byte[2];
            int resultCode = 0;
            try
            {
                int bytesRead = sock.Receive(buffer);
                if (bytesRead == 8)
                {
                    switch (Encoding.ASCII.GetString(buffer, 0, 8))
                    {
                        case "CRCWRONG":
                            resultCode = 1;
                            break;
                        case "RCVERROR":
                            resultCode = 2;
                            break;
                        case "NOTEXIST":
                            resultCode = 3;
                            break;
                        case "APIERROR":
                            resultCode = 4;
                            break;
                    }
                    return resultCode;
                }
                else if (bytesRead == UUID.Length + 3)
                {
                    string UUIDToCheck = Encoding.ASCII.GetString(buffer, 0, bytesRead - 3);
                    if (UUIDToCheck == UUID)
                    {
                        byte[] receiveByteWithoutCRC = new byte[bytesRead - 2];
                        Array.Copy(buffer, receiveByteWithoutCRC, bytesRead - 2);
                        Array.Copy(buffer, bytesRead - 2, CRC, 0, 2);
                        if (CRC.SequenceEqual(CRCCal(receiveByteWithoutCRC)))
                        {
                            if (buffer[bytesRead - 3] == (byte)1)
                            {
                                isDefected = true;
                            }
                            return resultCode;
                        }
                        else
                        {
                            resultCode = 1;
                            return resultCode;
                        }
                    }
                    else
                    {
                        resultCode = 3;
                        return resultCode;
                    }
                }
                else
                {
                    resultCode = 2;
                    return resultCode;
                }
            }
            catch (SocketException ex)
            {
                resultCode = 5;
                return resultCode;
            }
            catch (Exception e)
            {
                resultCode = 6;
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
