using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Text.RegularExpressions;
using WpfEarthLibrary;
using System.Windows;
using System.Xml;
using System.Windows.Media.Imaging;

namespace VisualSVGLibrary
{
    class PhraseSVG
    {
        //Earth uc;

        public PhraseSVG()//Earth earth)
        {
            //uc = earth;
        }

        string symbolid, stroke, stroke_width, data, fill, layer, id, symbol, transform, classname;
        int depth;
        Rect symbolviewbox;
        double x, y, w, h, angle;
        pSymbol psymbol = null;
        pLayer player = null, player_Text, player_Other;
        public RefreshData reData = null;

        private void StartRefresh()
        {
            if (reData == null)
            {
                reData = new RefreshData();
                return;
            }

            reData.bQuit = true;
            reData = null;
            reData = new RefreshData();
        }


        public void VisualFileSvg(string svgPath)
        {
            //切换svg 清理
            for (int i = 0; i < HySVG.uc.objManager.zLayers.Count; i++ )
            {
                HySVG.uc.objManager.zLayers.ElementAt(i).Value.pModels.Clear();
            }
            HySVG.uc.objManager.zLayers.Clear();
            HySVG.uc.UpdateModel();

            player_Other = HySVG.uc.objManager.AddLayer("Other", "Other", "Other");//如果无class就将其放入Other层
            player_Text = HySVG.uc.objManager.AddLayer("Text", "Text", "Text");
            HySVG.uc.objManager.AddLayer("NoticeBoard", "NoticeBoard", "NoticeBoard");//提示牌 检修 故障等

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Document;
            settings.IgnoreWhitespace = false;
            settings.IgnoreComments = true;
            settings.DtdProcessing = DtdProcessing.Ignore;

            XmlReader reader = XmlTextReader.Create(svgPath, settings);
            while (reader.Read())
            {
                string elename = reader.Name;
                switch (elename)
                {
                    case "svg":
                        double w = Double.Parse(getPropertyValue(reader, "width"));
                        double h = Double.Parse(getPropertyValue(reader, "height"));
                        HySVG.uc.earthManager.planeViewBox = new Rect(0, 0, w, h);
                        break;
                    case "g":
                        depth = reader.Depth;
                        //layer = getPropertyValue(reader, "id");
                        //player = uc.objManager.AddLayer(layer, layer, layer);
                        while (reader.Read())
                        {
                            if (reader.Depth == depth && reader.NodeType == XmlNodeType.EndElement)
                                break;

                            switch (reader.Name)
                            {
                                case "use":
                                    if (reader.HasAttributes)
                                    {
                                        classname = getPropertyValue(reader, "class");
                                        player = GetLayerFromName(classname);

                                        id = getPropertyValue(reader, "id");
                                        symbol = getPropertyValue(reader, "xlink:href");
                                        x = double.Parse(getPropertyValue(reader, "x"));
                                        y = double.Parse(getPropertyValue(reader, "y"));
                                        transform = getPropertyValue(reader, "transform");
                                        HySVG.uc.objManager.zSymbols.TryGetValue(symbol, out psymbol);
                                        Rect rt = TransformToXY(transform, x, y, psymbol.sizeX, psymbol.sizeY, out angle);
                                        angle = Math.PI / 180 * (360-angle);
                                        string icnname = getPropertyValue(reader, "icnname");

                                        pSymbolObject obj = new pSymbolObject(player)
                                        {
                                            id = id,
                                            name=id,
                                            //axis = new VECTOR3D(0, 0, 1),
                                            planeLocation = new Point(rt.X+rt.Width/2, rt.Y+rt.Height/2).ToString(),
                                            symbolid = symbol,
                                            scaleX = (float)(0.0005 * rt.Width),  //  0.0005为从svg平面空间到3D空间的缩放系数，可自行调整大小
                                            scaleY = (float)(0.0005 * rt.Height),
                                            color = Colors.BlueViolet,
                                            isH = true,
                                            angle = (float)angle,
                                            id2 = icnname,
                                            isUseColor=false,
                                        };
                                        //if (classname == "Transformer2")
                                        //{
                                        //    if (uc.objManager.zSymbols.TryGetValue("检修", out psymbol))
                                        //    {
                                        //        pSymbolObject obj11 = new pSymbolObject(player)
                                        //        {
                                        //            id = Helpler.getGUID(),
                                        //            planeLocation = new Point(rt.X + rt.Width / 2, rt.Y + rt.Height / 2).ToString(),
                                        //            isH = true,
                                        //            brush = psymbol.brush,
                                        //            scaleX = (float)(0.0005 * rt.Width),  //  0.0005为从svg平面空间到3D空间的缩放系数，可自行调整大小
                                        //            scaleY = (float)(0.0005 * rt.Height),
                                        //            //color = Colors.Red,
                                        //            isUseColor = false,
                                        //        };
                                        //        player.AddObject(id, obj11);
                                        //    }
                                        //}
                                        //else
                                            player.AddObject(id, obj);
                                    }
                                    break;

                                case "line":
                                    if (reader.HasAttributes)
                                    {
                                        classname = getPropertyValue(reader, "class");
                                        player = GetLayerFromName(classname);

                                        id = getPropertyValue(reader, "id");
                                        
                                        stroke = getPropertyValue(reader, "stroke");
                                        stroke_width = getPropertyValue(reader, "stroke-width");
                                        string strPoint = getPropertyValue(reader, "x1")+","+getPropertyValue(reader, "y1")+" "+getPropertyValue(reader, "x2")+","+getPropertyValue(reader, "y2");

                                        pPowerLine obj = new pPowerLine(player)
                                        {
                                            id = id,
                                            name = id,
                                            planeStrPoints = strPoint,
                                            color = SvgColorToColor(stroke),
                                            isFlow = false, //是否显示潮流
                                            thickness = 0.002f, //线宽
                                            arrowSize = 0.005f,  //潮流箭头大小
                                            //isInverse = (dir == "0" ? false : true)
                                        };
                                        player.AddObject(id, obj);

                                    }
                                    break;

                                case "polyline":
                                    if (reader.HasAttributes)
                                    {
                                        classname = getPropertyValue(reader, "class");
                                        player = GetLayerFromName(classname);

                                        id = getPropertyValue(reader, "id");
                                        stroke = getPropertyValue(reader, "stroke");
                                        stroke_width = getPropertyValue(reader, "stroke-width");
                                        data = getPropertyValue(reader, "points");

                                        //PointCollection pc = new PointCollection();
                                        PointCollection pc = PointCollection.Parse(data);
                                        IEnumerable<Point> newpc = pc.Distinct();

                                        if (newpc.Count() <= 1)
                                            break;


                                        string str = string.Join(" ", newpc);

                                        pPowerLine obj = new pPowerLine(player)
                                        {
                                            id = id,
                                            name = id,
                                            planeStrPoints = str,
                                            color = SvgColorToColor(stroke),
                                            isFlow = false, //是否显示潮流
                                            thickness = 0.002f, //线宽
                                            arrowSize = 0.005f,  //潮流箭头大小
                                            
                                            //isInverse = (dir == "0" ? false : true)
                                        };
                                        player.AddObject(id, obj);
                                    }
                                    break;

                                case "text":
                                    if (reader.HasAttributes)
                                    {
                                        id = getPropertyValue(reader, "id");
                                        x = double.Parse(getPropertyValue(reader, "x"));
                                        y = double.Parse(getPropertyValue(reader, "y"));
                                        string fontfamily = getPropertyValue(reader, "font-family");
                                        string fontsize = getPropertyValue(reader, "font-size");
                                        fill = getPropertyValue(reader, "fill");
                                        reader.Read();
                                        string text = reader.Value;

                                        float scalexy = float.Parse(fontsize) / 16f * 0.7f;
                                        w = text.Length * 9;
                                        h = 17;
                                        pText obj = new pText(player_Text)
                                        {
                                            id = id,
                                            name = text,
                                            text = text,
                                            planeLocation = new Point(x + w / 2, y + h / 2).ToString(),
                                            isH = true, //是否水平放置
                                            color = Colors.White,//SvgColorToColor(fill),
                                            scaleX = scalexy,
                                            scaleY = scalexy,
                                        };
                                        player_Text.AddObject(id, obj);
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }

            //==============清理空白层
            //int idxi = 0;
            //while (idxi < uc.objManager.zLayers.Count)
            //{
            //    if (uc.objManager.zLayers.Values.ElementAt(idxi).pModels.Count == 0)
            //    {
            //        uc.objManager.zLayers.Remove(uc.objManager.zLayers.Keys.ElementAt(idxi));
            //    }
            //    else
            //        idxi++;
            //}


            HySVG.uc.UpdateModel();
            StartRefresh();
        }

        pLayer GetLayerFromName(string classname)
        {
            if (classname == "")
                return player_Other;

            if (HySVG.uc.objManager.zLayers.TryGetValue(classname, out player) == false)
                player = HySVG.uc.objManager.AddLayer(classname, classname, classname);

            return player;
        }

        Rect TransformToXY(string transform, double x, double y, double w, double h, out double angle)
        {
            
            //transform="rotate(270 742.0 782.5) scale(0.178,0.183) translate(3426,3493)"
            Regex regexR = new Regex("rotate\\(\\d* \\d+(\\.\\d+) \\d+(\\.\\d+)\\) scale\\(\\d+(\\.\\d+),\\d+(\\.\\d+)\\) translate\\(\\d+(\\.\\d+),\\d+(\\.\\d+)\\)");

            //transform="rotate(0) scale(0.178,0.204) translate(3403,3396)"
            Regex regex = new Regex("rotate\\(\\d*\\) scale\\(\\d+(\\.\\d+),\\d+(\\.\\d+)\\) translate\\(\\d+(\\.\\d+),\\d+(\\.\\d+)\\)");

            angle = 0;
            if (regexR.Match(transform).Success)
            {
                string[] sArr = transform.Split('(', ')', ',', ' ');
                double rotateAng = Convert.ToDouble(sArr[1]);
                double rotateX = Convert.ToDouble(sArr[2]);
                double rotateY = Convert.ToDouble(sArr[3]);
                double scaleX = Convert.ToDouble(sArr[6]);
                double scaleY = Convert.ToDouble(sArr[7]);
                double transX = Convert.ToDouble(sArr[10]);
                double transY = Convert.ToDouble(sArr[11]);

                angle = rotateAng;
                return new Rect(x * scaleX + transX * scaleX, y*scaleY+transY*scaleY, scaleX*w, scaleY*h);

            }

            if (regex.Match(transform).Success)
            {
                string[] sArr = transform.Split('(', ')', ',', ' ');

                double rotateAng = Convert.ToDouble(sArr[1]);
                double scaleX = Convert.ToDouble(sArr[4]);
                double scaleY = Convert.ToDouble(sArr[5]);
                double transX = Convert.ToDouble(sArr[8]);
                double transY = Convert.ToDouble(sArr[9]);

                angle = rotateAng;
                return new Rect(x * scaleX + transX * scaleX, y * scaleY + transY * scaleY, scaleX * w, scaleY * h);
            }

            return new Rect(0, 0, 0, 0);
        }

        public void InitGzSymbol()
        {
            //psymbol = new pSymbol() 
            //{
            //    id = "检修", 
            //    sizeX = 32, 
            //    sizeY = 32,
            //    brush = new ImageBrush(new BitmapImage(new Uri(".\\ico\\检修.png", UriKind.RelativeOrAbsolute))),//".\\ico\\检修.ico"),
            //    //imagefile = ".\\ico\\检修.png",
            //};

            //uc.objManager.AddSymbol("检修", "检修", ".\\ico\\检修.ico");
            //psymbol.imagefile
            //uc.objManager.zSymbols.Add(psymbol.id, psymbol);
            //E:\wangdi\来福士\VisualSVGTest\VisualSVGLibrary\bin\Debug\ico
        }

        public void PhraseSymbol(string svgSymbolPath, string svgSymbolName)
        {
            
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Document;
            settings.IgnoreWhitespace = false;
            settings.IgnoreComments = true;
            settings.DtdProcessing = DtdProcessing.Ignore;
            
            DrawingGroup drawgroup = new DrawingGroup();
            GeometryDrawing aDrawing;
            

            XmlReader reader = XmlTextReader.Create(svgSymbolPath, settings);
            while (reader.Read())
            {
                string elename = reader.Name;
                switch (elename)
                {
                    case "svg":
                        if (reader.HasAttributes && reader.NodeType == XmlNodeType.Element)
                        {
                            w = Double.Parse(getPropertyValue(reader, "width"));
                            h = Double.Parse(getPropertyValue(reader, "height"));
                            symbolviewbox = new Rect(0, 0, w, h);
                        }
                       break;
                    case "g":
                       depth = reader.Depth;
                       drawgroup = new DrawingGroup();
                       symbolid = getPropertyValue(reader, "id");
                       symbolid = svgSymbolName + "#" + symbolid;
                       psymbol = new pSymbol() { id = symbolid, sizeX = symbolviewbox.Width, sizeY = symbolviewbox.Height };
                       HySVG.uc.objManager.zSymbols.Add(symbolid, psymbol);
                       
                       while (reader.Read())
                       {
                           if (reader.Depth == depth && reader.NodeType == XmlNodeType.EndElement)
                           {
                               DrawingBrush myDrawingBrush = new DrawingBrush();
                               myDrawingBrush.Drawing = drawgroup;
                               psymbol.brush = myDrawingBrush;
                               break;
                           }

                           switch(reader.Name)
                           {
                               case "line":
                                   if (reader.HasAttributes)
                                   {
                                       stroke = getPropertyValue(reader, "stroke");
                                       stroke_width = getPropertyValue(reader, "stroke-width");
                                       data = string.Format("M {0} {1} L {2} {3}", getPropertyValue(reader, "x1"), getPropertyValue(reader, "y1"), getPropertyValue(reader, "x2"), getPropertyValue(reader, "y2"));

                                       Geometry geo = PathGeometry.Parse(data);
                                       aDrawing = new GeometryDrawing();
                                       aDrawing.Geometry = geo;

                                       aDrawing.Pen = new Pen() { Thickness = Double.Parse(stroke_width), Brush = SvgColorToBrush(stroke) };
                                       //aDrawing.Brush = SvgColorToBrush(stroke);
                                       drawgroup.Children.Add(aDrawing);
                                   }
                                   break;

                                case "circle":
                                   if (reader.HasAttributes)
                                   {
                                       stroke = getPropertyValue(reader, "stroke");
                                       stroke_width = getPropertyValue(reader, "stroke-width");
                                       fill = getPropertyValue(reader, "fill");
                                       string cx = getPropertyValue(reader, "cx");
                                       string cy = getPropertyValue(reader, "cy");
                                       double r = Double.Parse(getPropertyValue(reader, "r"));

                                       Point center = Point.Parse(cx + "," + cy);

                                       EllipseGeometry geo = new EllipseGeometry(center, r, r);
                                       aDrawing = new GeometryDrawing();
                                       aDrawing.Geometry = geo;

                                       
                                       aDrawing.Pen = new Pen() { Thickness = Double.Parse(stroke_width), Brush = SvgColorToBrush(stroke) };
                                       aDrawing.Brush = SvgColorToBrush(fill);
                                       drawgroup.Children.Add(aDrawing);
                                   }
                                   break;
                                case "path":
                                   if (reader.HasAttributes)
                                   {
                                       stroke = getPropertyValue(reader, "stroke");
                                       stroke_width = getPropertyValue(reader, "stroke-width");
                                       data = getPropertyValue(reader, "d");

                                       Geometry geo = PathGeometry.Parse(data);

                                       aDrawing = new GeometryDrawing();
                                       aDrawing.Geometry = geo;

                                       aDrawing.Pen = new Pen() { Thickness = Double.Parse(stroke_width), Brush = SvgColorToBrush(stroke) };
                                       //aDrawing.Brush = SvgColorToBrush(stroke);

                                       
                                       //if (svgSymbolName == "1.dr.icn.svg")
                                       //{
                                       //    //TransformGroup transGroup = new TransformGroup();
                                       //    //transGroup.Children.Add(new RotateTransform(270, 43, 61));
                                       //    //geo.Transform = transGroup;

                                       //    try
                                       //    {
                                       //        //(geo as PathGeometry).Transform = new RotateTransform(270, 43, 61);

                                       //        TransformGroup transGroup = new TransformGroup();
                                       //        transGroup.Children.Add(new RotateTransform(270, 43, 61));
                                       //        geo.Transform = transGroup;
                                               

                                       //    }
                                       //    catch (System.Exception ex)
                                       //    {
                                       //        int a = 0;
                                       //    }

                                           
                                       //}


                                       drawgroup.Children.Add(aDrawing);
                                   }
                                   break;

                                case "polygon":
                                   if (reader.HasAttributes)
                                   {
                                       stroke = getPropertyValue(reader, "stroke");
                                       stroke_width = getPropertyValue(reader, "stroke-width");
                                       fill = getPropertyValue(reader, "fill");
                                       data = getPropertyValue(reader, "points");

                                       IEnumerable<Point> pc = PointCollection.Parse(data).Distinct();
                                       if (pc.Count() <= 1) break;

                                       data = "";
                                       for (int i=0; i<pc.Count(); i++)
                                       {
                                           string strTemp;
                                           if(i == 0)
                                               strTemp = "M "+ pc.ElementAt(i).ToString();
                                           else
                                               strTemp = "L "+ pc.ElementAt(i).ToString();
                                           data += strTemp;
                                       }


                                       Geometry geo = PathGeometry.Parse(data);
                                       aDrawing = new GeometryDrawing();
                                       aDrawing.Geometry = geo;

                                       aDrawing.Pen = new Pen() { Thickness = Double.Parse(stroke_width), Brush = SvgColorToBrush(stroke) };
                                       aDrawing.Brush = SvgColorToBrush(fill);
                                       drawgroup.Children.Add(aDrawing);
                                   }
                                   break;

                               case "rect":
                                   if (reader.HasAttributes)
                                   {
                                       stroke = getPropertyValue(reader, "stroke");
                                       stroke_width = getPropertyValue(reader, "stroke-width");
                                       x = Double.Parse(getPropertyValue(reader, "x"));
                                       y = Double.Parse(getPropertyValue(reader, "y"));
                                       w = Double.Parse(getPropertyValue(reader, "width"));
                                       h = Double.Parse(getPropertyValue(reader, "height"));

                                       RectangleGeometry geo = new RectangleGeometry();
                                       geo.Rect = new Rect(x, y, w, h);
                                       aDrawing = new GeometryDrawing();
                                       aDrawing.Geometry = geo;

                                       aDrawing.Pen = new Pen() { Thickness = Double.Parse(stroke_width), Brush = SvgColorToBrush(stroke) };
                                       //aDrawing.Brush = SvgColorToBrush(fill);
                                       drawgroup.Children.Add(aDrawing);
                                   }
                                   break;
                           }
                       }
                       break;
                }
            }
        }

        string getPropertyValue(XmlReader reader, string PropertyName)
        {
            if (reader.MoveToAttribute(PropertyName))
                return reader.Value;
            else
                return "";
        }

        Brush SvgColorToBrush(string color)
        {
            Brush brush = null;
            Regex regex = new Regex("rgb\\(\\d*,\\d*,\\d*\\)");
            Match m = regex.Match(color);
            if (m.Success)
            {
                string[] szTmp = color.Split('(', ')', ',');
                byte r = Convert.ToByte(szTmp[1]);
                byte g = Convert.ToByte(szTmp[2]);
                byte b = Convert.ToByte(szTmp[3]);
                brush = new SolidColorBrush(Color.FromRgb(r, g, b));

            }
            else if (color == "yellow")
                brush = Brushes.Yellow;
            else if (color == "")
                brush = Brushes.White;
            return brush;
        }

        Color SvgColorToColor(string color)
        {
            Regex regex = new Regex("rgb\\(\\d*,\\d*,\\d*\\)");
            Match m = regex.Match(color);
            if (m.Success)
            {
                string[] szTmp = color.Split('(', ')', ',');
                byte r = Convert.ToByte(szTmp[1]);
                byte g = Convert.ToByte(szTmp[2]);
                byte b = Convert.ToByte(szTmp[3]);
                return Color.FromRgb(r, g, b);
            }
            return Colors.White;
        }
    }
}
