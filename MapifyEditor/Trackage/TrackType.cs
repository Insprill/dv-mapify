using System;

namespace Mapify.Editor
{
    public enum TrackType
    {
        Road,
        Storage,
        Loading,
        In,
        Out,
        Parking,
        PassengerStorage,
        PassengerLoading
    }

    public static class TrackTypeExtensions
    {
        public static string LetterId(this TrackType trackType)
        {
            switch (trackType)
            {
                case TrackType.Storage:
                    return "S";
                case TrackType.Loading:
                    return "L";
                case TrackType.In:
                    return "I";
                case TrackType.Out:
                    return "O";
                case TrackType.Parking:
                    return "P";
                case TrackType.PassengerStorage:
                    return "SP";
                case TrackType.PassengerLoading:
                    return "LP";
                case TrackType.Road:
                    throw new ArgumentOutOfRangeException(nameof(trackType), trackType, $"{trackType} doesn't have a letter ID");
                default:
                    throw new ArgumentOutOfRangeException(nameof(trackType), trackType, null);
            }
        }
    }
}
