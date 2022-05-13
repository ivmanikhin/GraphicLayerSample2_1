// Decompiled with JetBrains decompiler
// Type: Ascon.Pilot.SDK.GraphicLayerSample.GraphicLayerSample
// Assembly: Ascon.Pilot.SDK.GraphicLayerSample.ext2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 187B3BB9-3768-4B7C-861E-6A56C03BF53E
// Assembly location: D:\Projects\Pilot-ICE\SDK\b396a650-48de-48bb-bf68-8ed251a97fbe\Ascon.Pilot.SDK.GraphicLayerSample.ext2.dll

using Ascon.Pilot.SDK.GraphicLayerSample.Properties;
using Ascon.Pilot.SDK.Menu;
using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Xml.Serialization;
using Ascon.Pilot.SDK.ObjectsSample;
using System.Xml.Linq;

namespace Ascon.Pilot.SDK.GraphicLayerSample
{
      
    [Export(typeof(IMenu<XpsRenderClickPointContext>))]
    public class XpsRenderContextMenuSample : IMenu<XpsRenderClickPointContext>
    {
        private readonly IObjectsRepository _repository;
        private DataObjectWrapper _selected;
        private readonly string dec_separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        private readonly IObjectModifier _modifier;
        private readonly IPerson _currentPerson;
        private string _filePath = string.Empty;
        private bool _includeStamp;
        private double _xOffset;
        private double _yOffset;
        private double _scaleXY;
        private double _angle;
        private int _pageNumber;
        private VerticalAlignment _verticalAlignment;
        private HorizontalAlignment _horizontalAlignment;
        private bool gotAccess = false;
        private AccessLevel _accessLevel = AccessLevel.None;
        private const string MoveSignatureMenuItem = "MoveSignatureMenuItem";
        private const string RotateSignatureMenuItem = "RotateSignatureMenuItem";
        private const string AddTextLayerMenuItem = "AddTextLayerMenuItem";
        private const string AddExtraSignatureMenuItem = "AddExtraSignatureMenuItem";
        //private int fontSize = 14;
        private string text = "000000";
        private string fontSize = "20";
        private string fontFamilyName = "Times New Roman";
        private System.Drawing.Color textColor = System.Drawing.Color.Black;

        [ImportingConstructor]
        public XpsRenderContextMenuSample(IObjectModifier modifier, IObjectsRepository repository)
        {
            _modifier = modifier;
            _currentPerson = repository.GetCurrentPerson();
            _repository = repository;  

        }


        public void Build(IMenuBuilder builder, XpsRenderClickPointContext context)
        //создание пунктов меню: "Перенести подпись сюда" и "Перенести сюда и повернуть":
        {
            //запрос прав на согласование документа:
            _selected = new DataObjectWrapper(context.DataObject, _repository);
            _accessLevel = GetMyAccessLevel(_selected);
            gotAccess = _accessLevel.ToString().Contains("Agrement");


            builder.AddItem(AddTextLayerMenuItem, 0)
                   .WithHeader(GraphicLayerSample2_1.Properties.Resources.AddTextLayerMenuItem)
                   .WithIsEnabled(gotAccess); //пункт меню активен, если есть право согласовывать
            builder.AddItem(AddExtraSignatureMenuItem, 0)
                   .WithHeader(GraphicLayerSample2_1.Properties.Resources.AddExtraSignatureMenuItem)
                   .WithIsEnabled(gotAccess); //пункт меню активен, если есть право согласовывать
            builder.AddItem(RotateSignatureMenuItem, 0)
                   .WithHeader(GraphicLayerSample2_1.Properties.Resources.RotateSignatureMenuItem)
                   .WithIsEnabled(gotAccess); //пункт меню активен, если есть право согласовывать
            builder.AddItem(MoveSignatureMenuItem, 0)
                   .WithHeader(GraphicLayerSample2_1.Properties.Resources.MoveSignatureMenuItem)
                   .WithIsEnabled(gotAccess); //пункт меню активен, если есть право согласовывать


        }

