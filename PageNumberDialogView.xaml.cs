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
    public partial class PageNumberDialogView
    {
        public int pageNumber { get; set; }
        public int _currentPage;
        public bool cancel { get; set; }



        public PageNumberDialogView(int currentPage)
        {

            InitializeComponent();
            _currentPage = currentPage; //задаём номер текущей страницы, полученный из вне
            pageNumber = _currentPage;
            inputPageNumber.Text = Convert.ToString(pageNumber);
            cancel = true;
        }

        private void OnOkButtonClicked(object sender, RoutedEventArgs e)
        {
            try //если вместо целого числа написано что-то другое, на ОК не реагируем (см. catch)
            {
                pageNumber = int.Parse(inputPageNumber.Text);
                if (pageNumber > _currentPage || pageNumber < 1) //если введённое число больше текущей страницы и меньше 1, на ОК не реагируем, присваиваем текущую страницу
                {
                    pageNumber = _currentPage;
                    inputPageNumber.Text = Convert.ToString(pageNumber);
                }
                else //если введено число, удовлетворяюшее условиям, принимаем его и закрываем окно
                {
                    cancel = false;
                    Close();
                }
            }
            catch
            {
                pageNumber = _currentPage;
                inputPageNumber.Text = Convert.ToString(pageNumber);
            }
        }

        private void OnCancelButtonClicked(object sender, RoutedEventArgs e)
        {
            cancel = true;
            Close();
        }

    }
}