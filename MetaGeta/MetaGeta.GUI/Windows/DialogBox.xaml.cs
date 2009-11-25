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
using MetaGeta.GUI.Services;

namespace MetaGeta.GUI.Windows {
    /// <summary>
    /// Interaction logic for DialogBox.xaml
    /// </summary>
    public partial class DialogBox : Window {
        public DialogBox() {
            InitializeComponent();
        }

        public void SetDialogButtons(DialogButtons buttons) {
            switch (buttons) {
                case DialogButtons.Ok:
                    btnCancel.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case DialogButtons.OkCancel:
                    break;
                case DialogButtons.Close:
                    btnCancel.Visibility = System.Windows.Visibility.Collapsed;
                    btnOK.Content = "Close";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("buttons");
            }
        }
    }
}
