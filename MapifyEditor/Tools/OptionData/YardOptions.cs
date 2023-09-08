using System;
using UnityEngine;

namespace Mapify.Editor.Tools.OptionData
{
    [Serializable]
    public class YardOptions
    {
        private int _tracksMainSide = 1;
        private int _tracksOtherSide = 1;
        private bool _half = false;
        private bool _alternateSides = true;
        private float _minimumLength = 24.0f;
        private string _stationId = "";
        private char _yardId = 'B';
        private byte _startTrackId = 1;
        private bool _reverseNumbers = false;

        public int TracksMainSide { get => _tracksMainSide; set => _tracksMainSide = Mathf.Max(value, 1); }
        public int TracksOtherSide { get => _tracksOtherSide; set => _tracksOtherSide = Mathf.Max(value, 0); }
        public bool Half { get => _half; set => _half = value; }
        public bool AlternateSides { get => _alternateSides; set => _alternateSides = value; }
        public float MinimumLength { get => _minimumLength; set => _minimumLength = Mathf.Max(value, 0.0f); }
        public string StationId { get => _stationId; set => _stationId = value; }
        public char YardId { get => _yardId; set => _yardId = value; }
        public byte StartTrackId { get => _startTrackId; set => _startTrackId = (byte)Mathf.Clamp(value, 1, 99); }
        public bool ReverseNumbers { get => _reverseNumbers; set => _reverseNumbers = value; }

        public static YardOptions DefaultOptions { get; private set; } = new YardOptions();

        public YardOptions(int tracksMainSide = 1, int tracksOtherSide = 1, bool half = false, bool alternateSides = true, float minimumLength = 24.0f,
            string stationId = "", char yardId = 'B', byte startTrackId = 1, bool reverseNumbers = false)
        {
            TracksMainSide = tracksMainSide;
            TracksOtherSide = tracksOtherSide;
            Half = half;
            AlternateSides = alternateSides;
            MinimumLength = minimumLength;
            StationId = stationId;
            YardId = yardId;
            StartTrackId = startTrackId;
            ReverseNumbers = reverseNumbers;    
        }
    }
}
