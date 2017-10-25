namespace DestinationElevatorAcs.Elip.Messages
{
    internal class Factory
    {
        public static IMessage CreateFromBytes(byte[] aBytes)
        {
            if (aBytes[(int)Header.FieldOffsetsE.Version] != Header.kVersion)
            {
                System.Diagnostics.Trace.TraceWarning("ELIP client received unknown protocol version data: data={0}", 
                    aBytes[(int)Header.FieldOffsetsE.Version]);


                return null;
            }

            if (aBytes[(int)Header.FieldOffsetsE.Command] == (byte)Header.CommandsE.RegistrationResponse)
            {
                return new ManualRegistrationResponse(aBytes);
            }

            System.Diagnostics.Trace.TraceWarning("ELIP client received a message with unknown command field value: command={0}",
                aBytes[(int)Header.FieldOffsetsE.Command]);

            return null;
        }
    }
}
