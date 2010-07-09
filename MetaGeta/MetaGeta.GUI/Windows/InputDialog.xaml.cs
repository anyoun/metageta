using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MetaGeta.GUI {
	/// <summary>
	/// Interaction logic for InputDialog.xaml
	/// </summary>
	public partial class InputDialog : Window {
		public InputDialog() {
			this.InitializeComponent();

			// Insert code required on object creation below this point.
		}

		public bool? ShowDialog(string title, string message, ref string text) {
			this.textBox1.Text = text;
			this.Title = title;
			this.label1.Content = message;
			var result = this.ShowDialog();
			if (result ?? false) {
				text = this.textBox1.Text;
			}
			return result;
		}

		private void btnOK_Click(object sender, RoutedEventArgs e) {
			this.DialogResult = true;
			this.Close();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e) {
			this.DialogResult = false;
			this.Close();
		}
	}
}