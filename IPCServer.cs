// Copyright(c) 2019-2022 pypy, Natsumi and individual contributors.
// All rights reserved.
//
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace VRCX
{
    internal class IPCServer
    {
        public static readonly IPCServer Instance;
        public static readonly List<IPCClient> Clients = new List<IPCClient>();

        static IPCServer()
        {
            Instance = new IPCServer();
        }

        public void Init()
        {
            new IPCServer().CreateIPCServer();
        }

        public static async Task Send(IPCPacket ipcPacket)
        {
            foreach (var client in Clients)
            {
                await client.Send(ipcPacket);
            }
        }

        public void CreateIPCServer()
        {
            var ipcServer = new NamedPipeServerStream("vrcx-ipc", PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            ipcServer.BeginWaitForConnection(DoAccept, ipcServer);
        }

        private void DoAccept(IAsyncResult asyncResult)
        {
            var ipcServer = (NamedPipeServerStream)asyncResult.AsyncState;

            try
            {
                ipcServer.EndWaitForConnection(asyncResult);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var ipcClient = new IPCClient(ipcServer);
            Clients.Add(ipcClient);
            ipcClient.BeginRead();
            CreateIPCServer();
        }
    }
}