        public void OnMenuItemClick(string name, XpsRenderClickPointContext context)
        {
            if (name == MoveSignatureMenuItem)
            {
                CheckSettings(); //чтение натсроек подписи
                _pageNumber = context.PageNumber + 1; //задание номера страницы
                _xOffset = (context.ClickPoint.X - 10 / _scaleXY) * 25.4 / 96; //установка координат подписи в точку клика мышом
                _yOffset = (context.ClickPoint.Y - 4 / _scaleXY) * 25.4 / 96;
                UpdateRastrInXPS(context.DataObject);
            }

            else if (name == RotateSignatureMenuItem)
            {
                _pageNumber = context.PageNumber + 1;
                CheckSettings();
                _xOffset = (context.ClickPoint.X - 4 / _scaleXY) * 25.4 / 96;
                _yOffset = (context.ClickPoint.Y + 10 / _scaleXY) * 25.4 / 96;
                _angle = 270;   // задание угла поворота подписи вместо указанного в настройках
                UpdateRastrInXPS(context.DataObject);
            }

            else if (name == AddTextLayerMenuItem)
            {
                _pageNumber = context.PageNumber + 1; //задание номера страницы
                _xOffset = (context.ClickPoint.X) * 25.4 / 96; //установка координат в точку клика мышом
                _yOffset = (context.ClickPoint.Y) * 25.4 / 96;
                _scaleXY = 1;
                _angle = 0;
                TextEditorView textEditorView = new TextEditorView();
                textEditorView.ShowDialog();
                if (!textEditorView.cancel)
                {
                    fontFamilyName = textEditorView.fontFamilyName;
                    textColor = textEditorView.textColor;
                    fontSize = textEditorView.fontSize;
                    if (!int.TryParse(fontSize, out int intFontSize))
                        intFontSize = 12;
                    text = textEditorView.text/*.Replace("\n", "<LineBreak />") относилось к печати в виде XAML*/;
                    if (text != "")
                        AddGraphicLayerTextElement(context.DataObject, text, intFontSize);
                }
            }

            else if (name == AddExtraSignatureMenuItem)
            {
                CheckSettings(); //чтение натсроек подписи
                _pageNumber = context.PageNumber + 1; //задание номера страницы
                _xOffset = (context.ClickPoint.X - 10 / _scaleXY) * 25.4 / 96; //установка координат подписи в точку клика мышом
                _yOffset = (context.ClickPoint.Y - 4 / _scaleXY) * 25.4 / 96;
                AddRastrToXPS(context.DataObject);
            }
                       
        }


        private void AddGraphicLayerTextElement(IDataObject dataObject, string text, int intFontSize)
        {
            var elementId = Guid.NewGuid(); // рандомный GUID
            System.Drawing.Image textImage = TextToImage(text, fontFamilyName, intFontSize * 4, textColor); //рисование текста в bitmap
            /* втавка текста в виде текста (устарело):
            string xamlObject1 = XElement.Parse(string.Format("<TextBlock Foreground=\"Black\" FontSize=\"" + fontSize + "\" TextAlignment=\"Left\">" + text + "</TextBlock>")).ToString();
            SaveToDataBaseXaml(dataObject, xamlObject1, elementId);
            */
            SaveToDataBaseTextBitmap(dataObject, textImage, elementId);
        }


