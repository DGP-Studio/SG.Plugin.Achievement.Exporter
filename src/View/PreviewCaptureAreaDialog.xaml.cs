using ModernWpf.Controls;
using System.Windows.Media;

namespace Achievement.Exporter.Plugin.View
{
    public sealed partial class PreviewCaptureAreaDialog : ContentDialog
    {
        public ImageSource PreviewSource { get; }

        public PreviewCaptureAreaDialog(ImageSource imageSource)
        {
            PreviewSource = imageSource;
            DataContext = this;
            InitializeComponent();
        }
    }
}
