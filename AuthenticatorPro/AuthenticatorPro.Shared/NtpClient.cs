using System;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace AuthenticatorPro
{
    public static class NtpClient
    {
        public static async void SynchronizeDeviceTime()
        {
            DatagramSocket socket = new DatagramSocket();
            socket.MessageReceived += DatagramSocket_MessageReceived;
            await socket.ConnectAsync(new HostName("time.windows.com"), "123");

            using (DataWriter writer = new DataWriter(socket.OutputStream))
            {
                byte[] container = new byte[48];
                container[0] = 0x1B;

                writer.WriteBytes(container);
                await writer.StoreAsync();
            }
        }

        private static void DatagramSocket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            using (DataReader reader = args.GetDataReader())
            {
                byte[] b = new byte[48];

                reader.ReadBytes(b);

                App.NtpTimeOffset = GetNetworkTime(b) - DateTime.UtcNow;
            }
        }

        private static DateTime GetNetworkTime(byte[] rawData)
        {
            //Offset to get to the "Transmit Timestamp" field (time at which the reply 
            //departed the server for the client, in 64-bit timestamp format."
            const byte serverReplyTime = 40;

            //Get the seconds part
            ulong intPart = BitConverter.ToUInt32(rawData, serverReplyTime);

            //Get the seconds fraction
            ulong fractPart = BitConverter.ToUInt32(rawData, serverReplyTime + 4);

            //Convert From big-endian to little-endian
            intPart = SwapEndianness(intPart);
            fractPart = SwapEndianness(fractPart);

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

            //**UTC** time
            var networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);

            return networkDateTime;
        }

        private static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }
    }
}
