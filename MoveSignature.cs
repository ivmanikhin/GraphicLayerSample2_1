
using Ascon.Pilot.SDK.GraphicLayerSample.Properties;
using Ascon.Pilot.SDK.Menu;
using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
//using System.Linq;
//using System.Collections.Generic;
using System.Windows;
using System.Xml.Serialization;
using Ascon.Pilot.SDK.ObjectsSample;
//using System.Xml.Linq;
using GraphicLayerSample2_1.Properties;

namespace Ascon.Pilot.SDK.GraphicLayerSample
{

    [Export(typeof(IMenu<XpsRenderClickPointContext>))]
    public class MoveSignature : IMenu<XpsRenderClickPointContext>
    {
        private readonly IObjectsRepository _repository;
        private DataObjectWrapper _selected;
        private readonly string dec_separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        private readonly IObjectModifier _modifier;
        private readonly IPerson _currentPerson;
        private readonly IFileProvider _fileProvider;
        private string _filePath = string.Empty;
        //private bool _includeStamp;
        private bool xpsIsSigned = false;
        private bool signatureIsOnXPS = false;
        private double _xOffset;
        private double _yOffset;
        private double _scaleXY;
        private double _angle;
        private int _pageNumber;
        //private int signatureNumber = 0;
        private VerticalAlignment _verticalAlignment;
        private HorizontalAlignment _horizontalAlignment;
        //private bool gotAccess = false;
        bool notFrozen = false;
        //private AccessLevel _accessLevel = AccessLevel.None;
        private const string MoveSignatureMenuItem = "MoveSignatureMenuItem";
        private const string SignHereMenuItem = "SignHereMenuItem";
        


        [ImportingConstructor]
        public MoveSignature(IObjectModifier modifier, IObjectsRepository repository, IFileProvider fileProvider)
        {
            _modifier = modifier;
            _currentPerson = repository.GetCurrentPerson();
            _repository = repository;
            _fileProvider = fileProvider;

        }


        public void Build(IMenuBuilder builder, XpsRenderClickPointContext context)
        //создание пунктов меню: "Перенести подпись сюда" и "Перенести сюда и повернуть":
        {
            //запрос прав на согласование документа:
            _selected = new DataObjectWrapper(context.DataObject, _repository);
            //_accessLevel = GetMyAccessLevel(_selected);
            //gotAccess = _accessLevel.ToString().Contains("Agrement") |
            //            _accessLevel.ToString().Contains("Agreement") |
            //            _accessLevel.ToString().Contains("Full");
            // проверка, не заморожен ли документ
            notFrozen = !(_selected.StateInfo.State.ToString().Contains("Frozen"));
            // проверка, подписал ли подписант
            xpsIsSigned = XPSSignedByCyrrentUser(context.DataObject);
            signatureIsOnXPS = SignatureIsOnXPS(context.DataObject);

            builder.AddItem(SignHereMenuItem, 0)
                   .WithHeader(Resources.SignHereMenuItem)
                   .WithIsEnabled(notFrozen & xpsIsSigned & !signatureIsOnXPS);

            builder.AddItem(MoveSignatureMenuItem, 0)
                   .WithHeader(Resources.MoveSignatureMenuItem)
                   .WithIsEnabled(notFrozen & xpsIsSigned & signatureIsOnXPS); //пункт меню активен, если есть право согласовывать, есть электронная подпись текущего пользователя

        }

        public void OnMenuItemClick(string name, XpsRenderClickPointContext context)
        {
            CheckSettings();
            _pageNumber = context.PageNumber + 1; //задание номера страницы
            _xOffset = (context.ClickPoint.X - 10 / _scaleXY) * 25.4 / 96; //установка координат подписи в точку клика мышом
            _yOffset = (context.ClickPoint.Y - 4 / _scaleXY) * 25.4 / 96;

            if (name == MoveSignatureMenuItem)
            {
                MoveSignatureHere(context);
            }

            else if (name == SignHereMenuItem)
            {
                SignHere(context.DataObject);
            }

        }


