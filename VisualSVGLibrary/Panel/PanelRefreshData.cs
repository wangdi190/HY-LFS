using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using MyClassLibrary.DevShare;
using System.Windows.Media;
using WpfEarthLibrary;

namespace VisualSVGLibrary.Panel
{
    class PanelRefreshData : MyClassLibrary.MVVM.NotificationObject
    {
        public PanelRefreshData()
        {
            //tmr.Tick += new EventHandler(tmr_Tick);
            realLoads = new BindingList<ChartDataPoint>();
            planLoads = new BindingList<ChartDataPoint>();
            realEle = new BindingList<ChartDataPoint>();

            lstEvent = new System.ComponentModel.BindingList<EventData>();
            eventCount = new BindingList<WChartDataPoint>();

        }

        public BindingList<ChartDataPoint> realLoads { get; set; }//负荷曲线
        public BindingList<ChartDataPoint> planLoads { get; set; }//负荷曲线
        public BindingList<ChartDataPoint> realEle { get; set; }//用电量

        public double dHz { get; set; }//系统频率
        public double dLoad { get; set; }//当前负荷
        public double dEle { get; set; } //用电量

        private bool bdgx=false;
        public bool dgxIsChecked
        {
            get { return bdgx; }
            set { bdgx = value; RaisePropertyChanged(() => dgxIsChecked); } 
        }//等高线ToggleSwitch

        //事件
        public System.ComponentModel.BindingList<EventData> lstEvent { get; set; }
        public BindingList<WChartDataPoint> eventCount { get; set; }

        //System.Windows.Threading.DispatcherTimer tmr = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromSeconds(10) };
        Random rd = new Random();

        //void tmr_Tick(object sender, EventArgs e)
        //{
        //    readData();
        //}

        public void start()
        {
            //负荷曲线
            DateTime d = DateTime.Now;
            d = new DateTime(d.Year, d.Month, d.Day);
            for (int i = 0; i < 96; i++)
            {
                ChartDataPoint cp = new ChartDataPoint() { argudate = d.AddMinutes(15 * i), value = MyClassLibrary.MyFunction.simHourData((int)(i / 4)) * 200 + rd.Next(30) };
                if (i < 66)
                {
                    realLoads.Add(cp);
                    realEle.Add(cp);
                }

                planLoads.Add(new ChartDataPoint() { argudate = d.AddMinutes(15 * i), value = cp.value + rd.Next(20) });
            }

            RaisePropertyChanged(() => realLoads);
            RaisePropertyChanged(() => planLoads);
            RaisePropertyChanged(() => realEle);


            for (int i = 0; i < 5; i++ )
            {
                //isInclude 当isCheck来用
                eventCount.Add(new WChartDataPoint() { argu = ((EventData.EEventType)i).ToString(), value = 0, isInclude = false, brush=EventData.brushs[i]});//, color=(Color)Enum.ToObject(typeof(Color), i)});
            }

            readData();
            //tmr.Start();
        }

        //public void stop()
        //{
        //    tmr.Stop();
        //}

        public void readData()
        {
            DataGeneratorEvents();
        }


        private void DataGeneratorEvents()
        {
            foreach (pSymbolObject item in getAllObjListByLayerName("yx"))
            {
                if (rd.NextDouble()<0.1)
                {
                    if (lstEvent.Count > 15)
                    {
                        EventData edata = lstEvent.Last();
                        lstEvent.Remove(edata);
                        edata.obj.isShowSubObject = false;
                        
                    }

                    if (item.isShowSubObject == false)
                    {
                        int type = rd.Next(4);

                        lstEvent.Insert(0, new EventData() { obj = item, eObjID = item.id, startTime = DateTime.Now, eType = (EventData.EEventType)type, eTitle = item.name + ((EventData.EEventType)type).ToString() + "事件.", eContent = DateTime.Now.ToShortDateString() + ((EventData.EEventType)type).ToString() + ": 描述......" });
                    }
                }

                if (item.busiRunData == null)
                    item.busiRunData = new RunDataTfm(item);
                RunDataTfm rdata = item.busiRunData as RunDataTfm;

                rdata.name = item.id;
                rdata.p = rd.Next(200);
                rdata.q = rd.Next(150);
                rdata.rateOfLoad = rd.NextDouble();

                item.tooltipMoveTemplate = "TransformerInfoTemplate";
                item.tooltipMoveContent = item.busiRunData;
                rdata.refresh();
            }
            RaisePropertyChanged(() => lstEvent);

            for (int i=0; i<eventCount.Count; i++)
            {
                eventCount[i].value = (from e0 in lstEvent where e0.eType == (EventData.EEventType)i select e0).Count();
            }
            RaisePropertyChanged(() => eventCount);

            
            dHz = rd.Next(49,51);
            dLoad = rd.Next(10000,20000);
            dEle = rd.Next(40000,60000);

            RaisePropertyChanged(() => dHz); RaisePropertyChanged(() => dLoad); RaisePropertyChanged(() => dEle);
        }

        public IEnumerable<PowerBasicObject> getAllObjListByLayerName(string layername)
        {
            return from e0 in HySVG.uc.objManager.zLayers.Values
                   from e1 in e0.pModels.Values where e0.name==layername
                   select e1;
        }
    }

    ///<summary>事件数据</summary>
    public class EventData
    {

        public enum EEventType { 检修, 故障, 停电, 过载, 轻载 }
        public static Color[] colors = { Colors.Blue, Colors.Yellow, Colors.Green, Colors.Sienna, Colors.Thistle };
        public static Brush[] brushs = { Brushes.Blue, Brushes.Yellow, Brushes.Green, Brushes.Sienna, Brushes.Thistle };

        public pSymbolObject obj { set; get; }

        ///<summary>事件对象ID</summary>
        public string eObjID { get; set; }
        ///<summary>事件类型</summary>
        //public EEventType eType{get;set;}
        private EEventType etype;
        public EEventType eType
        {
            get { return etype; }
            set
            {
                color = colors[(int)value];
                brush = brushs[(int)value];
                etype = value;
            }
        }
        ///<summary>事件标题</summary>
        public string eTitle { get; set; }
        ///<summary>事件内容</summary>
        public string eContent { get; set; }
        ///<summary>事件开始时间</summary>
        public DateTime? startTime { get; set; }
        ///<summary>事件结束时间</summary>
        public DateTime? endTime { get; set; }

        public string strStartTime { get { return startTime == null ? "" : ((DateTime)startTime).ToString("dd HH:mm"); } }

        public Color color { get; set; }
        public Brush brush { get; set; }
    }

    public class WChartDataPoint : ChartDataPoint
    {
        public Brush brush { get; set; }
    }
}