        private void SaveToDataBaseTextBitmap(IDataObject dataObject, System.Drawing.Image textBitmap, Guid elementId)
        {
            IObjectBuilder objectBuilder = _modifier.Edit(dataObject);
            int position = _currentPerson.MainPosition.Position;
            MemoryStream memoryStream1 = new MemoryStream();
            textBitmap.Save(memoryStream1, System.Drawing.Imaging.ImageFormat.Png);
            Point scale = new Point(0.25, 0.25);
            string name = "PILOT_GRAPHIC_LAYER_ELEMENT_" + elementId + "_" + position; //имя файла с записью свойств картинки
                                                                                       //ПРИВЯЗАНО К ЧЕЛОВЕКУ В ВИДЕ _currentPerson.MainPosition.Position в конце имени файла
            GraphicLayerElement o = GraphicLayerElementCreator.Create(_xOffset, _yOffset, scale, 0, position, _verticalAlignment, _horizontalAlignment, "bitmap", elementId, _pageNumber, true);
            using (MemoryStream memoryStream2 = new MemoryStream())
            {
                new XmlSerializer(typeof(GraphicLayerElement)).Serialize(memoryStream2, o);
                objectBuilder.AddFile(name, memoryStream2, DateTime.Now, DateTime.Now, DateTime.Now); //создание записи о расположении картинки на листе
                objectBuilder.AddFile("PILOT_CONTENT_GRAPHIC_LAYER_ELEMENT_" + o.ContentId, memoryStream1, DateTime.Now, DateTime.Now, DateTime.Now); //создание файла PNG. НЕ СОДЕРЖИТ ПРИВЯЗКУ К ЧЕЛОВЕКУ.
                                                                                                                                                      //CONTENT ID - РАНДОМНЫЙ GUID
            }
            _modifier.Apply();

        }

        /* Сохранение текста в виде XAML (устарело):
        private void SaveToDataBaseXaml(IDataObject dataObject, string xamlObject, Guid elementId)
        {
            IObjectBuilder objectBuilder = _modifier.Edit(dataObject);
            MemoryStream memoryStream1 = new MemoryStream();
            using (StreamWriter streamWriter = new StreamWriter(memoryStream1))
            {
                streamWriter.Write(xamlObject);
                streamWriter.Flush();
                int position = _currentPerson.MainPosition.Position;
                string name = "PILOT_GRAPHIC_LAYER_ELEMENT_" + elementId + "_" + position; //имя файла записи с рандомным elementID и привязкой к человеку через ID пользователя
                GraphicLayerElement o = GraphicLayerElementCreator.Create(_xOffset, _yOffset, new Point(_scaleXY, _scaleXY), _angle, position, _verticalAlignment, _horizontalAlignment, "xaml", elementId, _pageNumber, true);
                using (MemoryStream memoryStream2 = new MemoryStream())
                {
                    new XmlSerializer(typeof(GraphicLayerElement)).Serialize(memoryStream2, o);
                    objectBuilder.AddFile(name, memoryStream2, DateTime.Now, DateTime.Now, DateTime.Now);
                }
                objectBuilder.AddFile("PILOT_CONTENT_GRAPHIC_LAYER_ELEMENT_" + o.ContentId, memoryStream1, DateTime.Now, DateTime.Now, DateTime.Now); //создаёт файл с текстом XAML с рандомным именем.
                                                                                                                                                      //PILOT_CONTENT_GRAPHIC_LAYER_ELEMENT_ в распакованном XPS не найден.
                                                                                                                                                      //contentId всегда рандомный
                _modifier.Apply();
            }
        }
        */


