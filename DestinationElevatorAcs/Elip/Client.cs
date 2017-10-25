using System;

namespace DestinationElevatorAcs.Elip
{
    internal class Client
    {
        public Client(string aHost, int aPort, int aReadBufferSize, int aHealthCheckPeriod, int aConnectionRetryTimeout, int aIdleTimeMsec)
        {
            myHost = aHost;
            myPort = aPort;
            myReadByfferSize = aReadBufferSize;
            myHealthCheckCycle = (int)(aHealthCheckPeriod / ((float)(aIdleTimeMsec) / 1000));
            myConnectionRetryTimeoutMsec = aConnectionRetryTimeout * 1000;
            myIdleTimeMsec = aIdleTimeMsec;
            myQueue = new System.Collections.Concurrent.ConcurrentQueue<Messages.IMessage>();
            myConnection = null;
            myShouldRun = false;
            myThread = null;
            myReadBuffer = new byte[aReadBufferSize];
        }

        public void Connect()
        {
            if (myConnection == null || !myConnection.Connected)
            {
                try
                {
                    System.Diagnostics.Trace.TraceInformation("ELIP client connecting: host={0}, port={1}", myHost, myPort);

                    myConnection = new System.Net.Sockets.TcpClient
                    {
                        ReceiveBufferSize = myReadByfferSize,
                    };

                    myConnection.Connect(myHost, myPort);

                    System.Diagnostics.Trace.TraceInformation("ELIP client is connected");
                }
                catch (Exception aEx)
                {
                    System.Diagnostics.Trace.TraceError("ELIP client connection failed: error={0}", aEx.Message);
                }
            }

            if (myThread == null || !myThread.IsAlive)
            {
                myThread = new System.Threading.Thread(ThreadTarget)
                {
                    IsBackground = true
                };
                myShouldRun = true;
                myThread.Start(); 
            }
        }

        public void Send(Messages.IMessage aMessage)
        {
            myQueue.Enqueue(aMessage);
        }

        public void Disconnect()
        {
            if (myThread != null && myThread.IsAlive)
            {
                myShouldRun = false;
                myThread.Join();
            }

            if (myConnection != null)
            {
                myConnection.Close();
            }
        }

        private void ThreadTarget()
        {
            int cycleNum = 0;

            while (myShouldRun)
            {
                cycleNum += 1;

                if (myConnection != null && myConnection.Connected)
                {
                    try
                    {
                        // Sending health check
                        if (cycleNum % myHealthCheckCycle == 0)
                        {
                            var healthCheckMessageToSend = new Messages.HealthCheck();

                            try
                            {
                                myConnection.Client.Send(healthCheckMessageToSend.Bytes);
                            }
                            catch (Exception aEx)
                            {
                                System.Diagnostics.Trace.TraceWarning("ELIP client failed sending health check message to socket, ignoring: error={0}", aEx.Message);
                            }
                        }

                        // Receiving message
                        if (myConnection.Available > Messages.Header.kSize)
                        {
                            try
                            {
                                myConnection.Client.Receive(myReadBuffer, (int)(Messages.Header.kSize), System.Net.Sockets.SocketFlags.None);
                                int receivedSize = myConnection.Client.Receive(myReadBuffer, Messages.Header.kSize, myReadBuffer[(int)Messages.Header.FieldOffsetsE.Length],
                                    System.Net.Sockets.SocketFlags.None);

                                if (receivedSize < myReadBuffer[(int)Messages.Header.FieldOffsetsE.Length])
                                {
                                    System.Diagnostics.Trace.TraceWarning("ELIP client failed receiving entire data from socket, ignoring: headerLength={0}, receivedSize={1}",
                                        myReadBuffer[(int)Messages.Header.FieldOffsetsE.Length], receivedSize);
                                }
                                else
                                {
                                    var message = Messages.Factory.CreateFromBytes(myReadBuffer);

                                    if (message != null)
                                    {
                                        System.Diagnostics.Trace.TraceInformation("ELIP client received a message: message={0}", message);
                                    }
                                }

                            }
                            catch (Exception aEx)
                            {
                                System.Diagnostics.Trace.TraceWarning("Failed receiving ELIP data from socket, ignoring: error={0}", aEx.Message);
                            }
                        }

                        // Sending queued messages
                        if (myQueue.TryDequeue(out Messages.IMessage messageToSend))
                        {
                            try
                            {
                                myConnection.Client.Send(messageToSend.Bytes);
                            }
                            catch (Exception aEx)
                            {
                                System.Diagnostics.Trace.TraceWarning("Failed sending ELIP message to socket, ignoring: error={0}, message={1}", aEx.Message, messageToSend);
                            }
                        }
                    }
                    catch (Exception aEx)
                    {
                        System.Diagnostics.Trace.TraceError("ELIP client received an unexpected exception, trying to continue: error={0}", aEx.Message);
                    }

                    System.Threading.Thread.Sleep(myIdleTimeMsec);

                }
                else
                {
                    System.Threading.Thread.Sleep(myConnectionRetryTimeoutMsec);

                    if (myConnection == null)
                    {
                        System.Diagnostics.Trace.TraceWarning("ELIP client was disconnected trying to reconnect");
                    }

                    Connect();
                }
            }
        }

        private int myReadByfferSize;
        private string myHost;
        private int myPort;
        private int myHealthCheckCycle;
        private int myConnectionRetryTimeoutMsec;
        private int myIdleTimeMsec;
        private System.Collections.Concurrent.ConcurrentQueue<Messages.IMessage> myQueue;
        private System.Net.Sockets.TcpClient myConnection;
        private System.Threading.Thread myThread;
        private volatile bool myShouldRun;
        private byte[] myReadBuffer;
    }
}
