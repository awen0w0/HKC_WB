using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhiteBalanceCorrection
{
    public interface IDisplayColorAnalyzer
    {
        bool ConnectDevice(int ch, ref string errorMsg);
        bool ZeroCal();
        void SetDisplayModel(int model);
        void Disconnect();
        ProbeStruct Measure();
        ProbeStruct MeasureEx(double tolerance = 0.0001, int maxMeasureCounts = 60);
    }
}
