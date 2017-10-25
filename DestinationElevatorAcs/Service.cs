using System;
using System.ServiceProcess;

namespace DestinationElevatorAcs
{
    public partial class Service : ServiceBase
    {
        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (!System.Diagnostics.EventLog.SourceExists(kEventSourceName))
            {
                System.Diagnostics.EventLog.CreateEventSource(kEventSourceName, kLogName);
            }

            System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.EventLogTraceListener(new System.Diagnostics.EventLog(kLogName) { Source = kEventSourceName }));

            try
            {
                var elipHost = System.Configuration.ConfigurationManager.AppSettings["ElipHost"];
                var elipPort = int.Parse(System.Configuration.ConfigurationManager.AppSettings["ElipPort"]);
                var elipHealthCheckPeriod = int.Parse(System.Configuration.ConfigurationManager.AppSettings["ElipHealthCheckPeriod"]);
                var elipConnectionRetryTimeout = int.Parse(System.Configuration.ConfigurationManager.AppSettings["ElipConnectionRetryTimeout"]);
                var elipIdleTimeMsec = int.Parse(System.Configuration.ConfigurationManager.AppSettings["ElipIdleTimeMsec"]);
                var elipReadBufferSize = int.Parse(System.Configuration.ConfigurationManager.AppSettings["ElipReadBufferSize"]);

                myElipClient = new Elip.Client(elipHost, elipPort, elipReadBufferSize, elipHealthCheckPeriod, elipConnectionRetryTimeout, elipIdleTimeMsec);
                AxTraxNg.Client axTraxNgClient = new AxTraxNg.Client();

                myElipClient.Connect();

                if (axTraxNgClient.Connect())
                {
                    axTraxNgClient.OnAccess += HandleAccessEvent;
                }
                else
                {
                    ExitCode = kExitCode;
                }

            }
            catch (Exception aEx)
            {
                System.Diagnostics.Trace.TraceError("Failed reading configuration from app.config file: error={0}", aEx.Message);

                ExitCode = kExitCode;
            }

            if (ExitCode != 0)
            {
                Stop();
            }
        }

        protected override void OnStop()
        {
            if (myElipClient != null)
            {
                myElipClient.Disconnect();
            }
        }

        private void HandleAccessEvent(object aSource, AxTraxNg.Client.AccessEventArgs aArgs)
        {
            Elip.Messages.ManualRegistration.Floor[] floors = Array.ConvertAll(aArgs.FloorsIds, v => new Elip.Messages.ManualRegistration.Floor((byte)v, Elip.Messages.ManualRegistration.DoorsOpeningE.Front));
            myElipClient.Send(new Elip.Messages.ManualRegistration(floors, Elip.Messages.ManualRegistration.AttributionE.General, (byte)aArgs.ReaderId, mySequenceNumber++));
        }

        private Elip.Client myElipClient = null;
        private volatile byte mySequenceNumber = 0;
        private const int kExitCode = 1066;
        private const string kLogName = "Destination Elevator ACS Events";
        private const string kEventSourceName = "Destination Elevator ACS Service";
    }
}