        public void MoveSignatureHere(XpsRenderClickPointContext context)
            {
            foreach (IFile file in context.DataObject.Files)
            {
                if (file.Name.Equals("PILOT_GRAPHIC_LAYER_ELEMENT_" + ToGuid(_currentPerson.Id)))
                {
                    var stream = _fileProvider.OpenRead(file);
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(GraphicLayerElement));
                    GraphicLayerElement element = (GraphicLayerElement)xmlSerializer.Deserialize(stream);
                    element.VerticalAlignment = VerticalAlignment.Top;
                    element.HorizontalAlignment = HorizontalAlignment.Left;
                    element.PageNumber = context.PageNumber;
                    element.OffsetX = context.ClickPoint.X - 10 / _scaleXY;
                    element.OffsetY = context.ClickPoint.Y - 4 / _scaleXY;
                    IObjectBuilder objectBuilder = _modifier.Edit(context.DataObject);
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        new XmlSerializer(typeof(GraphicLayerElement)).Serialize(memoryStream, element);
                        objectBuilder.AddOrReplaceFile(file.Name, memoryStream, file, DateTime.Now, DateTime.Now, DateTime.Now);
                    };
                    _modifier.Apply();
                }
            }
        }




        private void CheckSettings()
        {
            _filePath = Settings.Default.Path;
            if (!double.TryParse(Settings.Default.Scale.Replace(".", dec_separator).Replace(",", dec_separator), out _scaleXY))
                _scaleXY = 1.0;
            if (!double.TryParse(Settings.Default.Angle, out _angle))
                _angle = 0;
        }



        //private int CountSignaturesOfCurrentUser(IDataObject dataObject)
        //{
        //    signatureNumber = 0;    
        //    foreach (IFile file in dataObject.Files)
        //    {
        //        if (file.Name.Contains("PILOT_GRAPHIC_LAYER_ELEMENT_" + ToGuid(_currentPerson.Id)))
        //            signatureNumber ++;
        //    }
        //    return signatureNumber;
        //}

        // Рисование дополнительной подписи текущего пользователя

        //private void AddRastrToXPS(IDataObject dataObject)
        //{
        //    var elementId = Guid.NewGuid(); // рандомный GUID
        //    if (string.IsNullOrEmpty(_filePath))
        //        return;
        //    IObjectBuilder objectBuilder = _modifier.Edit(dataObject);
        //    using (FileStream fileStream = File.Open(_filePath, FileMode.Open, FileAccess.ReadWrite))
        //    {
        //        int position = _currentPerson.MainPosition.Position;
        //        byte[] buffer = new byte[fileStream.Length];
        //        fileStream.Read(buffer, 0, (int)fileStream.Length);
        //        MemoryStream memoryStream1 = new MemoryStream(buffer);
        //        Point scale = new Point(_scaleXY, _scaleXY);
        //        string name = "PILOT_GRAPHIC_LAYER_ELEMENT_" + elementId + "_" + position; //имя файла с записью свойств картинки
        //                                                                                   //ПРИВЯЗАНО К ЧЕЛОВЕКУ В ВИДЕ _currentPerson.MainPosition.Position в конце имени файла
        //        GraphicLayerElement o = GraphicLayerElementCreator.Create(_xOffset, _yOffset, scale, _angle, position, _verticalAlignment, _horizontalAlignment, "bitmap", elementId, _pageNumber, true);
        //        using (MemoryStream memoryStream2 = new MemoryStream())
        //        {
        //            new XmlSerializer(typeof(GraphicLayerElement)).Serialize(memoryStream2, o);
        //            objectBuilder.AddFile(name, memoryStream2, DateTime.Now, DateTime.Now, DateTime.Now); //создание записи о расположении картинки на листе
        //            objectBuilder.AddFile("PILOT_CONTENT_GRAPHIC_LAYER_ELEMENT_" + o.ContentId, memoryStream1, DateTime.Now, DateTime.Now, DateTime.Now); //создание файла PNG. НЕ СОДЕРЖИТ ПРИВЯЗКУ К ЧЕЛОВЕКУ.
        //                                                                                                                                                  //CONTENT ID - РАНДОМНЫЙ GUID
        //        }
        //        _modifier.Apply();
        //    }

        //}

        // Рисование главной подписи текущего пользователя
        private void SignHere(IDataObject dataObject)
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
                string name = "PILOT_GRAPHIC_LAYER_ELEMENT_" + ToGuid(_currentPerson.Id); //имя файла с записью свойств картинки
                                                                                          //ПРИВЯЗАНО К ЧЕЛОВЕКУ В ВИДЕ GUID C НУЛЯМИ (В ОСНОВНОМ)
                GraphicLayerElement o = GraphicLayerElementCreator.Create(_xOffset, _yOffset, scale, _angle, position, _verticalAlignment, _horizontalAlignment, "bitmap", ToGuid(_currentPerson.Id), _pageNumber, true);
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


        public static Guid ToGuid(int value)
        {
            byte[] b = new byte[16];
            BitConverter.GetBytes(value).CopyTo(b, 0);
            return new Guid(b);
        }


        private bool XPSSignedByCyrrentUser(IDataObject dataObject)
        {
            foreach (IFile file in dataObject.Files)
            {
                if (file.CreatorId().Equals(_currentPerson.Id) & file.Name.Equals("PilotDigitalSignature"))
                    return true;
            }
            return false;
        }

        private bool SignatureIsOnXPS(IDataObject dataObject)
        {
            foreach (IFile file in dataObject.Files)
            {
                if (file.Name.Equals("PILOT_GRAPHIC_LAYER_ELEMENT_" + ToGuid(_currentPerson.Id)))
                    return true;
            }
            return false;
        }
    }
}

