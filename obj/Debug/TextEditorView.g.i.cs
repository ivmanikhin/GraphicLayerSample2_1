﻿#pragma checksum "..\..\TextEditorView.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "C14F461C51FE77FEAF3F6DA1C5675D772CEFF670FCD39DB55166161F161BB081"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Ascon.Pilot.SDK.GraphicLayerSample;
using Ascon.Pilot.Theme.Controls;
using Ascon.Pilot.Theme.Tools;
using GraphicLayerSample2_1.Properties;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Ascon.Pilot.SDK.GraphicLayerSample {
    
    
    /// <summary>
    /// TextEditorView
    /// </summary>
    public partial class TextEditorView : Ascon.Pilot.Theme.Controls.DialogWindow, System.Windows.Markup.IComponentConnector {
        
        
        #line 34 "..\..\TextEditorView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox inputText;
        
        #line default
        #line hidden
        
        
        #line 55 "..\..\TextEditorView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox inputFontSize;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/GraphicLayerSample2_1.ext2;component/texteditorview.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\TextEditorView.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.inputText = ((System.Windows.Controls.TextBox)(target));
            return;
            case 2:
            this.inputFontSize = ((System.Windows.Controls.TextBox)(target));
            
            #line 56 "..\..\TextEditorView.xaml"
            this.inputFontSize.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.FontSizeTextChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 76 "..\..\TextEditorView.xaml"
            ((System.Windows.Controls.RadioButton)(target)).Checked += new System.Windows.RoutedEventHandler(this.PrintedRadioBTNChecked);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 83 "..\..\TextEditorView.xaml"
            ((System.Windows.Controls.RadioButton)(target)).Checked += new System.Windows.RoutedEventHandler(this.HandWriteRadioBTNChecked);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 95 "..\..\TextEditorView.xaml"
            ((System.Windows.Controls.RadioButton)(target)).Checked += new System.Windows.RoutedEventHandler(this.BlackRadioBTNChecked);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 102 "..\..\TextEditorView.xaml"
            ((System.Windows.Controls.RadioButton)(target)).Checked += new System.Windows.RoutedEventHandler(this.NavyRadioBTNChecked);
            
            #line default
            #line hidden
            return;
            case 7:
            
            #line 117 "..\..\TextEditorView.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.OnOkButtonClicked);
            
            #line default
            #line hidden
            return;
            case 8:
            
            #line 122 "..\..\TextEditorView.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.OnCancelButtonClicked);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

