namespace DestinationElevatorAcs.Elip.Messages
{
    internal class HealthCheck : IMessage
    {
        public HealthCheck()
        {
            Bytes = new byte[] { (byte)Header.LengthsE.HealthCheck,         // Length byte       
                                 Header.kVersion,                           // Version byte
                                 0x00, 0x00,                                // 2 Reserved bytes
                                 (byte)Header.CommandsE.HealthCheck,        // Command byte                               
                                 0x00, 0x00, 0x00 };                        // 3 reserved bytes  
        }
        
        public override string ToString()
        {
            return "HealthCheck()";
        }

        public byte[] Bytes { get; }
    }
}
