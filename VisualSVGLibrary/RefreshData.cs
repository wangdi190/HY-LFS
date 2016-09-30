using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfEarthLibrary;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows;

namespace VisualSVGLibrary
{
    class RefreshData
    {
        Thread t = null;
        public bool bQuit = false;


        public RefreshData()
        {
            if (t == null)
            {
                t = new Thread(() =>
                {
                    while (true)
                    {
                        if (bQuit == true)
                            break;

                        Update();
                        UpdateObjStatus();
                        //ShowObjStatus(HySVG.bShowObjStatus);

                        Thread.Sleep(10000);
                    }
                });
                t.IsBackground = true;
            }
            t.Start();
        }

        static Random rd = new Random();
        private void Update()
        {
            pLayer player = null;//层
            if (HySVG.uc.objManager.zLayers.TryGetValue("yx", out player))
            {
                foreach (pSymbolObject obj in player.pModels.Values)
                {
                    if (obj.busiRunData == null)
                        obj.busiRunData = new RunDataTfm(obj);
                    RunDataTfm rdata = obj.busiRunData as RunDataTfm;

                    rdata.name = obj.id;
                    rdata.p = rd.Next(200);
                    rdata.q = rd.Next(150);
                    rdata.rateOfLoad = rd.NextDouble();

                    obj.tooltipMoveTemplate = "TransformerInfoTemplate";
                    obj.tooltipMoveContent = obj.busiRunData;
                    rdata.refresh();
                }
            }

            if (HySVG.uc.objManager.zLayers.TryGetValue("yx", out player))
            {
                foreach (pSymbolObject obj in player.pModels.Values)
                {
                    //string str = obj.symbolid.Substring(obj.symbolid.Length - 4);
                    //if (str == "sta0")
                    //    obj.symbolid = obj.id2 + "#sta1";
                    //else
                    //    obj.symbolid = obj.id2 + "#sta0";

                    obj.aniTwinkle.doCount = 20;
                    obj.AnimationBegin(pSymbolObject.EAnimationType.闪烁);
                    obj.AnimationStop(pSymbolObject.EAnimationType.缩放);
                }
            }


        }


        public void UpdateObjStatus()
        {
            //IEnumerable<PowerBasicObject> allobj = HySVG.uc.objManager.getAllObjList();
            pLayer player = null;//层
            pSymbolObject.ECStatus status = pSymbolObject.ECStatus._正常;
            if (HySVG.uc.objManager.zLayers.TryGetValue("yx", out player))
            {
                foreach (pSymbolObject obj in player.pModels.Values)
                {
                    int nRand = rd.Next(10);
                    if (nRand < 3)
                        status = pSymbolObject.ECStatus._正常;
                    else if (nRand < 5)
                        status = pSymbolObject.ECStatus.检修;
                    else if (nRand < 6)
                        status = pSymbolObject.ECStatus.故障;
                    else if (nRand < 7)
                        status = pSymbolObject.ECStatus.停电;
                    else if (nRand < 8)
                        status = pSymbolObject.ECStatus.过载;
                    else if (nRand < 9)
                        status = pSymbolObject.ECStatus.轻载;

                    obj.objStatus = status;
                }
            }

            //pSymbolObject.ECStatus status = pSymbolObject.ECStatus._正常;
            //foreach (PowerBasicObject tmpobj in allobj)
            //{
            //    if (tmpobj is pSymbolObject)
            //    {
            //        int nRand = rd.Next(10);
            //        if (nRand < 3)
            //            status = pSymbolObject.ECStatus._正常;
            //        else if (nRand < 5)
            //            status = pSymbolObject.ECStatus.检修;
            //        else if (nRand < 6)
            //            status = pSymbolObject.ECStatus.故障;
            //        else if (nRand < 7)
            //            status = pSymbolObject.ECStatus.停电;
            //        else if (nRand < 8)
            //            status = pSymbolObject.ECStatus.过载;
            //        else if (nRand < 9)
            //            status = pSymbolObject.ECStatus.轻载;

            //        (tmpobj as pSymbolObject).objStatus = status;
            //    }
            //}
        }

        //Random rd = new Random();

