using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NirIdentifier.Offline
{
    /// <summary>
    /// ModelSelector.xaml 的交互逻辑
    /// </summary>
    public partial class ModelSelector : Window
    {
        private Common.ModelInfo selectedModel = null;
        public ModelSelector()
        {
            InitializeComponent();
            listModels.SetEditable(false);
            btnOkCancel.OKEanbled = false;
        }

        private void listModels_ModelSelected(object sender, RoutedEventArgs e)
        {
            Ai.Hong.CommonLibrary.SelectChangedArgs arg = e as Ai.Hong.CommonLibrary.SelectChangedArgs;
            if (arg == null || arg.item == null)
                return;

            selectedModel = arg.item as Common.ModelInfo;

            btnOkCancel.OKEanbled = (selectedModel != null);
        }

        public Common.ModelInfo GetSelectedModel()
        {
            return selectedModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Common.CommonMethod.HideWindowSystemButton(this);
        }

        private void btnOkCancel_CancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void btnOkCancel_OKClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }
    }
}