        private System.Drawing.Image TextToImage(string text, string fontName, int intFontSize, System.Drawing.Color textColor)
            //рисовалка текста, чтобы отвязаться от установленных у пользователей шрифтов
        {
            System.Drawing.Font font = new System.Drawing.Font(fontName, intFontSize);
            //System.Drawing.Font handWriteFont = new System.Drawing.Font(fontCollection.Families[0], intFontSize);

            //first, create a dummy bitmap just to get a graphics object
            System.Drawing.Image img = new System.Drawing.Bitmap(1, 1);
            System.Drawing.Graphics drawing = System.Drawing.Graphics.FromImage(img);

            //DirectoryInfo d = new DirectoryInfo("/");
            //FileInfo[] fileArray = d.GetFiles("*.ttf");
            //string str = "";
            //foreach (FileInfo file in fileArray)
            //    str = str + file.DirectoryName + "\n";
            //text = str;


            //measure the string to see how big the image needs to be
            if (fontName == "Katherine Plus")
            {
                System.Drawing.Text.PrivateFontCollection fontCollection = new System.Drawing.Text.PrivateFontCollection();
                fontCollection.AddFontFile("9540.ttf");
                font = new System.Drawing.Font(fontCollection.Families[0], intFontSize);
            }
            System.Drawing.SizeF textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            img = new System.Drawing.Bitmap((int)textSize.Width, (int)textSize.Height);
            drawing = System.Drawing.Graphics.FromImage(img);

            //create a brush for the text
            System.Drawing.Brush textBrush = new System.Drawing.SolidBrush(textColor);
            //drawing.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; //не оправдало себя
            //drawing.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias; //не оправдало себя
            drawing.DrawString(text, font, textBrush, 0, 0);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            return img;
        }

        private void CheckSettings()
        {
            _filePath = Settings.Default.Path;
            _includeStamp = Settings.Default.IncludeStamp;
            if (!double.TryParse(Settings.Default.Scale.Replace(".", dec_separator).Replace(",", dec_separator), out _scaleXY)) //если трайпарс не смог,
                _scaleXY = 1;                                                                                                   //масштаб равен 1
            double.TryParse(Settings.Default.Angle, out _angle);
        }


        private void UpdateRastrInXPS(IDataObject dataObject)
        {
            //удаление всех файлов с подписью пользователя из документа:
            foreach (IFile file in dataObject.Files)
            {
                if (file.Name.Equals("PILOT_GRAPHIC_LAYER_ELEMENT_" + GraphicLayerSample.ToGuid(_currentPerson.Id)))
                    _modifier.Edit(dataObject).RemoveFile(file.Id);
                    
            }
            SaveToDataBaseRastr(dataObject);
        }



        private void AddRastrToXPS(IDataObject dataObject)
        {

            var elementId = Guid.NewGuid(); // рандомный GUID
            if (string.IsNullOrEmpty(_filePath))
                return;
            IObjectBuilder objectBuilder = _modifier.Edit(dataObject);
            using (FileStream fileStream = File.Open(_filePath, FileMode.Open, FileAccess.ReadWrite))
            {
                int position = _currentPerson.MainPosition.Position;
                byte[] buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, (int)fileStream.Length);
                MemoryStream memoryStream1 = new MemoryStream(buffer);
                Point scale = new Point(_scaleXY, _scaleXY);
                string name = "PILOT_GRAPHIC_LAYER_ELEMENT_" + elementId + "_" + position; //имя файла с записью свойств картинки
                                                                                           //ПРИВЯЗАНО К ЧЕЛОВЕКУ В ВИДЕ _currentPerson.MainPosition.Position в конце имени файла
                GraphicLayerElement o = GraphicLayerElementCreator.Create(_xOffset, _yOffset, scale, _angle, position, _verticalAlignment, _horizontalAlignment, "bitmap", elementId, _pageNumber, true);
                using (MemoryStream memoryStream2 = new MemoryStream())
                {
                    new XmlSerializer(typeof(GraphicLayerElement)).Serialize(memoryStream2, o);
                    objectBuilder.AddFile(name, memoryStream2, DateTime.Now, DateTime.Now, DateTime.Now); //создание записи о расположении картинки на листе
                    objectBuilder.AddFile("PILOT_CONTENT_GRAPHIC_LAYER_ELEMENT_" + o.ContentId, memoryStream1, DateTime.Now, DateTime.Now, DateTime.Now); //создание файла PNG. НЕ СОДЕРЖИТ ПРИВЯЗКУ К ЧЕЛОВЕКУ.
                                                                                                                                                          //CONTENT ID - РАНДОМНЫЙ GUID
                }
                _modifier.Apply();
            }

        }


