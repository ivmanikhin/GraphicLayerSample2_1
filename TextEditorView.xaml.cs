using System;
using System.Windows;

namespace Ascon.Pilot.SDK.GraphicLayerSample
{
    
    public partial class TextEditorView
    {
        public string text { get; set; }
        public string fontSize { get; set; }
        public bool cancel { get; set; }    

        public TextEditorView()
        {
            InitializeComponent();
            cancel = true;
        }

        private void OnOkButtonClicked(object sender, RoutedEventArgs e)
        {
            text = inputText.Text;
            fontSize = inputFontSize.Text;
            cancel = false;
            Close();
        }

        private void OnCancelButtonClicked(object sender, RoutedEventArgs e)
        {
            cancel = true;
            Close();
        }

    }
}