        static pLayer contourLayer;
        static ContourGraph.Contour con;
        static List<ContourGraph.ValueDot> dots;
        static pContour gcon;
        public static void showVLContour(bool isShow)
        {
            if (isShow)
            {
                if (!HySVG.uc.legendManager.legends.ContainsKey("电压等值图例"))
                {
                    GradientBrushLegend legend = HySVG.uc.legendManager.createGradientBrushLegend("电压等值图例");
                    legend.header = " 电压标幺值 ";
                    legend.isShow = true;
                    legend.panel.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                    legend.panel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    legend.panel.Margin = new Thickness(120, 0, 0, 30);
                    legend.panel.Background = new SolidColorBrush(Color.FromArgb(0xCC, 0x00, 0x00, 0x00));
                    legend.headerBackground = Brushes.Black;
                    legend.headerForeground = Brushes.Aqua;
                    legend.headerBorderBrush = new SolidColorBrush(Colors.White);
                    legend.setText("0.9", "1", "1.1");

                    GradientStopCollection gsc = new GradientStopCollection();
                    gsc.Add(new GradientStop(Colors.Blue, 0));
                    gsc.Add(new GradientStop(Color.FromArgb(0x34, 0x00, 0xFF, 0xFF), 0.25));
                    gsc.Add(new GradientStop(Color.FromArgb(0x00, 0x00, 0xFF, 0x00), 0.5));
                    gsc.Add(new GradientStop(Color.FromArgb(0x34, 0xFF, 0xFF, 0x00), 0.75));
                    gsc.Add(new GradientStop(Colors.Red, 1));
                    legend.setGradientStop(gsc);
                }
                HySVG.uc.legendManager.legends["电压等值图例"].isShow = true;

                if (!HySVG.uc.objManager.zLayers.TryGetValue("等高图层", out contourLayer))
                {
                    contourLayer = HySVG.uc.objManager.AddLayer("等高图层", "等高图层", "等高图层");
                    //contourLayer.deepOrder = -1;

                    dots = new List<ContourGraph.ValueDot>();

                    pLayer layer;
                    if (HySVG.uc.objManager.zLayers.TryGetValue("yx", out layer))
                    {
                        foreach (pSymbolObject obj in layer.pModels.Values)
                        {
                            dots.Add(new ContourGraph.ValueDot() { id = obj.id, location = obj.center, value = rd.Next(90,110)/(double)100.0 });
                        }

                        double minx, miny, maxx, maxy;
                        //根据scene.coordinateManager.isXAsLong确定原始坐标的横纵方向
                        miny = dots.Min(p => HySVG.uc.coordinateManager.isXAsLong ? p.location.Y : p.location.X);
                        maxy = dots.Max(p => HySVG.uc.coordinateManager.isXAsLong ? p.location.Y : p.location.X);
                        minx = dots.Min(p => HySVG.uc.coordinateManager.isXAsLong ? p.location.X : p.location.Y);
                        maxx = dots.Max(p => HySVG.uc.coordinateManager.isXAsLong ? p.location.X : p.location.Y);

                        double w = maxx - minx; double h = maxy - miny;
                        minx = minx - w * 0.2; maxx = maxx + w * 0.2;
                        miny = miny - h * 0.2; maxy = maxy + h * 0.2;
                        w = maxx - minx; h = maxy - miny;
                        //经纬换为屏幕坐标
                        int size = 1024;
                        foreach (ContourGraph.ValueDot dot in dots)
                        {
                            if (HySVG.uc.coordinateManager.isXAsLong)
                                dot.location = new Point((dot.location.X - minx) / w * size, (maxy - dot.location.Y) / h * size);
                            else
                                dot.location = new Point((dot.location.Y - minx) / w * size, (maxy - dot.location.X) / h * size);  //重新赋与新的平面点位置, 注，纬度取反，仅适用北半球
                        }


                        //设置计算参数
                        if (con == null) con = new ContourGraph.Contour();
                        con.dots = dots;
                        con.opacityType = ContourGraph.Contour.EOpacityType.倒梯形;
                        con.canvSize = new Size(size, size);
                        con.gridXCount = 200;
                        con.gridYCount = 200;
                        con.Span = 30;
                        con.maxvalue = 1.1;
                        con.minvalue = 0.9;
                        con.dataFillValue = 1;
                        con.dataFillMode = ContourGraph.Contour.EFillMode.单点包络填充;
                        con.isDrawGrid = false;
                        con.isDrawLine = false;
                        con.isFillLine = true;
                        if (gcon == null) gcon = new pContour(contourLayer) { id = "等值图" };// { minJD = minx, maxJD = maxx, minWD = miny, maxWD = maxy };
                        gcon.setRange(minx, maxx, miny, maxy);
                        gcon.brush = con.ContourBrush;
                        contourLayer.AddObject("等值线", gcon);
                        con.GenCompleted += new EventHandler(con_GenCompleted);
                        con.GenContourAsync(); //异步开始生成

                    }
                }
                else//刷新数据
                {
                    foreach (var item in dots)
                    {
                        if (item.id != null)
                        {
                            item.value = rd.Next(90, 110) / (double)100.0;
                        }
                    }
                    con.ReGenContourAsync();
                }
                if (HySVG.uc.objManager.zLayers.TryGetValue("等高图层", out contourLayer))
                    contourLayer.logicVisibility = true;

            }
            else
            {
                if (HySVG.uc.objManager.zLayers.TryGetValue("等高图层", out contourLayer))
                    contourLayer.logicVisibility = false;
                if (HySVG.uc.legendManager.legends.ContainsKey("电压等值图例"))
                    HySVG.uc.legendManager.legends["电压等值图例"].isShow = false;

                HySVG.uc.UpdateModel();
            }
            
        }

        static void con_GenCompleted(object sender, EventArgs e) //异步完成
        {
            gcon.brush = con.ContourBrush;
            HySVG.uc.UpdateModel();

        }

        //public static void hideVLContour()
        //{

        //}

    }
}
