// Decompiled with JetBrains decompiler
// Type: Ascon.Pilot.SDK.GraphicLayerSample.GraphicLayerElementSettingsModel
// Assembly: Ascon.Pilot.SDK.GraphicLayerSample.ext2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 187B3BB9-3768-4B7C-861E-6A56C03BF53E
// Assembly location: D:\Projects\Pilot-ICE\SDK\b396a650-48de-48bb-bf68-8ed251a97fbe\Ascon.Pilot.SDK.GraphicLayerSample.ext2.dll

using Ascon.Pilot.SDK.GraphicLayerSample.Properties;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Ascon.Pilot.SDK.GraphicLayerSample
{
  public class GraphicLayerElementSettingsModel : INotifyPropertyChanged
  {
    private readonly DelegateCommand _selectImageCommand;
    private string _filePath;
    private bool _includeStamp;

    public string FilePath
    {
      get => this._filePath;
      set
      {
        this._filePath = value;
        this.OnPropertyChanged(nameof (FilePath));
      }
    }

    public bool IncludeStamp
    {
      get => this._includeStamp;
      set
      {
        this._includeStamp = value;
        this.OnPropertyChanged(nameof (IncludeStamp));
      }
    }

    public string XOffsetStr { get; set; }

    public string YOffsetStr { get; set; }

    public string Scale { get; set; }

    public string Angle { get; set; }

    public string PageNumber { get; set; }

    public ICommand SelectImageCommand => (ICommand) this._selectImageCommand;

    public bool LeftBottomCornerButtonChecked { get; set; }

    public bool LeftTopCornerButtonChecked { get; set; }

    public bool RightTopCornerButtonChecked { get; set; }

    public bool RightBottomCornerButtonChecked { get; set; }

    public event EventHandler OnSaveSettings;

    public GraphicLayerElementSettingsModel(
      string filePath,
      string x,
      string y,
      string scale,
      string angle,
      VerticalAlignment verticalAlignment,
      HorizontalAlignment horizontalAlignment,
      bool includeStamp,
      string pageNumber)
    {
      this.FilePath = filePath;
      this.XOffsetStr = x;
      this.YOffsetStr = y;
      this.Scale = scale;
      this.Angle = angle;
      this.IncludeStamp = includeStamp;
      this.PageNumber = pageNumber;
      if (verticalAlignment == VerticalAlignment.Top && horizontalAlignment == HorizontalAlignment.Left)
        this.LeftTopCornerButtonChecked = true;
      if (verticalAlignment == VerticalAlignment.Bottom && horizontalAlignment == HorizontalAlignment.Left)
        this.LeftBottomCornerButtonChecked = true;
      if (verticalAlignment == VerticalAlignment.Top && horizontalAlignment == HorizontalAlignment.Right)
        this.RightTopCornerButtonChecked = true;
      if (verticalAlignment == VerticalAlignment.Bottom && horizontalAlignment == HorizontalAlignment.Right)
        this.RightBottomCornerButtonChecked = true;
      this._selectImageCommand = new DelegateCommand(new Action(this.ShowDialog));
    }

    private void ShowDialog()
    {
      OpenFileDialog openFileDialog1 = new OpenFileDialog();
      openFileDialog1.DefaultExt = ".png";
      openFileDialog1.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg";
      OpenFileDialog openFileDialog2 = openFileDialog1;
      bool? nullable = openFileDialog2.ShowDialog();
      bool flag = true;
      if (!(nullable.GetValueOrDefault() == flag & nullable.HasValue))
        return;
      this.FilePath = openFileDialog2.FileName;
    }

    public void SaveSettings(
      string path,
      string xOffset,
      string yOffset,
      string scale,
      string angle,
      string pageNumber,
      VerticalAlignment vertical,
      HorizontalAlignment horizontal,
      bool includeStamp)
    {
      this.FilePath = path;
      this.XOffsetStr = xOffset;
      this.YOffsetStr = yOffset;
      this.Scale = scale;
      this.Angle = angle;
      this.PageNumber = pageNumber;
      Settings.Default.Path = path;
      Settings.Default.X = this.XOffsetStr;
      Settings.Default.Y = this.YOffsetStr;
      Settings.Default.Scale = this.Scale;
      Settings.Default.Angle = this.Angle;
      Settings.Default.PageNumber = this.PageNumber;
      Settings.Default.VerticalAligment = vertical.ToString();
      Settings.Default.HorizontalAligment = horizontal.ToString();
      Settings.Default.IncludeStamp = includeStamp;
      Settings.Default.Save();
      EventHandler onSaveSettings = this.OnSaveSettings;
      if (onSaveSettings == null)
        return;
      onSaveSettings((object) this, EventArgs.Empty);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
      if (propertyChanged == null)
        return;
      propertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
