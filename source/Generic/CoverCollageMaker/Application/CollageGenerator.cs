using CoverCollageMaker.Domain.Enums;
using CoverCollageMaker.Domain.Exceptions;
using CoverCollageMaker.Domain.ValueObjects;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CoverCollageMaker.Application
{
    public static class CollageGenerator
    {
        #region Public Methods
        public static SKBitmap CreateCollageWithFinalSize(
            List<ImageData> imagesData,
            CollageParameters parameters,
            CancellationToken cancellationToken = default)
        {
            ValidateParameters(imagesData, parameters);
            parameters = parameters.CreateClone();
            SortAndSetRowsAndColumns(imagesData, parameters);
            CalculateDimensionsForFinalSize(parameters, imagesData, cancellationToken);

            return GenerateCollageInternal(imagesData, parameters, cancellationToken);
        }

        public static SKBitmap CreateCollageWithCellWidth(
            List<ImageData> imagesData,
            CollageParameters parameters,
            CancellationToken cancellationToken = default)
        {
            ValidateParameters(imagesData, parameters, checkCellWidth: true);
            parameters = parameters.CreateClone();
            SortAndSetRowsAndColumns(imagesData, parameters);

            var tallestImageInfo = GetTallestImageDimensions(imagesData);
            parameters.CellHeight = (parameters.CellWidth * tallestImageInfo.Height) / tallestImageInfo.Width;
            CalculateDimensionsForCell(parameters, imagesData, cancellationToken);

            return GenerateCollageInternal(imagesData, parameters, cancellationToken);
        }

        public static SKBitmap CreateCollageWithCellHeight(
            List<ImageData> imagesData,
            CollageParameters parameters,
            CancellationToken cancellationToken = default)
        {
            ValidateParameters(imagesData, parameters, checkCellHeight: true);
            parameters = parameters.CreateClone();
            SortAndSetRowsAndColumns(imagesData, parameters);

            var widestImageInfo = GetWidestImageDimensions(imagesData);
            parameters.CellWidth = (parameters.CellHeight * widestImageInfo.Width) / widestImageInfo.Height;
            CalculateDimensionsForCell(parameters, imagesData, cancellationToken);

            return GenerateCollageInternal(imagesData, parameters, cancellationToken);
        }

        public static int CalculateRowCountForGrid(int imagesCount, int columnsCount)
        {
            var cols = Math.Min(imagesCount, columnsCount);
            return (int)Math.Ceiling((double)imagesCount / cols);
        }
        #endregion

        #region Private Helper Methods

        private static SKBitmap GenerateCollageInternal(
            List<ImageData> imagesData,
            CollageParameters parameters,
            CancellationToken cancellationToken)
        {
            var collage = new SKBitmap(parameters.FinalWidth, parameters.FinalHeight);
            try
            {
                using (var canvas = new SKCanvas(collage))
                {
                    canvas.Clear(ToSKColor(parameters.BackgroundColor));
                    DrawImagesAndTitles(canvas, imagesData, parameters, cancellationToken);
                }
            }
            catch
            {
                collage.Dispose();
                throw;
            }

            return collage;
        }

        private static void SortAndSetRowsAndColumns(List<ImageData> imagesData, CollageParameters parameters)
        {
            SortImages(imagesData, parameters.ImageInsertOrder);
            parameters.Columns = Math.Min(parameters.Columns, imagesData.Count);
            parameters.Rows = CalculateRowCountForGrid(imagesData.Count, parameters.Columns);
        }

        private static void ValidateParameters(
            List<ImageData> imagesData,
            CollageParameters parameters,
            bool checkCellWidth = false,
            bool checkCellHeight = false)
        {
            if (imagesData is null || imagesData.Count == 0)
            {
                throw new ArgumentException("Invalid number of images.");
            }

            if (parameters.Columns <= 0)
            {
                throw new ArgumentException("Columns must be greater than 0.");
            }

            if (checkCellWidth && parameters.CellWidth <= 0)
            {
                throw new ArgumentException("Cell width must be greater than 0.");
            }

            if (checkCellHeight && parameters.CellHeight <= 0)
            {
                throw new ArgumentException("Cell height must be greater than 0.");
            }
        }

        private static void CalculateDimensionsForFinalSize(
            CollageParameters parameters,
            List<ImageData> imagesData,
            CancellationToken cancellationToken)
        {
            int availableWidthForCells = parameters.FinalWidth - parameters.Padding * 2;
            parameters.CellWidth = (availableWidthForCells - (parameters.Columns - 1) * parameters.HorizontalSpacing) / parameters.Columns;

            using (var paint = GetPaintFromParameters(parameters))
            {
                float totalTitleHeight = CalculateTotalTitleHeight(imagesData, parameters, parameters.CellWidth, paint);
                var availableHeightForCells = parameters.FinalHeight - Convert.ToInt32(totalTitleHeight) - parameters.Padding * 2;
                if (availableHeightForCells <= 0 || availableHeightForCells < imagesData.Count)
                {
                    throw new InsufficientSpaceForImageException();
                }

                parameters.CellHeight = (availableHeightForCells - (parameters.Rows - 1) * parameters.VerticalSpacing) / parameters.Rows;
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        private static void CalculateDimensionsForCell(
            CollageParameters parameters,
            List<ImageData> imagesData,
            CancellationToken cancellationToken)
        {
            parameters.FinalWidth = (parameters.Columns * (parameters.CellWidth + parameters.HorizontalSpacing) - parameters.HorizontalSpacing) + parameters.Padding * 2;
            using (var paint = GetPaintFromParameters(parameters))
            {
                float totalTitleHeight = CalculateTotalTitleHeight(imagesData, parameters, parameters.CellWidth, paint);
                parameters.FinalHeight = (parameters.Rows * (parameters.CellHeight + parameters.VerticalSpacing) - parameters.VerticalSpacing + Convert.ToInt32(totalTitleHeight)) + parameters.Padding * 2;
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        private static float CalculateTotalTitleHeight(
            List<ImageData> images,
            CollageParameters parameters,
            int cellWidth,
            SKPaint paint)
        {
            if (!parameters.ShowTexts)
            {
                return 0;
            }

            if (parameters.TextInsertMethod == TextInsertMethod.Trim)
            {
                return parameters.Rows * paint.TextSize * 1.2f;
            }

            int cols = (int)Math.Ceiling((double)images.Count / parameters.Rows);
            float totalHeight = 0;
            for (int row = 0; row < parameters.Rows; row++)
            {
                float tallestTitleHeight = 0;
                for (int col = 0; col < cols && row * cols + col < images.Count; col++)
                {
                    var imageData = images[row * cols + col];
                    if (imageData.Name.IsNullOrEmpty())
                    {
                        continue;
                    }

                    if (parameters.TextInsertMethod == TextInsertMethod.Wrap)
                    {
                        float maxTextWidth = cellWidth * 0.85f;
                        var wrappedText = WrapText(imageData.Name, paint, maxTextWidth);
                        float lineHeight = paint.TextSize * 1.2f;
                        float titleHeight = wrappedText.Count * lineHeight;
                        if (titleHeight > tallestTitleHeight)
                        {
                            tallestTitleHeight = titleHeight;
                        }
                    }
                }

                // Add the tallest title height in this row to the total height
                totalHeight += tallestTitleHeight;
            }

            return totalHeight;
        }

        private static void DrawImagesAndTitles(
            SKCanvas canvas,
            List<ImageData> images,
            CollageParameters parameters,
            CancellationToken cancellationToken)
        {
            int rowYPoint = parameters.Padding, imageIndex = 0;
            using (var bitmapResizePaint = new SKPaint())
            {
                bitmapResizePaint.FilterQuality = SKFilterQuality.High;
                bitmapResizePaint.IsAntialias = false;
                bitmapResizePaint.IsDither = false;

                // Resized images become blurry so we apply a little sharpening
                var kernel = new float[9]
                {
                       0, -.1f,    0,
                    -.1f, 1.4f, -.1f,
                       0, -.1f,    0,
                };

                var kernelSize = new SKSizeI(3, 3);
                var kernelOffset = new SKPointI(1, 1);

                bitmapResizePaint.ImageFilter = SKImageFilter.CreateMatrixConvolution(
                    kernelSize, kernel, 1f, 0f, kernelOffset,
                    SKShaderTileMode.Clamp, false);

                using (var textDrawPaint = GetPaintFromParameters(parameters))
                {
                    for (int row = 0; row < parameters.Rows; row++)
                    {
                        int columnXPoint = parameters.Padding;
                        float heightTallestTitle = 0;
                        for (int col = 0; col < parameters.Columns && imageIndex < images.Count; col++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            var imageData = images[imageIndex];
                            using (var originalBitmap = SKBitmap.Decode(imageData.Path))
                            {
                                if (originalBitmap.Width != parameters.CellWidth || originalBitmap.Height != parameters.CellHeight)
                                {
                                    using (var resizedBitmap = ResizeBitmapToFit(originalBitmap, parameters.CellWidth, parameters.CellHeight))
                                    {
                                        if (resizedBitmap is null)
                                        {
                                            throw new InvalidOperationException("Resized bitmap is null.");
                                        }

                                        DrawImage(canvas, parameters, rowYPoint, columnXPoint, resizedBitmap, bitmapResizePaint);
                                    }
                                }
                                else
                                {
                                    DrawImage(canvas, parameters, rowYPoint, columnXPoint, originalBitmap, null);
                                }

                                if (parameters.ShowTexts && !imageData.Name.IsNullOrEmpty())
                                {
                                    float titleHeight = DrawText(canvas, imageData.Name, parameters, columnXPoint, rowYPoint, textDrawPaint);
                                    if (titleHeight > heightTallestTitle)
                                    {
                                        heightTallestTitle = titleHeight;
                                    }
                                }

                                columnXPoint += parameters.CellWidth + parameters.HorizontalSpacing;
                                imageIndex++;
                            }
                        }

                        rowYPoint += parameters.CellHeight + parameters.VerticalSpacing + Convert.ToInt32(heightTallestTitle);
                    }
                }
            }
        }

        private static void DrawImage(SKCanvas canvas, CollageParameters parameters, int rowYPoint, int columnXPoint, SKBitmap bitmap, SKPaint bitmapResizePaint = null)
        {
            int offsetX = (parameters.CellWidth - bitmap.Width) / 2;
            int offsetY = (parameters.CellHeight - bitmap.Height) / 2;
            canvas.DrawBitmap(bitmap, columnXPoint + offsetX, rowYPoint + offsetY, bitmapResizePaint);
        }

        private static float DrawText(SKCanvas canvas, string title, CollageParameters parameters, int xPoint, int yPoint, SKPaint textDrawPaint)
        {
            float maxTextWidth = parameters.CellWidth * 0.85f;
            float textY = yPoint + parameters.CellHeight;

            if (parameters.TextInsertMethod == TextInsertMethod.Wrap)
            {
                return DrawWrappedText(canvas, title, textDrawPaint, xPoint, textY, maxTextWidth, parameters.CellWidth, parameters.TextHorizontalAlignment);
            }
            else if (parameters.TextInsertMethod == TextInsertMethod.Trim)
            {
                return DrawTrimmedText(canvas, title, textDrawPaint, xPoint, textY, maxTextWidth, parameters.CellWidth, parameters.TextHorizontalAlignment);
            }

            return 0;
        }

        private static float DrawWrappedText(SKCanvas canvas, string text, SKPaint paint, int x, float y, float maxTextWidth, int cellWidth, TextHorizontalAlignment alignment)
        {
            var wrappedTextLines = WrapText(text, paint, maxTextWidth);
            float lineHeight = paint.TextSize * 1.2f;
            float initialY = y;

            foreach (var line in wrappedTextLines)
            {
                float textX = GetAlignedTextX(line, paint, x, cellWidth, alignment);
                y += lineHeight;
                canvas.DrawText(line, textX, y, paint);
            }

            return y - initialY;
        }

        private static float DrawTrimmedText(SKCanvas canvas, string text, SKPaint paint, int x, float y, float maxTextWidth, int cellWidth, TextHorizontalAlignment alignment)
        {
            var trimmedText = text;
            if (paint.MeasureText(trimmedText) > maxTextWidth)
            {
                const string ellipsis = "...";
                while (paint.MeasureText(trimmedText.Trim() + ellipsis) > maxTextWidth && trimmedText.Length > 0)
                {
                    trimmedText = trimmedText.Substring(0, trimmedText.Length - 1);
                }

                trimmedText = trimmedText.Trim() + ellipsis;
            }

            float textX = GetAlignedTextX(trimmedText, paint, x, cellWidth, alignment);
            y += paint.TextSize * 1.2f;
            canvas.DrawText(trimmedText, textX, y, paint);

            return paint.TextSize * 1.2f;
        }

        #endregion

        #region Utility Methods
        private static void SortImages(List<ImageData> imagesData, ImageInsertOrder order)
        {
            switch (order)
            {
                case ImageInsertOrder.Original:
                    break;
                case ImageInsertOrder.Path:
                    imagesData.Sort((x, y) => string.Compare(x.Path, y.Path, StringComparison.OrdinalIgnoreCase));
                    break;
                case ImageInsertOrder.Name:
                    imagesData.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
                    break;
            }
        }

        private static SKColor ToSKColor(Color color)
        {
            return new SKColor(color.R, color.G, color.B, color.A);
        }

        private static SKPaint GetPaintFromParameters(CollageParameters parameters)
        {
            var paint = new SKPaint
            {
                Color = ToSKColor(parameters.TextColor),
                TextSize = parameters.TextFontSize,
                IsAntialias = parameters.TextAntiAliasing
            };

            var fontStyleWeight = parameters.TextFontBold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
            var fontStyleSlant = parameters.TextFontItalic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;

            var fontStyle = new SKFontStyle(fontStyleWeight, SKFontStyleWidth.Normal, fontStyleSlant);
            paint.Typeface = SKTypeface.FromFamilyName(parameters.TextFontName, fontStyle);

            return paint;
        }

        private static SKImageInfo GetWidestImageDimensions(List<ImageData> imagesData)
        {
            var currentInfo = new SKImageInfo();
            foreach (var imageData in imagesData)
            {
                using (var stream = File.OpenRead(imageData.Path))
                {
                    var info = SKBitmap.DecodeBounds(stream);
                    if (currentInfo.Width < info.Width)
                    {
                        currentInfo = info;
                    }
                }
            }

            return currentInfo;
        }

        private static SKImageInfo GetTallestImageDimensions(List<ImageData> imagesData)
        {
            var currentInfo = new SKImageInfo();
            foreach (var imageData in imagesData)
            {
                using (var stream = File.OpenRead(imageData.Path))
                {
                    var info = SKBitmap.DecodeBounds(stream);
                    if (currentInfo.Height < info.Height)
                    {
                        currentInfo = info;
                    }
                }
            }

            return currentInfo;
        }

        private static SKBitmap ResizeBitmapToFit(SKBitmap image, int maxWidth, int maxHeight)
        {
            var scaleFactor = GetScaleFactorToFitImage(image.Width, image.Height, maxWidth, maxHeight);
            var newWidth = image.Width * scaleFactor;
            var newHeight = image.Height * scaleFactor;

            // Ensure the dimensions are within bounds, rounding down to avoid exceeding max dimensions
            var resizeWidth = Math.Min(maxWidth, (int)Math.Round(newWidth));
            var resizeHeight = Math.Min(maxHeight, (int)Math.Round(newHeight));

            var resizedImage = image.Resize(new SKImageInfo(resizeWidth, resizeHeight), SKFilterQuality.High);
            return resizedImage;
        }

        private static float GetScaleFactorToFitImage(int imageWidth, int imageHeight, int maxWidth, int maxHeight)
        {
            float widthScale = (float)maxWidth / imageWidth;
            float heightScale = (float)maxHeight / imageHeight;
            return Math.Min(widthScale, heightScale);
        }

        private static float GetAlignedTextX(string text, SKPaint paint, int x, int cellWidth, TextHorizontalAlignment alignment)
        {
            switch (alignment)
            {
                case TextHorizontalAlignment.Left:
                    return x;
                case TextHorizontalAlignment.Center:
                    return x + (cellWidth - paint.MeasureText(text)) / 2;
                case TextHorizontalAlignment.Right:
                    return cellWidth - x - paint.MeasureText(text);
                default:
                    throw new NotSupportedException($"TextHorizontalAlignment '{alignment}' is not supported.");
            }
        }

        private static List<string> WrapText(string text, SKPaint paint, float maxWidth)
        {
            var wrappedLines = new List<string>();
            var words = text.Split(' ');
            var currentLine = "";

            foreach (var word in words)
            {
                var newLine = currentLine.IsNullOrEmpty() ? word : $"{currentLine} {word}";
                if (paint.MeasureText(newLine) <= maxWidth)
                {
                    currentLine = newLine;
                }
                else
                {
                    if (!currentLine.IsNullOrEmpty())
                    {
                        wrappedLines.Add(currentLine);
                    }

                    currentLine = word;
                }
            }

            if (!currentLine.IsNullOrEmpty())
            {
                wrappedLines.Add(currentLine);
            }

            return wrappedLines;
        }

        #endregion
    }
}
