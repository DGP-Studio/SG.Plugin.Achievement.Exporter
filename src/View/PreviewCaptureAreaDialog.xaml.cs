using ModernWpf.Controls;
using System.Windows.Media;

namespace Achievement.Exporter.Plugin
{
    public partial class PreviewCaptureAreaDialog : ContentDialog
    {
        public ImageSource PreviewSource { get; set; }

        public PreviewCaptureAreaDialog(ImageSource imageSource)
        {
            PreviewSource = imageSource;
            DataContext = this;
            InitializeComponent();
        }
    }
}