        private void SaveToDataBaseRastr(IDataObject dataObject)
        {
            if (string.IsNullOrEmpty(_filePath))
                return;
            IObjectBuilder objectBuilder = _modifier.Edit(dataObject);
            using (FileStream fileStream = File.Open(_filePath, FileMode.Open, FileAccess.ReadWrite))
            {
                int position = _currentPerson.MainPosition.Position;
                byte[] buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, (int)fileStream.Length);
                MemoryStream memoryStream1 = new MemoryStream(buffer);
                Point scale = new Point(_scaleXY, _scaleXY);
                string name = "PILOT_GRAPHIC_LAYER_ELEMENT_" + GraphicLayerSample.ToGuid(_currentPerson.Id); //имя файла с записью свойств картинки
                                                                                                             //ПРИВЯЗАНО К ЧЕЛОВЕКУ В ВИДЕ GUID C НУЛЯМИ (В ОСНОВНОМ)
                GraphicLayerElement o = GraphicLayerElementCreator.Create(_xOffset, _yOffset, scale, _angle, position, _verticalAlignment, _horizontalAlignment, "bitmap", GraphicLayerSample.ToGuid(_currentPerson.Id), _pageNumber, true);
                using (MemoryStream memoryStream2 = new MemoryStream())
                {
                    new XmlSerializer(typeof(GraphicLayerElement)).Serialize(memoryStream2, o);
                    objectBuilder.AddFile(name, memoryStream2, DateTime.Now, DateTime.Now, DateTime.Now); //создание записи о расположении картинки на листе
                    objectBuilder.AddFile("PILOT_CONTENT_GRAPHIC_LAYER_ELEMENT_" + o.ContentId, memoryStream1, DateTime.Now, DateTime.Now, DateTime.Now); //создание файла PNG. НЕ СОДЕРЖИТ ПРИВЯЗКУ К ЧЕЛОВЕКУ.
                                                                                                                                                          //CONTENT ID - РАНДОМНЫЙ GUID
                }
                _modifier.Apply();
            }
        }

        private AccessLevel GetMyAccessLevel(DataObjectWrapper element)
        {
            var currentAccesLevel = AccessLevel.None;
            var person = _repository.GetCurrentPerson();
            foreach (var position in person.Positions)
            {
                currentAccesLevel = currentAccesLevel | GetAccessLevel(element, position.Position);
            }

            return currentAccesLevel;
        }

        private AccessLevel GetAccessLevel(DataObjectWrapper element, int positonId)
        {
            var currentAccesLevel = AccessLevel.None;
            var orgUnits = _repository.GetOrganisationUnits().ToDictionary(k => k.Id);
            var accesses = GetAccessRecordsForPosition(element, positonId, orgUnits);
            foreach (var source in accesses)
            {
                currentAccesLevel = currentAccesLevel | source.Access.AccessLevel;
            }
            return currentAccesLevel;
        }

        private IEnumerable<AccessRecordWrapper> GetAccessRecordsForPosition(DataObjectWrapper obj, int positionId, IDictionary<int, IOrganisationUnit> organisationUnits)
        {
            return obj.Access.Where(x => BelongsTo(positionId, x.OrgUnitId, organisationUnits));
        }

        public static bool BelongsTo(int position, int organisationUnit, IDictionary<int, IOrganisationUnit> organisationUnits)
        {
            Stack<int> units = new Stack<int>();
            units.Push(organisationUnit);
            while (units.Any())
            {
                var unitId = units.Pop();
                if (position == unitId)
                    return true;

                IOrganisationUnit unit;
                if (organisationUnits.TryGetValue(unitId, out unit))
                {
                    foreach (var childUnitId in unit.Children)
                    {
                        units.Push(childUnitId);
                    }
                }
            }
            return false;
        }
    }
}
