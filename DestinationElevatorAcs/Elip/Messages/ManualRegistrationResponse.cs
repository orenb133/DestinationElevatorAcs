namespace DestinationElevatorAcs.Elip.Messages
{
    internal class ManualRegistrationResponse : IMessage
    {
        public ManualRegistrationResponse(byte[] aBytes)
        {
            Bytes = aBytes;
            SequenceNumber = aBytes[kOffsetSequenceNumber];
            string assignedCarNumberStr = System.Text.Encoding.ASCII.GetString(aBytes, kOffsetAssignedCarNumber, kSizeAssignedCarNumber);

            if (byte.TryParse(assignedCarNumberStr, out byte assignedCarNumber))
            {
                AssignedCarNumber = assignedCarNumber;
            }
            else
            {
                AssignedCarNumber = 0;

                System.Diagnostics.Trace.TraceWarning("ELIP client failed parsing assigned car number from manual registration response, ignoring: value={0}", assignedCarNumberStr); 
            }

            string assignedBankNumberStr = System.Text.Encoding.ASCII.GetString(aBytes, kOffsetAssignedBankNumber, kSizeAssignedBankNumber);
            
            if (byte.TryParse(assignedBankNumberStr, out byte assignedBankNumber))
            {
                AssignedBankNumber = assignedBankNumber;
            }
            else
            {
                AssignedBankNumber = 0;

                System.Diagnostics.Trace.TraceWarning("ELIP client failed parsing assigned bank number from manual registration response, ignoring: value={0}", assignedBankNumberStr);
            }
        }
        
        public override string ToString()
        {
            return string.Format("ManualRegistrationResponse(SequenceNumber={0}, AssignedCarNumber={1}, AssignedBankNumber={2}",
                  SequenceNumber, AssignedCarNumber, AssignedBankNumber);
        }

        public byte[] Bytes { get; }
        public byte SequenceNumber { get; }
        public byte AssignedCarNumber { get; }
        public byte AssignedBankNumber { get; }

        private const byte kSizeSequenceNumber = 0x01;
        private const byte kSizeAssignedCarNumber = 0x03;
        private const byte kSizeAssignedBankNumber = 0x03;
        private const byte kOffsetSequenceNumber = Header.kSize;
        private const byte kOffsetAssignedCarNumber = kOffsetSequenceNumber + kSizeSequenceNumber;
        private const byte kOffsetAssignedBankNumber = kOffsetAssignedCarNumber + kSizeAssignedCarNumber;

    }
}
