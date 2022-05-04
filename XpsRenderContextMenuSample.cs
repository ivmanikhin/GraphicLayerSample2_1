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
            if (_accessLevel.ToString().Contains("Agrement")) 
                gotAccess = true;
            else
                gotAccess = false;

            builder.AddItem(RotateSignatureMenuItem, 0)
                   .WithHeader(GraphicLayerSample2_1.Properties.Resources.RotateSignatureMenuItem)
                   .WithIsEnabled(gotAccess); //пункт меню активен, если есть право согласовывать
            builder.AddItem(MoveSignatureMenuItem, 0)
                   .WithHeader(GraphicLayerSample2_1.Properties.Resources.MoveSignatureMenuItem)
                   .WithIsEnabled(gotAccess); //пункт меню активен, если есть право согласовывать
            builder.AddItem(AddTextLayerMenuItem, 0)
                   .WithHeader(GraphicLayerSample2_1.Properties.Resources.AddTextLayerMenuItem)
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
                _xOffset = (context.ClickPoint.X) * 25.4 / 96; //установка координат подписи в точку клика мышом
                _yOffset = (context.ClickPoint.Y) * 25.4 / 96;
                _scaleXY = 1;
                _angle = 0;
                AddGraphicLayerTextElement(context.DataObject, "asdjflajsdfl");
            }
        }


        private void AddGraphicLayerTextElement(IDataObject dataObject, string text)
        {
            var elementId = Guid.NewGuid();
            string xamlObject1 = XElement.Parse(string.Format("<TextBlock Foreground=\"Black\" FontSize=\"20\" TextAlignment=\"Center\">" + text + "</TextBlock>")).ToString();
            SaveToDataBaseXaml(dataObject, xamlObject1, Guid.NewGuid());
        }


        private void SaveToDataBaseXaml(IDataObject dataObject, string xamlObject, Guid elementId)
        {
            IObjectBuilder objectBuilder = _modifier.Edit(dataObject);
            MemoryStream memoryStream1 = new MemoryStream();
            using (StreamWriter streamWriter = new StreamWriter(memoryStream1))
            {
                streamWriter.Write(xamlObject);
                streamWriter.Flush();
                int position = _currentPerson.MainPosition.Position;
                string name = "PILOT_GRAPHIC_LAYER_ELEMENT_" + elementId + "_" + position;
                GraphicLayerElement o = GraphicLayerElementCreator.Create(_xOffset, _yOffset, new Point(_scaleXY, _scaleXY), _angle, position, _verticalAlignment, _horizontalAlignment, "xaml", elementId, _pageNumber, true);
                using (MemoryStream memoryStream2 = new MemoryStream())
                {
                    new XmlSerializer(typeof(GraphicLayerElement)).Serialize(memoryStream2, o);
                    objectBuilder.AddFile(name, memoryStream2, DateTime.Now, DateTime.Now, DateTime.Now);
                }
                objectBuilder.AddFile("PILOT_CONTENT_GRAPHIC_LAYER_ELEMENT_" + o.ContentId, memoryStream1, DateTime.Now, DateTime.Now, DateTime.Now);
                _modifier.Apply();
            }
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
                string name = "PILOT_GRAPHIC_LAYER_ELEMENT_" + GraphicLayerSample.ToGuid(_currentPerson.Id);
                GraphicLayerElement o = GraphicLayerElementCreator.Create(_xOffset, _yOffset, scale, _angle, position, _verticalAlignment, _horizontalAlignment, "bitmap", GraphicLayerSample.ToGuid(_currentPerson.Id), _pageNumber, true);
                using (MemoryStream memoryStream2 = new MemoryStream())
                {
                    new XmlSerializer(typeof(GraphicLayerElement)).Serialize(memoryStream2, o);
                    objectBuilder.AddFile(name, memoryStream2, DateTime.Now, DateTime.Now, DateTime.Now);
                    objectBuilder.AddFile("PILOT_CONTENT_GRAPHIC_LAYER_ELEMENT_" + o.ContentId, memoryStream1, DateTime.Now, DateTime.Now, DateTime.Now);
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
