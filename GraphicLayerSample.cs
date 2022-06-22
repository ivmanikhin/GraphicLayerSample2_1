// Decompiled with JetBrains decompiler
// Type: Ascon.Pilot.SDK.GraphicLayerSample.GraphicLayerSample
// Assembly: Ascon.Pilot.SDK.GraphicLayerSample.ext2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 187B3BB9-3768-4B7C-861E-6A56C03BF53E
// Assembly location: D:\Projects\Pilot-ICE\SDK\b396a650-48de-48bb-bf68-8ed251a97fbe\Ascon.Pilot.SDK.GraphicLayerSample.ext2.dll

using Ascon.Pilot.SDK.GraphicLayerSample.Properties;
using Ascon.Pilot.SDK.Menu;
using System;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace Ascon.Pilot.SDK.GraphicLayerSample
{
    [Export(typeof (IMenu<MainViewContext>))]
    public class GraphicLayerSample : IMenu<MainViewContext>, IHandle<UnloadedEventArgs>, IHandle, IObserver<INotification>
    {
        private readonly string dec_separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        private readonly IObjectModifier _modifier;
        private readonly IObjectsRepository _repository;
        private readonly IXpsViewer _xpsViewer;
        private int _currentPage;
        private IPerson _currentPerson;
        private const string ServiceGraphicLayerMenu = "ServiceGraphicLayerMenu";
        private GraphicLayerElementSettingsView _settingsView;
        private GraphicLayerElementSettingsModel _model;
        private VerticalAlignment _verticalAlignment;
        private HorizontalAlignment _horizontalAlignment;
        private string _filePath = string.Empty;
        private double _xOffset;
        private double _yOffset;
        private double _scaleXY;
        private double _angle;
        private int _pageNumber;
        private bool _includeStamp;

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [ImportingConstructor]
        public GraphicLayerSample(IEventAggregator eventAggregator, IObjectModifier modifier, IObjectsRepository repository, IPilotDialogService dialogService, IXpsViewer xpsViewer)
        {
            object accent = ColorConverter.ConvertFromString(dialogService.AccentColor);
            if (accent != null)
              Theme.ColorScheme.ColorScheme.Initialize((Color) accent, dialogService.Theme);
            eventAggregator.Subscribe(this);
            _xpsViewer = xpsViewer;
            _modifier = modifier;
            _repository = repository;
            IObservable<INotification> observable1 = repository.SubscribeNotification(NotificationKind.ObjectSignatureChanged);
            IObservable<INotification> observable2 = repository.SubscribeNotification(NotificationKind.ObjectFileChanged);
            observable1.Subscribe((IObserver<INotification>) this);
            observable2.Subscribe((IObserver<INotification>) this);
            CheckSettings();
        }

        public class ObjectLoader : IObserver<IDataObject>
        {
            private readonly IObjectsRepository _repository;
            //     private IDataObject _dataObject;
            private IDisposable _subscription;
            private TaskCompletionSource<IDataObject> _tcs;
            private long _changesetId;

            public ObjectLoader(IObjectsRepository repository)
            {
                _repository = repository;
            }



            public Task<IDataObject> Load(Guid id, long changesetId = 0)
            {
                _changesetId = changesetId;
                _tcs = new TaskCompletionSource<IDataObject>();
                _subscription = _repository.SubscribeObjects(new[] { id }).Subscribe(this);
                return _tcs.Task;
            }

            public void OnNext(IDataObject value)
            {
                if (value.State != DataState.Loaded)
                    return;

                if (value.LastChange() < _changesetId)
                    return;

                _tcs.TrySetResult(value);
                _subscription.Dispose();
            }

            public void OnError(Exception error) { }
            public void OnCompleted() { }
        }



        public void Build(IMenuBuilder builder, MainViewContext context)
        {
            string name = builder.ItemNames.First<string>();
            builder.GetItem(name).AddItem(ServiceGraphicLayerMenu, 0).WithHeader(GraphicLayerSample2_1.Properties.Resources.txtMenuItem);
        }

        public void OnMenuItemClick(string itemName, MainViewContext context)
        {
            string x = _xOffset.ToString(CultureInfo.InvariantCulture);
            string y = _yOffset.ToString(CultureInfo.InvariantCulture);
            string scale = _scaleXY.ToString(CultureInfo.InvariantCulture);
            string angle = _angle.ToString(CultureInfo.InvariantCulture);
            string pageNumber = _pageNumber.ToString(CultureInfo.InvariantCulture);
            bool includeStamp = _includeStamp;
            _model = new GraphicLayerElementSettingsModel(_filePath, x, y, scale, angle, _verticalAlignment, _horizontalAlignment, includeStamp, pageNumber);
            _model.OnSaveSettings += new EventHandler(ReloadSettings);
            if (itemName != ServiceGraphicLayerMenu)
                return;
            GraphicLayerElementSettingsView elementSettingsView = new GraphicLayerElementSettingsView();
            elementSettingsView.DataContext = _model;
            _settingsView = elementSettingsView;
            _settingsView.Unloaded += new RoutedEventHandler(SettingsViewOnUnloaded);
            new WindowInteropHelper(_settingsView).Owner = GetForegroundWindow();
            _settingsView.ShowDialog();
            Dispatcher.Run();
        }

        private void ReloadSettings(object sender, EventArgs e) => CheckSettings();

        private void CheckSettings()
        {
            _filePath = Settings.Default.Path;
            _includeStamp = Settings.Default.IncludeStamp;
            double.TryParse(Settings.Default.X, out _xOffset);
            double.TryParse(Settings.Default.Y, out _yOffset);
            if (!double.TryParse(Settings.Default.Scale.Replace(".", dec_separator).Replace(",", dec_separator), out _scaleXY))
                _scaleXY = 1.0;
            double.TryParse(Settings.Default.Angle, out _angle);
            int.TryParse(Settings.Default.PageNumber, out _pageNumber);
            Enum.TryParse(Settings.Default.VerticalAligment, out _verticalAlignment);
            Enum.TryParse(Settings.Default.HorizontalAligment, out _horizontalAlignment);
        }

        private void SettingsViewOnUnloaded(object sender, RoutedEventArgs e)
        {
            _settingsView.Unloaded -= new RoutedEventHandler(SettingsViewOnUnloaded);
            _model.OnSaveSettings -= new EventHandler(ReloadSettings);
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
                fileStream.Read(buffer, 0, (int) fileStream.Length);
                MemoryStream memoryStream1 = new MemoryStream(buffer);
                Point scale = new Point(_scaleXY, _scaleXY);
                string name = "PILOT_GRAPHIC_LAYER_ELEMENT_" + ToGuid(_currentPerson.Id);
                GraphicLayerElement o = GraphicLayerElementCreator.Create(_xOffset, _yOffset, scale, _angle, position, _verticalAlignment, _horizontalAlignment, "bitmap", ToGuid(_currentPerson.Id), _pageNumber, true);
                using (MemoryStream memoryStream2 = new MemoryStream())
                {
                    new XmlSerializer(typeof (GraphicLayerElement)).Serialize(memoryStream2, o);
                    objectBuilder.AddFile(name, memoryStream2, DateTime.Now, DateTime.Now, DateTime.Now);
                    objectBuilder.AddFile("PILOT_CONTENT_GRAPHIC_LAYER_ELEMENT_" + o.ContentId, memoryStream1, DateTime.Now, DateTime.Now, DateTime.Now);
                }
                _modifier.Apply();
            }
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
                    new XmlSerializer(typeof (GraphicLayerElement)).Serialize(memoryStream2, o);
                    objectBuilder.AddFile(name, memoryStream2, DateTime.Now, DateTime.Now, DateTime.Now);
                }
                objectBuilder.AddFile("PILOT_CONTENT_GRAPHIC_LAYER_ELEMENT_" + o.ContentId, memoryStream1, DateTime.Now, DateTime.Now, DateTime.Now);
                _modifier.Apply();
            }
        }

        public async void OnNext(INotification value)
        {
            if (string.IsNullOrEmpty(_filePath))
                return;
            _currentPerson = _repository.GetCurrentPerson();
            int? userId;
            int num1;
            if (value.ChangeKind == NotificationKind.ObjectSignatureChanged)
            {
                int id = _currentPerson.Id;
                userId = value.UserId;
                int valueOrDefault = userId.GetValueOrDefault();
                num1 = id == valueOrDefault & userId.HasValue ? 1 : 0;
            }
            else
                num1 = 0;
            if (num1 != 0)
            {
                ObjectLoader loaderForFirstSign = new ObjectLoader(_repository);
                IDataObject obj = await loaderForFirstSign.Load(value.ObjectId);
                if (!obj.Files.Any(f => f.Name.Contains("Signature")))
                    return;
                CheckPageNumber(obj);
            }
            else
            {
                int num2;
                if (value.ChangeKind == NotificationKind.ObjectFileChanged)
                {
                    int id = _currentPerson.Id;
                    userId = value.UserId;
                    int valueOrDefault = userId.GetValueOrDefault();
                    num2 = id == valueOrDefault & userId.HasValue ? 1 : 0;
                }
                else
                    num2 = 0;
                if (num2 == 0)
                    return;
                ObjectLoader loader = new ObjectLoader(_repository);
                IDataObject obj = await loader.Load(value.ObjectId);
                if (!obj.Files.Any(f => f.Name.Contains("Signature")))
                    return;
                CheckPageNumber(obj);
                loader = null;
                obj = null;
            }
        }

        //Сверка номера текущей страницы с номером страницы в настройках подписи:
        private void CheckPageNumber(IDataObject dataObject)
        {
            CheckSettings(); //чтение настроек подписи
            _currentPage = _xpsViewer.CurrentPageNumber + 1;  //определение текущей страницы
            if (_pageNumber != _currentPage)  //если отличается от номера страницы в настройках, вызываем окно
            {
                PageNumberDialogView pageNumberDialogView = new PageNumberDialogView(_currentPage); //в окно отправляем информацию о текущей странице _currentPage
                pageNumberDialogView.ShowDialog();
                if (!pageNumberDialogView.cancel)
                {
                    _pageNumber = pageNumberDialogView.pageNumber; //задаём новый номер страницы из окна
                    AddGraphicLayer(dataObject); //наносим подпись 
                }
            }
            else
                AddGraphicLayer(dataObject); //если номера совпали, просто наносим подпись
        }

        private void AddGraphicLayer(IDataObject dataObject)
        {
            foreach (IFile file in dataObject.Files)
            {
                if (file.Name.Equals("PILOT_GRAPHIC_LAYER_ELEMENT_" + ToGuid(_currentPerson.Id)))
                    _modifier.Edit(dataObject).RemoveFile(file.Id);
            }
            //далее идёт подсчёт количества подписей на документе, сверка этого количества
            //с количеством запросов на подпись и нанесение неких "штампов", если все согласовали
            int num1 = dataObject.Files.Count (f => f.Name.Contains("Signature"));
            IFile file1 = dataObject.ActualFileSnapshot.Files.FirstOrDefault (f =>
            {
                string extension = Path.GetExtension(f.Name);
                return extension != null && (extension.Equals(".xps") || extension.Equals(".dwfx"));
            });
            int? count = file1?.Signatures.Count;
            file1.Signatures.Count (f => _currentPerson.AllOrgUnits().Contains(f.PositionId) && f.Sign != null);
            int? nullable = count;
            int num2 = num1;
            if (nullable.GetValueOrDefault() == num2 & nullable.HasValue && _includeStamp)
            {
                string xamlObject1 = GraphicLayerElementCreator.CreateStamp1().ToString();
                SaveToDataBaseXaml(dataObject, xamlObject1, Guid.NewGuid());
                string xamlObject2 = GraphicLayerElementCreator.CreateStamp2().ToString();
                SaveToDataBaseXaml(dataObject, xamlObject2, Guid.NewGuid());
            }
            SaveToDataBaseRastr(dataObject);
        }

        public static Guid ToGuid(int value)
        {
            byte[] b = new byte[16];
            BitConverter.GetBytes(value).CopyTo(b, 0);
            return new Guid(b);
        }

        public static Guid ToGuidMultiple (int value, int number)
        {
            byte[] b = new byte[16];
            BitConverter.GetBytes(value).CopyTo(b, 0);
            BitConverter.GetBytes(number).CopyTo(b, 1);
            return new Guid(b);
        }

        public void Handle(UnloadedEventArgs message)
        {
        }
        
        public void OnError(Exception error)
        {
        }
        
        public void OnCompleted()
        {
        }
    }
           
}
