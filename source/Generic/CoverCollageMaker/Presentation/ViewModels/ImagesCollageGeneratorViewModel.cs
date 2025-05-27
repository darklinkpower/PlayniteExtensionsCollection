using CoverCollageMaker.Application;
using CoverCollageMaker.Domain.Enums;
using CoverCollageMaker.Domain.ValueObjects;
using Playnite.SDK;
using PluginsCommon;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CoverCollageMaker.Presentation.ViewModels
{
    public class ImagesCollageGeneratorViewModel : ObservableObject
    {
        private double _zoomScale = 1.0;
        private const double _zoomIncrement = 0.2;
        private const double _maxZoom = 5.0;
        private const double _minZoom = 0.5;
        public double ZoomScale
        {
            get => _zoomScale;
            set
            {
                if (_zoomScale != value)
                {
                    _zoomScale = value;
                    OnPropertyChanged(nameof(ZoomScale));
                    OnPropertyChanged(nameof(ZoomLevel));
                }
            }
        }

        public string ZoomLevel => $"{(int)(_zoomScale * 100)}%";

        public RelayCommand ResetZoomCommand { get; }

        private readonly ILogger _logger;
        private readonly IPlayniteAPI _playniteApi;
        private readonly List<ImageData> _imagesData;
        public CollageParameters CollageParameters { get; }
        public List<string> FontFamilies { get; }

        private System.Windows.Media.Color _pageBackgroundColor;
        public System.Windows.Media.Color PageBackgroundColor
        {
            get => _pageBackgroundColor;
            set
            {
                _pageBackgroundColor = value;
                OnPropertyChanged();
            }
        }

        private System.Windows.Media.Color _titleColor;
        public System.Windows.Media.Color TitleColor
        {
            get => _titleColor;
            set
            {
                _titleColor = value;
                OnPropertyChanged();
            }
        }

        public List<TextInsertMethod> TextInsertMethods { get; }
        public List<TextHorizontalAlignment> TextHorizontalAlignments { get; }
        public RelayCommand CreateCollageWithCellHeightCommand { get; }
        public RelayCommand CreateCollageWithCellWidthCommand { get; }
        public RelayCommand CreateCollageWithFinalSizeCommand { get; }
        public RelayCommand SelectExportDirectoryCommand { get; }
        public RelayCommand ExportImageCommand { get; }
        public RelayCommand CopyImageToClipboardCommand { get; }

        private WriteableBitmap _collageImageBitmap;
        public WriteableBitmap CollageImageBitmap
        {
            get => _collageImageBitmap;
            set
            {
                _collageImageBitmap = value;
                OnPropertyChanged();
            }
        }

        private string _collageImageBitmapResolution = string.Empty;
        public string CollageImageBitmapResolution
        {
            get => _collageImageBitmapResolution;
            set
            {
                _collageImageBitmapResolution = value;
                OnPropertyChanged();
            }
        }

        private string _exportDirectory;
        public string ExportDirectory
        {
            get => _exportDirectory;
            set
            {
                _exportDirectory = value;
                OnPropertyChanged();
            }
        }

        private string _exportFileName;
        public string ExportFileName
        {
            get => _exportFileName;
            set
            {
                _exportFileName = value;
                OnPropertyChanged();
            }
        }

        public ImagesCollageGeneratorViewModel(
            ILogger logger,
            IPlayniteAPI playniteApi,
            List<ImageData> imagesData = null,
            string exportDirectory = null,
            CollageParameters collageParameters = null)
        {
            _logger = logger;
            _playniteApi = playniteApi;
            _imagesData = imagesData ?? new List<ImageData>();
            ExportDirectory = !exportDirectory.IsNullOrEmpty() ? exportDirectory : string.Empty;
            CollageParameters = collageParameters ?? new CollageParameters();
            FontFamilies = GetFontFamilies();
            TextInsertMethods = new List<TextInsertMethod>
            {
                TextInsertMethod.Wrap,
                TextInsertMethod.Trim
            };

            TextHorizontalAlignments = new List<TextHorizontalAlignment>
            {
                TextHorizontalAlignment.Left,
                TextHorizontalAlignment.Center,
                TextHorizontalAlignment.Right
            };

            PageBackgroundColor = System.Windows.Media.Color.FromArgb(0, 255, 255, 255);
            TitleColor = System.Windows.Media.Color.FromArgb(255, 255, 255, 255);

            CreateCollageWithCellHeightCommand = new RelayCommand(() => CreateCollageWithCellHeight());
            CreateCollageWithCellWidthCommand = new RelayCommand(() => CreateCollageWithCellWidth());
            CreateCollageWithFinalSizeCommand = new RelayCommand(() => CreateCollageWithFinalSize());
            SelectExportDirectoryCommand = new RelayCommand(() => SelectExportDirectory());
            ExportImageCommand = new RelayCommand(() => SaveImageAsPng(), () => _collageImageBitmap != null && !ExportDirectory.IsNullOrEmpty());
            CopyImageToClipboardCommand = new RelayCommand(() => CopyImageToClipboard(), () => _collageImageBitmap != null);
            ResetZoomCommand = new RelayCommand(ResetZoom);
        }

        private void ResetZoom()
        {
            ZoomScale = 1.0;
        }

        public void OnMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta > 0 && ZoomScale < _maxZoom)
            {
                ZoomScale += _zoomIncrement;
            }
            else if (e.Delta < 0 && ZoomScale > _minZoom)
            {
                ZoomScale -= _zoomIncrement;
            }
        }

        private void SelectExportDirectory()
        {
            var selectedDirectory = _playniteApi.Dialogs.SelectFolder();
            if (!selectedDirectory.IsNullOrEmpty() && FileSystem.DirectoryExists(selectedDirectory))
            {
                ExportDirectory = selectedDirectory;
            }
        }

        private void CreateCollage(CollageGenerationMethod collageGenerationMethod)
        {

            _playniteApi.Dialogs.ActivateGlobalProgress(progArgs =>
            {
                SKBitmap skBitmap = null;
                try
                {
                    switch (collageGenerationMethod)
                    {
                        case CollageGenerationMethod.ByCellHeight:
                            skBitmap = CollageGenerator.CreateCollageWithCellHeight(
                                _imagesData, CollageParameters, progArgs.CancelToken);
                            break;
                        case CollageGenerationMethod.ByCellWidth:
                            skBitmap = CollageGenerator.CreateCollageWithCellWidth(
                                _imagesData, CollageParameters, progArgs.CancelToken);
                            break;
                        case CollageGenerationMethod.ByFinalSize:
                            skBitmap = CollageGenerator.CreateCollageWithFinalSize(
                                _imagesData, CollageParameters, progArgs.CancelToken);
                            break;
                        default:
                            throw new NotSupportedException($"CollageGenerationMethod '{collageGenerationMethod}' is not supported.");
                    }

                    _playniteApi.MainView.UIDispatcher.Invoke(() =>
                    {
                        // If WriteableBitmap is null or size has changed, recreate it
                        if (CollageImageBitmap is null || CollageImageBitmap.PixelWidth != skBitmap.Width || CollageImageBitmap.PixelHeight != skBitmap.Height)
                        {
                            CollageImageBitmap = new WriteableBitmap(skBitmap.Width, skBitmap.Height, 96, 96, PixelFormats.Bgra32, null);
                        }

                        // Lock the bitmap for writing
                        CollageImageBitmap.Lock();

                        // Get the pixel data pointer directly from SKBitmap (unmanaged memory)
                        IntPtr pixelDataPtr = skBitmap.GetPixels();

                        // Copy the pixel data directly into the WriteableBitmap's back buffer
                        CollageImageBitmap.WritePixels(
                            new Int32Rect(0, 0, skBitmap.Width, skBitmap.Height),
                            pixelDataPtr,
                            skBitmap.RowBytes * skBitmap.Height,
                            skBitmap.RowBytes);

                        // Unlock the bitmap after writing
                        CollageImageBitmap.Unlock();

                        //ToWriteableBitmap(skBitmap, ref WriteableBitmap);
                        CollageImageBitmapResolution = $"{CollageImageBitmap.Width}x{CollageImageBitmap.Height}px";
                        ResetZoom();
                    });
                }
                catch (OperationCanceledException e)
                {
                    _logger.Debug(e, "Collage generation was cancelled");
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error while generating collage");
                    _playniteApi.Dialogs.ShowErrorMessage("Error while generating collage:" + e.Message);
                }
                finally
                {
                    skBitmap?.Dispose();
                }
            }, new GlobalProgressOptions("Generating images collage...", true));
        }

        private void CreateCollageWithCellHeight()
        {
            CreateCollage(CollageGenerationMethod.ByCellHeight);
        }

        private void CreateCollageWithCellWidth()
        {
            CreateCollage(CollageGenerationMethod.ByCellWidth);
        }

        private void CreateCollageWithFinalSize()
        {
            CreateCollage(CollageGenerationMethod.ByFinalSize);
        }

        public void SaveImageAsJpg() => SaveImage("jpg", () => new JpegBitmapEncoder());
        public void SaveImageAsPng() => SaveImage("png", () => new PngBitmapEncoder());

        private void SaveImage(string extension, Func<BitmapEncoder> encoderFactory)
        {
            try
            {
                if (_collageImageBitmap is null)
                {
                    throw new InvalidOperationException("CollageImageBitmap is null. There is no image to save.");
                }

                var fileName = string.IsNullOrWhiteSpace(ExportFileName)
                    ? DateTime.Now.ToString("yyyyMMdd_HHmmss_fff")
                    : ExportFileName;

                var filePath = Path.Combine(ExportDirectory, $"{fileName}.{extension}");
                if (FileSystem.FileExists(filePath))
                {
                    var boxResult = _playniteApi.Dialogs.ShowMessage(
                        $"File already exists at:\n{filePath}.\n\nDo you want to overwrite it?", "", MessageBoxButton.YesNo);
                    if (boxResult == MessageBoxResult.No)
                    {
                        return;
                    }

                    FileSystem.DeleteFile(filePath);
                }

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    var encoder = encoderFactory();
                    encoder.Frames.Add(BitmapFrame.Create(_collageImageBitmap));
                    encoder.Save(fileStream);
                }

                _playniteApi.Dialogs.ShowMessage($"Saved image to:\n{filePath}.");
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error while saving collage");
                _playniteApi.Dialogs.ShowErrorMessage("An error occurred while saving collage: " + e.Message);
            }
        }

        private static BitmapSource ToBitmapSource(SKBitmap skBitmap)
        {
            // Get the total number of bytes for the bitmap
            int totalBytes = skBitmap.RowBytes * skBitmap.Height;
            byte[] pixelData = new byte[totalBytes];

            // Copy the pixel data from SKBitmap to the byte array
            System.Runtime.InteropServices.Marshal.Copy(skBitmap.GetPixels(), pixelData, 0, totalBytes);

            // Create the BitmapSource using the pixel data
            var bitmapSource = BitmapSource.Create(
                skBitmap.Width,
                skBitmap.Height,
                96, // DPI X
                96, // DPI Y
                PixelFormats.Bgra32, // SKBitmap is usually in BGRA format
                null,
                pixelData,
                skBitmap.RowBytes);

            bitmapSource.Freeze(); // Freezing is necessary for multi-threaded UI use
            return bitmapSource;
        }

        private void CopyImageToClipboard()
        {
            try
            {
                if (_collageImageBitmap is null)
                {
                    return;
                }

                Clipboard.Clear();
                Clipboard.SetImage(_collageImageBitmap);
                _playniteApi.Dialogs.ShowMessage(
                    "Image copied to clipboard.\n\nNote: Transparency is not preserved when copying images to the clipboard."
                );
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error while setting the image to clipboard");
                _playniteApi.Dialogs.ShowErrorMessage(
                    $"An error occurred while copying the image to the clipboard:\n{e.Message}"
                );
            }
        }

        private List<string> GetFontFamilies()
        {
            return System.Drawing.FontFamily.Families.Select(f => f.Name).ToList();
        }



    }
}
