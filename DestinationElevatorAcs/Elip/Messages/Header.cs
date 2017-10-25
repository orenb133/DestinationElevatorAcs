namespace DestinationElevatorAcs.Elip.Messages
{
    internal abstract class Header
    {
        public enum CommandsE
        {
            ManualRegistration = 0x20,
            HealthCheck = 0x11,
            RegistrationResponse = 0x90
        }

        public enum LengthsE
        {
            ManualRegistration = 0x4c,
            HealthCheck = 0x04,
            RegistrationResponse = 0x06
        }

        public enum FieldSizesE
        {
            Length = 0x01,
            Version = 0x01,
            Reserved = 0x02,
            Command = 0x01,
        }

        public enum FieldOffsetsE
        {
            Length = 0x00,
            Version = Length + FieldOffsetsE.Length,
            Reserved = Version + FieldOffsetsE.Version,
            Command = Reserved + FieldOffsetsE.Reserved,
        }

        public const byte kVersion = 0x41; // 'A'
        public const byte kSize = 0x05; // Total
    }
}
