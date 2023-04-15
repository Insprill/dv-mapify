using System;

namespace Mapify.Editor
{
    public enum ItemType
    {
        Boombox,
        CassetteAlbum01,
        CassetteAlbum02,
        CassetteAlbum03,
        CassetteAlbum04,
        CassetteAlbum05,
        CassetteAlbum06,
        CassetteAlbum07,
        CassetteAlbum08,
        CassetteAlbum09,
        CassetteAlbum10,
        CassetteAlbum11,
        CassetteAlbum12,
        CassetteAlbum13,
        CassetteAlbum14,
        CassetteAlbum15,
        CassetteAlbum16,
        CassettePlaylist01,
        CassettePlaylist02,
        CassettePlaylist03,
        CassettePlaylist04,
        CassettePlaylist05,
        CassettePlaylist06,
        CassettePlaylist07,
        CassettePlaylist08,
        CassettePlaylist09,
        CassettePlaylist10,
        ExpertShovel,
        GoldenShovel,
        Lighter,
        ManualShunterBooklet,
        ManualSteamBooklet,
        RemoteController,
        Shovel,
        Stopwatch
    }

    public static class ItemTypeExtensions
    {
        public static VanillaAsset ToVanillaAsset(this ItemType itemType)
        {
            return (VanillaAsset)Enum.Parse(typeof(VanillaAsset), $"StoreItem{itemType}", true);
        }
    }
}
