using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Services
{
    public class NetworkSpeeder
    {
        DateTime _LastUpdateTime = DateTime.Now;
        double _TimeDiff = 0d;
        double _SizeDiff = 0d;
        double _LastUpdateDownloadedSize = 0d;
        double _Speed = 0d;

        double _MaxSpeed = 0d;
        public double MaxSpeed => _MaxSpeed;


        public double CalculateSpeed(double downloaded)
        {
            DateTime now = DateTime.Now;
            TimeSpan interval = now - _LastUpdateTime;
            _TimeDiff = interval.TotalSeconds;
            _SizeDiff = downloaded - _LastUpdateDownloadedSize;
            _Speed = Math.Floor((double)(_SizeDiff) / _TimeDiff);
            _LastUpdateDownloadedSize = downloaded;
            _LastUpdateTime = now;

            if (_Speed > _MaxSpeed)
                _MaxSpeed = _Speed;

            return _Speed;
        }

    }
}
