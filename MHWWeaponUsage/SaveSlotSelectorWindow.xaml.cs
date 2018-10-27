using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MHWWeaponUsage.ViewModels;

namespace MHWWeaponUsage
{
    /// <summary>
    /// Interaction logic for SaveSlotSelectorWindow.xaml
    /// </summary>
    public partial class SaveSlotSelectorWindow : Window
    {
        private readonly SaveSlotSelectorViewModel saveSlotSelectorViewModel;

        public SaveSlotSelectorWindow(SaveSlotSelectorViewModel saveSlotSelectorViewModel)
        {
            InitializeComponent();

            this.saveSlotSelectorViewModel = saveSlotSelectorViewModel;

            saveSlotSelectorViewModel.ClearSelection();
            saveSlotSelectorViewModel.SelectionDone += SaveSlotSelectorViewModel_SelectionDone;

            DataContext = saveSlotSelectorViewModel;
        }

        private void SaveSlotSelectorViewModel_SelectionDone(object sender, EventArgs e)
        {
            DialogResult = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            saveSlotSelectorViewModel.SelectionDone -= SaveSlotSelectorViewModel_SelectionDone;
        }
    }
}
