namespace DestinationElevatorAcs.Elip.Messages
{
    internal class ManualRegistration : IMessage
    {
        public enum AttributionE
        {
            General = 0x30,      // '0'
            Handicapped = 0x31,  // '1'
            Vip = 0x32           // '2'
        }

        public enum DoorsOpeningE
        {
            None = 0,  // 00
            Front = 1, // 01
            Rear = 2,  // 10
            Both = 3,  // 11
        }

        public struct Floor
        {
            public Floor(byte aNumber, DoorsOpeningE aDoorOpening) { Number = aNumber; DoorOpening = aDoorOpening; }
            public byte Number { get; }
            public DoorsOpeningE DoorOpening { get; }
            public override string ToString() { return string.Format("(Floor(Number={0}, DoorOpening={1})", Number, DoorOpening); }
        }

        public ManualRegistration(Floor[] aAccessibleFloors, AttributionE aAttribution, byte aCardReaderId, byte aSequenceNumber)
        {
            AccessibleFloors = aAccessibleFloors;
            Attribution = aAttribution;
            CardReaderId = aCardReaderId;
            SequenceNumber = aSequenceNumber;
            string cardReaderIdStr = aCardReaderId.ToString("0000");
            Bytes = new byte[] { (byte) Header.LengthsE.ManualRegistration,      // Length byte       
                                 Header.kVersion,                                // Version byte
                                 0x00, 0x00,                                     // 2 Reserved bytes
                                 (byte)Header.CommandsE.ManualRegistration,      // Command byte                               
                                 (byte)cardReaderIdStr[0],                       // Card reader number as 4 chars
                                 (byte)cardReaderIdStr[1],
                                 (byte)cardReaderIdStr[2],
                                 (byte)cardReaderIdStr[3],  
                                 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,  // 64 Bytes of non allowed floors
                                 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                                 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                                 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                                 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                 (byte)Attribution,                                // Attribution char
                                 SequenceNumber,                                   // Sequence number byte
                                 0x00, 0x00, 0x00, 0x00, 0x00 };                   // 5 Reserved bytes


            foreach (var floor in aAccessibleFloors)
            {
                if (floor.Number < KMinFloorNumber)
                {
                    System.Diagnostics.Trace.TraceWarning("ELIP client received received lower than minimum floor number, ignoring: minimum={0}, value={1}", KMinFloorNumber, floor.Number);

                    continue;
                }

                // Floor number -1 since minimal floor number is 1 but 0 in our array
                // *2 to get the lowest bit location of the floor and /8 for the byte location within the array
                // Example: Floor 242 -> ((242 - 1) * 2) / 8 = 60 Which is exactly the 61th byte
                int byteIndex = (int)(((floor.Number - 1) * 2) / 8);

                // Now add the correct message offset
                byteIndex += kOffsetAccessibleFloors;

                // Floor number -1 since minimal floor number is 1 but 0 in our array
                // We have 4 floors in one bytes, so the location of a floor is determined by %4, but each floor takes 2 bits
                // So we need to *2
                // Example Floor 242 -> ((242 - 1) % 4) * 2 = 2 Which is exactly the 3rd bit shift left offset
                int doorOpeningOffset = (((floor.Number - 1) % 4) * 2);

                // Now we just place it 
                int byteValue = Bytes[byteIndex];
                Bytes[byteIndex] = (byte)(byteValue | (int)floor.DoorOpening << doorOpeningOffset); 
            }
        }

        public override string ToString()
        {
            return string.Format("ManualRegistration(AccessibleFloors=[{0}], Attribution={1}, CardReaderId={2}, SequenceNumber={3}", 
                string.Join(",", AccessibleFloors), Attribution, CardReaderId, SequenceNumber);
        }

        public byte[] Bytes { get; }
        public Floor[] AccessibleFloors { get; }
        public AttributionE Attribution { get; }
        public byte CardReaderId { get; }
        public byte SequenceNumber { get; }

        private const byte kSizeCardReaderId = 0x04;
        private const byte kOffsetAccessibleFloors = Header.kSize + kSizeCardReaderId;
        private const byte KMinFloorNumber = 1;
    }
}
