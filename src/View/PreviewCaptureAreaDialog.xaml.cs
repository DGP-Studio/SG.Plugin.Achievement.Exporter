using ModernWpf.Controls;
using System.Windows.Media;

namespace Achievement.Exporter.Plugin.View
{
    public sealed partial class PreviewCaptureAreaDialog : ContentDialog
    {
        public ImageSource PreviewSource { get; }

        public PreviewCaptureAreaDialog(ImageSource imageSource)
        {
            this.PreviewSource = imageSource;
            this.DataContext = this;
            this.InitializeComponent();
        }
    }
}
