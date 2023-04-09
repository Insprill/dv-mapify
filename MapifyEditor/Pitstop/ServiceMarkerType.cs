namespace Mapify.Editor
{
    public enum ServiceMarkerType
    {
        OPEN,
        CLOSED
    }

    public static class ServiceMarkerTypeExtensions
    {
        public static VanillaAsset ToVanillaAsset(this ServiceMarkerType markerType)
        {
            return markerType == ServiceMarkerType.OPEN ? VanillaAsset.ServiceStationMarkerOpen : VanillaAsset.ServiceStationMarkerClosed;
        }
    }
}
