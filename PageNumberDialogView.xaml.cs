using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace Ascon.Pilot.SDK.GraphicLayerSample
{
    //class fontList : ObservableCollection<string>
    //{
    //    public fontList()
    //    {
    //        Add("Times New Roman");
    //        Add("Katherine Plus");
    //        //Type type = typeof(Fonts);
    //        //foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Static |
    //        //                                                         BindingFlags.Public))
    //        //{
    //        //    if (propertyInfo.Name.Equals("SystemFontFamilies"))
    //        //    {
    //        //        Add(propertyInfo.Name);

    //        //    }
    //        //}
    //    }
    //}
    public partial class TextEditorView
    {
        public string text { get; set; }
        public string fontSize { get; set; }
        public bool cancel { get; set; }
        public string fontFamilyName { get; set; }
        public System.Drawing.Color textColor { get; set; }

        public int intFontSize { get; set; }





        //private void GetListOfFonts()
        //{

        //    //Type type = typeof(Fonts);
        //    //foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Static |
        //    //                                                         BindingFlags.Public))
        //    //{
        //    //    if (propertyInfo.Name.Equals("SystemFontFamilies"))
        //    //    {
        //    //        var name = propertyInfo.Name;
        //    //        this.fontList = (ReadOnlyCollection<FontFamily>)propertyInfo.GetValue(null);
        //    //    }
        //    //}
        //    //using (System.Drawing.Text.InstalledFontCollection installedFonts = new System.Drawing.Text.InstalledFontCollection())
        //    //{
        //    //    foreach (System.Drawing.FontFamily fontFamily in installedFonts.Families)
        //    //    {
        //    //        fontList.Add(fontFamily.Name);
        //    //    }
        //    //}
        //}

        public TextEditorView()
        {
            //string[] fontList = new string[] { "Times New Roman", "Katherine Plus" };

            InitializeComponent();
            inputText.FontSize = Math.Round(Convert.ToDouble(fontSize) * 1.4);
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

        private void PrintedRadioBTNChecked(object sender, RoutedEventArgs e)
        {
            fontFamilyName = "Times New Roman";
            inputText.FontFamily = new FontFamily("Times New Roman");
        }

        private void HandWriteRadioBTNChecked(object sender, RoutedEventArgs e)
        {
            fontFamilyName = "Katherine Plus";
            inputText.FontFamily = new FontFamily(new Uri("file:///t:/manikhin_i/Fonts/"), "./#Katherine Plus");
        }

        private void BlackRadioBTNChecked(object sender, RoutedEventArgs e)
        {
            textColor = System.Drawing.Color.Black;
        }

        private void NavyRadioBTNChecked(object sender, RoutedEventArgs e)
        {
            textColor = System.Drawing.Color.Navy;
        }

        private void FontSizeTextChanged(object sender, RoutedEventArgs e)
        {
            fontSize = inputFontSize.Text;
            try
            {
                inputText.FontSize = Math.Round(Convert.ToDouble(fontSize) * 1.4);
            }
            catch
            {
                inputText.FontSize = 14;
            }    
        }

    }
}