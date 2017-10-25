using System;

namespace DestinationElevatorAcs.AxTraxNg
{
    internal class Client
    {
        public class AccessEventArgs : EventArgs
        {
            public AccessEventArgs(int aReaderId, int[] aFloorsIds) { ReaderId = aReaderId; FloorsIds = aFloorsIds; }
            public int ReaderId { get; set; }
            public int[] FloorsIds { get; set; }
        }

        public event EventHandler<AccessEventArgs> OnAccess;

        public bool Connect()
        {
            try
            {
                ServiceConsumptionObject.CallBackServiceHandling.Instance.SetSubsciberType(Rosslare.Musketeer.Shared.ACCSInterface.SubsciberListEnum.Client);
                ServiceConsumptionObject.CallBackServiceHandling.Instance.EnableSendingEvents = true;
                DateTime startTime = ServiceConsumptionObject.PoolingServiceHandling.Instance._service.GetServerTime();

                if (startTime != DateTime.MinValue)
                {
                    Rosslare.Musketeer.Shared.RILShared.MessageHandler.Instance.AccessEventReceived += AccessEventReceived;

                    System.Diagnostics.Trace.TraceInformation("AxTraxNG client is connected");

                    return true;
                }
                else
                {
                    System.Diagnostics.Trace.TraceError("AxTraxNG client failed connecting to server, received illegal server time");

                    return false;
                }
            }
            catch (Exception aEx)
            {
                System.Diagnostics.Trace.TraceError("AxTraxNG client failed connecting to server: error={0}", aEx.Message);

                return false;
            }
        }

        private void AccessEventReceived(object aSender, Rosslare.Musketeer.Shared.RosslareSystemEventArgs.DeviceEventArgs aEvent)
        {
            foreach (var entityInfo in (Rosslare.Musketeer.Shared.ACEntities.BaseEntityInfo[])aEvent.EntityArr)
            {
                try
                {
                    // Extract reader and doors from server received event and fire our oun event
                    var eventPanelInfo = (Rosslare.Musketeer.Shared.ACEntities.EventPanelInfo)entityInfo;
                    var eventEmployeeInfo = ServiceConsumptionObject.PoolingServiceHandling.Instance._service.GetEmployeeByCardNum(eventPanelInfo.iCardCode, (short)(eventPanelInfo.iSiteCode));
                    var doors = ServiceConsumptionObject.PoolingServiceHandling.Instance._service.GetOutputPanelByGroupPanel(eventPanelInfo.IdPanel, eventEmployeeInfo.IdOutputsGroup);
                    int[] doorIds = System.Array.ConvertAll(doors, v => v.IdInOut);

                    System.Diagnostics.Trace.TraceInformation("AxTraxNG client received access event: readerId={0}, fullName={1}, cardCode={2}, doors={3}", 
                        eventPanelInfo.IdReader, eventPanelInfo.tFullName, eventPanelInfo.iCardCode, string.Join(",", doorIds));

                    OnAccess(this, new AccessEventArgs(eventPanelInfo.IdReader, doorIds));
                }
                catch(Exception aEx)
                {
                    System.Diagnostics.Trace.TraceWarning("AxTraxNG client failed reading received access event, ignoring: error={0}", aEx.Message);
                }
            }
        }
    }
}
