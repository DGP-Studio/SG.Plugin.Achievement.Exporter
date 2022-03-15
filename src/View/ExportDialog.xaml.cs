using ModernWpf.Controls;
using Snap.Win32;
using System;
using System.Windows;

namespace Achievement.Exporter.Plugin
{
    public sealed partial class ExportDialog : ContentDialog
    {
        public const string Export1 = "cocogoat.work";
        public const string Export2 = "paimon.moe";
        public const string Export3 = "seelie.me";

        public string JsCode
        {
            get { return (string)GetValue(JsCodeProperty); }
            set { SetValue(JsCodeProperty, value); }
        }
        public static readonly DependencyProperty JsCodeProperty = DependencyProperty.Register("JsCode", typeof(string), typeof(ExportDialog), new PropertyMetadata(string.Empty));

        public ExportDialog(PaimonMoeJson paimonMoeJson)
        {
            DataContext = this;
            InitializeComponent();
            comboBoxSelectExport.SelectionChanged += (s, e) => SetJsCode(paimonMoeJson);
            SetJsCode(paimonMoeJson);
        }

        private void SetJsCode(PaimonMoeJson paimonMoeJson)
        {
            string stext = comboBoxSelectExport.SelectedIndex switch
            {
                0 => Export1,
                1 => Export2,
                2 => Export3,
                _ => throw new NotImplementedException(),
            };
            JsCode = stext switch
            {
                Export1 => TextUtils.GenerateCocogoatWorkJS("天地万象", paimonMoeJson),
                Export2 => TextUtils.GeneratePaimonMoeJS("天地万象", paimonMoeJson),
                Export3 => TextUtils.GenerateSeelieMeJS("天地万象", paimonMoeJson),
                _ => throw new NotImplementedException(),
            };
        }

        private void CopyCodeButtonClick(object sender, RoutedEventArgs e)
        {
            Clipboard.Clear();
            try
            {
                Clipboard.SetText(JsCode);
            }
            catch
            {
                try
                {
                    Clipboard2.SetText(JsCode);
                }
                catch { }
            }
        }
    }
}
