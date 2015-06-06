using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    { 
        public WriteableBitmap bm;
         
        bool down1 = false, down2 = false, down3 = false;
        int xMove, yMove,
            xScale, yScale,
            xRotate;

        bool firstDot = true, secondDot = true, thirdDot = true;
        int xA = 0, yA = 0, xB = 0, yB = 0, xC = 0, yC = 0, xD = 0, yD = 0;
        int imgHeight, imgWidth;
        int radius1 = 3;

        private Dictionary<int, Action<int, int, int, int>> _algorithms = new Dictionary<int, Action<int, int, int, int>>();

        private void Swap<T>(ref T lhs, ref T rhs) { T temp; temp = lhs; lhs = rhs; rhs = temp; }

        public MainWindow()
        {
            InitializeComponent();
            initImage();
        }

        private void initImage()
        {
            imgHeight = (int)image.Height;
            imgWidth = (int)image.Width;
            //image = new Image();
            bm = new WriteableBitmap(imgWidth, imgHeight, 96, 96, PixelFormats.Bgra32, null);
            image.Source = bm;
        }

        private void drawPixel(int x1, int y1)
        {
            if (x1 < 1 || x1 > imgWidth - 2 || y1 < 1 || y1 > imgHeight - 2)
            {
                return;
            }
            byte blue = 0;
            byte green = 0;
            byte red = 0;
            byte alpha = 255;
            byte[] colorData = { blue, green, red, alpha };
            Int32Rect rect = new Int32Rect(x1, y1, 1, 1);
            bm.WritePixels(rect, colorData, 4, 0);
        }

        private void drawPixelRed(int x1, int y1)
        {
            if (x1 < 1 || x1 > imgWidth - 1 || y1 < 1 || y1 > imgHeight - 1)
            {
                return;
            }
            byte blue = 0;
            byte green = 0;
            byte red = 255;
            byte alpha = 255;
            byte[] colorData = { blue, green, red, alpha };
            Int32Rect rect = new Int32Rect(x1, y1, 1, 1);
            bm.WritePixels(rect, colorData, 4, 0);
        }
        
        private double curveCoordinate(double t, int A, int B, int C, int D)
        {
            return (double)A * Math.Pow((1 - t), 3)
                + (double)B * 3 * t * Math.Pow((1 - t), 2) 
                + (double)C * 3 * (1 - t) * Math.Pow(t, 2) 
                + (double)D * Math.Pow(t, 3);
        }

        private void bezierCurve()
        {
            for (double i = 0; i < 1; i += 0.001)
            {
                drawPixel((int)curveCoordinate(i, xA, xB, xC, xD), (int)curveCoordinate(i, yA, yB, yC, yD));
            }
        }

        private void circle(int x1, int y1)
        {
            int x = 0,
                y = radius1,
                delta = 1 - 2 * radius1,
                error = 0;
            while (y >= 0)
            {

                drawPixelRed(x1 + x, y1 + y);
                drawPixelRed(x1 + x, y1 - y);
                drawPixelRed(x1 - x, y1 + y);
                drawPixelRed(x1 - x, y1 - y);
                error = 2 * (delta + y) - 1;
                if ((delta < 0) && (error <= 0))
                {
                    delta += 2 * ++x + 1;
                    continue;
                }
                error = 2 * (delta - x) - 1;
                if ((delta > 0) && (error > 0))
                {
                    delta += 1 - 2 * --y;
                    continue;
                }
                x++;
                delta += 2 * (x - y);
                y--;
            }
        }

        private void image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition(image);
            if (firstDot)
            {
                image.Source = null;
                bm = new WriteableBitmap(imgWidth, imgHeight, 96, 96, PixelFormats.Bgra32, null);
                image.Source = bm;

                xA = (int)p.X;
                yA = (int)p.Y;
                firstDot = false;
            }
            else
                if (secondDot)
                {
                    xB = (int)p.X;
                    yB = (int)p.Y;
                    secondDot = false;
                }
                else
                    if (thirdDot)
                    {
                        xC = (int)p.X;
                        yC = (int)p.Y;
                        thirdDot = false;
                    }
                    else
                    {
                        xD = (int)p.X;
                        yD = (int)p.Y;
                        firstDot = true;
                        secondDot = true;
                        thirdDot = true;
                    }

            circle((int)p.X, (int)p.Y);
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            image.Source = null;
            bm = new WriteableBitmap(imgWidth, imgHeight, 96, 96, PixelFormats.Bgra32, null);
            image.Source = bm;
        }

        private void Clear()
        {
            image.Source = null;
            bm = new WriteableBitmap(imgWidth, imgHeight, 96, 96, PixelFormats.Bgra32, null);
            image.Source = bm;
        }
  
        private void buttonDrawCurve_Click(object sender, RoutedEventArgs e)
        {
            bezierCurve();
        }
        

        private void imgMoveCurve_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition(image);
            down1 = true;
            xMove = (int)p.X;
            yMove = (int)p.Y;
        }

        private void imgMoveCurve_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            down1 = false;
        }

        private void imgMoveCurve_MouseMove(object sender, MouseEventArgs e)
        {
            if (down1)
            {
                Clear();

                Point p = Mouse.GetPosition(image);
                int dx = -xMove + (int)p.X;
                int dy = -yMove + (int)p.Y;

                xA += dx;
                yA += dy;
                circle(xA, yA);

                xB += dx;
                yB += dy;
                circle(xB, yB);

                xC += dx;
                yC += dy;
                circle(xC, yC);

                xD += dx;
                yD += dy;
                circle(xD, yD);

                bezierCurve();

                xMove = (int)p.X;
                yMove = (int)p.Y;
            }
        }

        private void imgScaleCurve_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition(image);
            down2 = true;
            xScale = (int)p.X;
            yScale = (int)p.Y;
        }

        private void imgScaleCurve_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            down2 = false;
        }

        private void imgScaleCurve_MouseMove(object sender, MouseEventArgs e)
        {
            if (down2)
            {
                Clear();

                Point p = Mouse.GetPosition(image);
                int dx = -xScale + (int)p.X;
                int dy = yScale - (int)p.Y;

                xA = (int)((xA + 0.5) * (100 + dx) / 100);
                yA = (int)((yA + 0.5) * (100 + dy) / 100);
                circle(xA, yA);

                xB = (int)((xB + 0.5) * (100 + dx) / 100);
                yB = (int)((yB + 0.5) * (100 + dy) / 100);
                circle(xB, yB);

                xC = (int)((xC + 0.5) * (100 + dx) / 100);
                yC = (int)((yC + 0.5) * (100 + dy) / 100);
                circle(xC, yC);

                xD = (int)((xD + 0.5) * (100 + dx) / 100);
                yD = (int)((yD + 0.5) * (100 + dy) / 100);
                circle(xD, yD);

                bezierCurve();

                xScale = (int)p.X;
                yScale = (int)p.Y;
            }
        }


        private void imgRotateCurve_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition(image);
            down3 = true;
            xRotate = (int)p.X;
        }

        private void imgRotateCurve_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            down3 = false;
        }

        private int newCoordinateX(double d, double coordinateX, double coordinateY)
        {
            return (int)(Math.Cos(d / 5) * coordinateX - Math.Sin(d / 5) * coordinateY);
        }

        private int newCoordinateY(double d, double coordinateX, double coordinateY)
        {
            return (int)(Math.Sin(d / 5) * coordinateX + Math.Cos(d / 5) * coordinateY);
        }

        private void imgRotateCurve_MouseMove(object sender, MouseEventArgs e)
        {
            if (down3)
            {
                int tempX, tempY;
                Clear();

                Point p = Mouse.GetPosition(image);
                double dx = -xRotate + p.X;

                tempX = xA - imgWidth / 2;
                tempY = yA - imgHeight / 2;
                xA = newCoordinateX(dx, tempX, tempY) + imgWidth / 2;
                yA = newCoordinateY(dx, tempX, tempY) + imgHeight / 2;
                circle(xA, yA);

                tempX = xB - imgWidth / 2;
                tempY = yB - imgHeight / 2;
                xB = newCoordinateX(dx, tempX, tempY) + imgWidth / 2;
                yB = newCoordinateY(dx, tempX, tempY) + imgHeight / 2;
                circle(xB, yB);

                tempX = xC - imgWidth / 2;
                tempY = yC - imgHeight / 2;
                xC = newCoordinateX(dx, tempX, tempY) + imgWidth / 2;
                yC = newCoordinateY(dx, tempX, tempY) + imgHeight / 2;
                circle(xC, yC);

                tempX = xD - imgWidth / 2;
                tempY = yD - imgHeight / 2;
                xD = newCoordinateX(dx, tempX, tempY) + imgWidth / 2;
                yD = newCoordinateY(dx, tempX, tempY) + imgHeight / 2;
                circle(xD, yD);

                bezierCurve();

                xRotate = (int)p.X;
            }
        }

    }
}
