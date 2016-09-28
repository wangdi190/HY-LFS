using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfEarthLibrary;
using System.Windows.Media;

namespace VisualSVGLibrary
{
    class RunData
    {
    }

    public abstract class RunDataBase : MyClassLibrary.MVVM.NotificationObject
    {
        public RunDataBase(PowerBasicObject Parent)
        {
            parent = Parent;
        }

        internal PowerBasicObject parent;
    }

    public class RunDataPowerLine : RunDataBase
    {
        public RunDataPowerLine(PowerBasicObject Parent)
            : base(Parent)
        {
        }

        public string name { get; set; }
        public double p { get; set; }
        public double q { get; set; }
        public double rateOfLoad { get; set; }

        public string pInfo { get { return string.Format("{0:f3}MW", p); } }
        public string qInfo { get { return string.Format("{0:f3}MVar", q); } }
        public string rateOfLoadInfo { get { return string.Format("{0:p1}", rateOfLoad); } }

        public Color rateOfLoadColor1 { get { return rateOfLoad > 0.8 ? Color.FromRgb(0xFF, 0x8A, 0x8A) : (rateOfLoad < 0.2 ? Color.FromRgb(0x81, 0xFF, 0x81) : Color.FromRgb(0x8E, 0x8E, 0xFF)); } }
        public Color rateOfLoadColor2 { get { return rateOfLoad > 0.8 ? Color.FromRgb(0xFF, 0x60, 0x60) : (rateOfLoad < 0.2 ? Color.FromRgb(0x00, 0xB8, 0x00) : Color.FromRgb(0x4F, 0x4F, 0xFF)); } }
        public Color rateOfLoadColor3 { get { return rateOfLoad > 0.8 ? Color.FromRgb(0x99, 0x00, 0x00) : (rateOfLoad < 0.2 ? Color.FromRgb(0x00, 0x42, 0x00) : Color.FromRgb(0x00, 0x00, 0x66)); } }

        public void refresh()
        {
            RaisePropertyChanged(() => pInfo);
            RaisePropertyChanged(() => qInfo);
            RaisePropertyChanged(() => rateOfLoad);
            RaisePropertyChanged(() => rateOfLoadInfo);
            RaisePropertyChanged(() => rateOfLoadColor1);
            RaisePropertyChanged(() => rateOfLoadColor2);
            RaisePropertyChanged(() => rateOfLoadColor3);
        }
    }

    public class RunDataTfm : RunDataBase
    {
        public RunDataTfm(PowerBasicObject Parent)
            : base(Parent)
        {
        }

        public string name { get; set; }
        public double p { get; set; }
        public double q { get; set; }
        public double rateOfLoad { get; set; }

        public string pInfo { get { return string.Format("{0:f3}MW", p); } }
        public string qInfo { get { return string.Format("{0:f3}MVar", q); } }
        public string rateOfLoadInfo { get { return string.Format("{0:p1}", rateOfLoad); } }

        public Color rateOfLoadColor1 { get { return rateOfLoad > 0.8 ? Color.FromRgb(0xFF, 0x8A, 0x8A) : (rateOfLoad < 0.2 ? Color.FromRgb(0x81, 0xFF, 0x81) : Color.FromRgb(0x8E, 0x8E, 0xFF)); } }
        public Color rateOfLoadColor2 { get { return rateOfLoad > 0.8 ? Color.FromRgb(0xFF, 0x60, 0x60) : (rateOfLoad < 0.2 ? Color.FromRgb(0x00, 0xB8, 0x00) : Color.FromRgb(0x4F, 0x4F, 0xFF)); } }
        public Color rateOfLoadColor3 { get { return rateOfLoad > 0.8 ? Color.FromRgb(0x99, 0x00, 0x00) : (rateOfLoad < 0.2 ? Color.FromRgb(0x00, 0x42, 0x00) : Color.FromRgb(0x00, 0x00, 0x66)); } }

        public void refresh()
        {
            RaisePropertyChanged(() => pInfo);
            RaisePropertyChanged(() => qInfo);
            RaisePropertyChanged(() => rateOfLoad);
            RaisePropertyChanged(() => rateOfLoadInfo);
            RaisePropertyChanged(() => rateOfLoadColor1);
            RaisePropertyChanged(() => rateOfLoadColor2);
            RaisePropertyChanged(() => rateOfLoadColor3);
        }
    }

    
    
}
