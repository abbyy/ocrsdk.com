using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace Sample
{
    public class DrawingTools
    {
        public static void DrawBarChart( Bitmap bitmap, Rectangle rect, Font font, BarData[] bars, string xLabel, string yLabel, string comment )
        {
             BarChart barChart = new BarChart( bars, xLabel, yLabel, comment );
             barChart.Draw(bitmap, rect, font);
        }

        public static void DrawScaledImage( Image image, Image imageToScale, Rectangle rect )
        {
            float scale = ScaleToFit( imageToScale.Size, rect.Size );
            int width = (int)( imageToScale.Width * scale );
            int height = (int)( imageToScale.Height * scale );
            int x = rect.X + ( rect.Width - width ) / 2;
            int y = rect.Y + ( rect.Height - height ) / 2;
            using( Graphics g = Graphics.FromImage( image ) ) {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage( imageToScale, x, y, width, height );
            }
        }

        public static Image LoadScaledImage( string filePath, Size size )
        {
            try {
                using( Image original = Image.FromFile( filePath ) ) {
                    return ScaleImage( original, size );
                }
            } catch ( OutOfMemoryException ) {
                throw new Exception( "The file does not have a valid format." );
            }
        }

        public static Image ScaleImage( Image original, Size size )
        {
            float scale = ScaleToFit( original.Size, size );
            int width = (int)( original.Width * scale );
            int height = (int)( original.Height * scale );
            Image image = new Bitmap( width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb );
            using( Graphics g = Graphics.FromImage( image ) ) {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage( original, 0, 0, image.Width, image.Height );
            }
            return image;
        }

        public static float ScaleToFit( Size objectSize, Size viewSize )
        {
            int width =  viewSize.Width;
            int height = viewSize.Height;
            float scaleHeight = (float)height / objectSize.Height;
            float scaleWidth = (float)width / objectSize.Width;
            if( scaleHeight < scaleWidth ) {
                return scaleHeight;
            } else {
                return scaleWidth;
            }
        }

        public static Image CreateImage( Size size, Brush brush )
        {
            Image newImage = new Bitmap( size.Width, size.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb );
            using( Graphics g = Graphics.FromImage( newImage ) ) {
                g.FillRectangle( brush, new Rectangle( 0, 0, size.Width, size.Height ) );
            }
            return newImage;
        }

        public static SizeF DrawNamedValue( Graphics g, string name, string value, Font font, PointF point )
        {
            string _name = name + ": ";
            SizeF size1 = g.MeasureString( _name, font );
            g.DrawString( _name, font, Brushes.Gray, point );
            SizeF size2 = g.MeasureString( value, font );
            g.DrawString( value, new Font( font, FontStyle.Bold ), Brushes.Black, point.X + size1.Width, point.Y );
            return new SizeF( size1.Width + size2.Width, Math.Max( size1.Height, size2.Height ) );
        }

        public static Color TransparentColor( Color c )
        {
            return Color.FromArgb( 170, c.R, c.G, c.B );
        }
    }
    
    
    public struct BarData
    {
        public string Label;
        public Decimal Value;
        public Color Color;

        public BarData( string label, Decimal value, Color color ) 
        {
            Label = label;
            Value = value;
            Color = color;
        }
    }

    public class BarChart
    {
        float xAxisY = 0.15f;
        float yAxisX = 0.2f;
        float xAxisRightX = 0.95f;
        float yAxisTopY = 0.95f;
        private string xAxisSign = "x";
        private string yAxisSign = "y";
        private string comment;

        private Font font;
        private Font boldFont;
        private Rectangle drawingRect;
        private BarData[] values;

        public BarChart( BarData[] bars, string xLabel, string yLabel, string _comment ) 
        {
            values = bars;
            xAxisSign = xLabel;
            yAxisSign = yLabel;
            comment = _comment;
        }

        public void Draw(Bitmap bitmap, Rectangle _drawingRect, Font _font)
        {
            if( !_drawingRect.IsEmpty ) 
            {
                drawingRect = _drawingRect;

                font = _font;
                try
                {
                    boldFont = new Font( font, FontStyle.Bold );
                } 
                catch
                {
                    //sometimes system cannot create bold font
                    boldFont = font;
                }

                using( Graphics graphics = Graphics.FromImage( bitmap ) )
                {
                    graphics.TranslateTransform( drawingRect.Left, drawingRect.Top );                    
                    
                    drawBackground( graphics );
                    drawYScale( graphics );
                    drawBars( graphics );
                    drawXAxis( graphics );
                    drawYAxis( graphics );
                    writeXSign( graphics );
                    writeYSign( graphics );
                    drawCoresNumberUnderSigns( graphics );
                    drawValuesAboveBars( graphics );
                }
            }
        }

        private float chartWidth
        {
            get
            {
                return ( xAxisRightX - yAxisX ) * drawingRect.Width;
            }
        }

        private float chartHeight
        {
            get
            {
                return ( yAxisTopY - xAxisY ) * drawingRect.Height;
            }
        }

        private void drawBackground( Graphics graphics )
        {
            eraseBackgoround( graphics );
            drawChartBackground( graphics );
        }

        private void eraseBackgoround( Graphics graphics )
        {
            SolidBrush backgroundBrush = new SolidBrush( Color.White );
            graphics.FillRectangle( backgroundBrush, 0, 0, drawingRect.Width, drawingRect.Height );            
        }

        private void drawChartBackground( Graphics graphics )
        {
            SolidBrush backgroundBrush = new SolidBrush( Color.FromArgb( 253,254,240 ) );
            graphics.FillRectangle( backgroundBrush, ( float ) yAxisX * drawingRect.Width, 
                ( float ) ( 1 - xAxisRightX ) * drawingRect.Height, chartWidth, chartHeight );
        }

        private float maxValueHeight
        {
            get
            {
                return drawingRect.Height * ( yAxisTopY - xAxisY );
            }
        }

        private float unitSize
        {
            get
            {
            //    return maxValueHeight / maxValue;
                return ( float ) ( chartHeight / Decimal.ToDouble( floorUp() * findNearestPowerOfTen() ) );
            }
        }

        private void drawBars( Graphics graphics )
        {            
            if( values == null || values.Length == 0 )
            {
                return;
            }

            double  barWidth = chartWidth / 2 / values.Length;


            // Draw the chart
            for( int i = 0; i < values.Length; i++ )
            {    
                SolidBrush chartBrush = new SolidBrush( DrawingTools.TransparentColor( values[i].Color ) );
                Pen pen = new Pen( values[i].Color, 2 );
                
                float heightRect = ( float ) ( Decimal.ToDouble( values[i].Value ) * unitSize );
                float y1 = ( float ) ( drawingRect.Height - xAxisY * drawingRect.Height - heightRect );
                graphics.FillRectangle( chartBrush, (float) ( yAxisX * drawingRect.Width + barWidth / 2 +  i * 2 * barWidth ), 
                    y1, (float) barWidth, heightRect );
                graphics.DrawRectangle( pen, (float) ( yAxisX * drawingRect.Width + barWidth / 2 +  i * 2 * barWidth ), 
                    y1, (float) barWidth, heightRect );

                chartBrush.Dispose();
                pen.Dispose();
            }
        }

        private void drawCoresNumberUnderSigns( Graphics graphics )
        {
            if( values == null || values.Length == 0 )
            {
                return;
            }

            double  barWidth = chartWidth / 2 / values.Length;
            
            SolidBrush textBrush = new SolidBrush( Color.Black );
            Pen textPen = new Pen( textBrush, 1 );
            
            for( int i = 0; i < values.Length; i++ )
            {
                float textWidth = graphics.MeasureString( values[i].Label, font ).Width;
                float textHeight = graphics.MeasureString( values[i].Label, font ).Height;

                graphics.DrawString( values[i].Label, boldFont, textBrush, 
                    (float) ( yAxisX * drawingRect.Width + barWidth / 2 +  i * 2 * barWidth + barWidth /2 - textWidth / 2 ),
                    drawingRect.Height - xAxisY * drawingRect.Height + 3 );
            }
            textBrush.Dispose();
        }

        private void drawValuesAboveBars( Graphics graphics )
        {
            if( values == null || values.Length == 0 )
            {
                return;
            }

            double  barWidth = chartWidth / 2 / values.Length;
            
            SolidBrush textBrush = new SolidBrush( Color.Black );
            Pen textPen = new Pen( textBrush, 1 );
            
            for( int i = 0; i < values.Length; i++ )
            {
                string acceleration = "";
                //if( i == 1 )
                if( i > 0 )
                {
                    Decimal accelerationValue = 0;
                    if( values[0].Value > 0 ) { 
                        accelerationValue = Decimal.Round( ( ( values[i].Value - values[0].Value ) * 100 /  values[0].Value ), 0 );
                    }
                    acceleration = " ( ";
                    if( accelerationValue > 0 ) {
                        acceleration += "+";
                    }
                    acceleration += accelerationValue.ToString("F1") + "% )";
                }
                string text = values[i].Value.ToString("F1") + acceleration;

                float textWidth = graphics.MeasureString( text, font ).Width;
                float textHeight = graphics.MeasureString( text, font ).Height;

                float heightRect = ( float ) ( Decimal.ToDouble( values[i].Value ) * unitSize );
                float y1 = ( float ) ( drawingRect.Height - xAxisY * drawingRect.Height - heightRect );

                graphics.DrawString( text, boldFont, textBrush, 
                    ( float ) ( yAxisX * drawingRect.Width + barWidth / 2 +  i * 2 * barWidth + barWidth /2 - textWidth / 2 ),
                    y1 - textHeight - 3 );
            }
            textBrush.Dispose();
        }

        private void drawXAxis( Graphics graphics )
        {
            // Create brushe for coloring the pie chart
            SolidBrush axisBrush = new SolidBrush( Color.FromArgb( 200, 203, 161 ) );
            Pen axisPen = new Pen( axisBrush, 2 );

            graphics.DrawLine( axisPen, yAxisX * drawingRect.Width - 5, drawingRect.Height - xAxisY * drawingRect.Height,
                xAxisRightX * drawingRect.Width, drawingRect.Height - xAxisY * drawingRect.Height );
        }

        private void drawYAxis( Graphics graphics )
        {
            // Create brushe for coloring the pie chart
            SolidBrush axisBrush = new SolidBrush( Color.FromArgb( 200, 203, 161 ) );
            Pen axisPen = new Pen( axisBrush, 2 );

            graphics.DrawLine( axisPen, yAxisX * drawingRect.Width, drawingRect.Height - xAxisY * drawingRect.Height + 5,
                yAxisX * drawingRect.Width, drawingRect.Height - yAxisTopY * drawingRect.Height );
        }

        private void writeXSign( Graphics graphics )
        {
            int textWidth = ( int ) graphics.MeasureString( xAxisSign, font ).Width;
            int textHeight = ( int ) graphics.MeasureString( xAxisSign, font ).Height;
        
            Debug.Assert( textWidth < drawingRect.Width );
            
            float startStringPosition = yAxisX * drawingRect.Width + ( chartWidth  - textWidth ) / 2 ;

            SolidBrush textBrush = new SolidBrush( Color.Olive );
            
            float yPosition = drawingRect.Height - ( xAxisY * drawingRect.Height - textHeight ) / 2 - textHeight / 2;

            graphics.DrawString( xAxisSign, font, textBrush, startStringPosition, yPosition );

            SolidBrush commentBrush = new SolidBrush( Color.Olive );
            
            textWidth = ( int ) graphics.MeasureString( comment, font ).Width;
            startStringPosition = yAxisX * drawingRect.Width + ( chartWidth  - textWidth ) / 2 ;
            yPosition += textHeight;
            graphics.DrawString( comment, font, commentBrush, startStringPosition, yPosition  );
        }

        private void writeYSign( Graphics graphics )
        {
            System.Drawing.Drawing2D.Matrix oldTransform = graphics.Transform;
            graphics.RotateTransform( -90, System.Drawing.Drawing2D.MatrixOrder.Prepend );

            int textWidth = ( int ) graphics.MeasureString( yAxisSign, font ).Width;
            int textHeight = ( int ) graphics.MeasureString( yAxisSign, font ).Height;

            float axisMarkWidth = graphics.MeasureString( this.maxValue.ToString(), font ).Width; 

            float xPosition = - drawingRect.Height + ( chartHeight - textWidth  ) / 2 + xAxisY * drawingRect.Height;
            float yPosition = yAxisX * drawingRect.Width - textHeight - 35;

            SolidBrush textBrush = new SolidBrush( Color.Olive );
            graphics.DrawString( yAxisSign, font, textBrush, xPosition, yPosition );
            
            graphics.Transform = oldTransform;
        }

        private Decimal maxValue
        {
            get
            {            
                Debug.Assert( values != null );

                Decimal returnValue = 0;
                for( int index = 0; index < values.Length; index++ )
                {
                    if( values[index].Value > returnValue )
                    {
                        returnValue = values[index].Value;
                    }
                }   
                return returnValue;
            }
        }

        private Decimal findNearestPowerOfTen()
        {
            if( maxValue == 0 ) {
                return 1;
            }

            Decimal currentPower = 10000; 
            while( currentPower > maxValue )
            {
                currentPower /= 10;
            }

            if( maxValue > currentPower &&
                maxValue < currentPower * 2 )
            {
                currentPower /= 10;
            }

            return currentPower;
        }

        private void drawYScale( Graphics graphics )
        {
            if( maxValue == 0 )
            {
                return;
            }
            
            Decimal nearestPower = findNearestPowerOfTen();
            Decimal markToDraw  = floorUp() * nearestPower / 10;

            while ( markToDraw * new Decimal( unitSize ) <= new Decimal( chartHeight ) + 1 ) {
                drawMark( graphics, ( float ) ( Decimal.ToDouble( markToDraw ) * unitSize ), markToDraw );
                markToDraw += floorUp() * nearestPower / 10;
            }            
        }

        private void drawMark( Graphics graphics, float markPosition, Decimal markValue )
        {
            SolidBrush axisBrush = new SolidBrush( Color.FromArgb( 200, 203, 161 ) );
            Pen axisPen = new Pen( axisBrush, 1 );
            SolidBrush textBrush = new SolidBrush( Color.Black );

            graphics.DrawLine( axisPen, yAxisX * drawingRect.Width - 5, 
                drawingRect.Height - ( markPosition + xAxisY * drawingRect.Height ), xAxisRightX * drawingRect.Width,
                drawingRect.Height - ( markPosition + xAxisY * drawingRect.Height ) );

            float stringHeight = graphics.MeasureString( markValue.ToString(), font ).Height;
            float stringWidth = graphics.MeasureString( markValue.ToString(), font ).Width;

            graphics.DrawString( markValue.ToString(), font, textBrush, yAxisX * drawingRect.Width - stringWidth - 2, 
                drawingRect.Height - ( markPosition + xAxisY * drawingRect.Height )  - stringHeight / 2 );
        }            

        Decimal floorUp()
        {
            Decimal floorUp = Decimal.Floor( maxValue / findNearestPowerOfTen() + 1 );
            return floorUp;
        }


    }
}
