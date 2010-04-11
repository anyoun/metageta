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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MetaGeta.GUI
{
	/// <summary>
	/// Interaction logic for ProgressBadge.xaml
	/// </summary>
	public partial class ProgressBadge : UserControl
	{
		public ProgressBadge()
		{
			this.InitializeComponent();
		}

		/// <summary>
		/// The <see cref="BadgeCount" /> dependency property's name.
		/// </summary>
		public const string BadgeCountPropertyName = "BadgeCount";

		/// <summary>
		/// Gets or sets the value of the <see cref="BadgeCount" />
		/// property. This is a dependency property.
		/// </summary>
		public int BadgeCount {
			get {
				return (int)GetValue(BadgeCountProperty);
			}
			set {
				SetValue(BadgeCountProperty, value);
			}
		}

		/// <summary>
		/// Identifies the <see cref="BadgeCount" /> dependency property.
		/// </summary>
		public static readonly DependencyProperty BadgeCountProperty = DependencyProperty.Register(
			BadgeCountPropertyName,
			typeof(int),
			typeof(ProgressBadge),
			new UIPropertyMetadata(3));

		
		/// <summary>
		/// The <see cref="BadgeProgress" /> dependency property's name.
		/// </summary>
		public const string BadgeProgressPropertyName = "BadgeProgress";

		/// <summary>
		/// Gets or sets the value of the <see cref="BadgeProgress" />
		/// property. This is a dependency property.
		/// </summary>
		public double BadgeProgress {
			get {
				return (double)GetValue(BadgeProgressProperty);
			}
			set {
				SetValue(BadgeProgressProperty, value);
			}
		}

		/// <summary>
		/// Identifies the <see cref="BadgeProgress" /> dependency property.
		/// </summary>
		public static readonly DependencyProperty BadgeProgressProperty = DependencyProperty.Register(
			BadgeProgressPropertyName,
			typeof(double),
			typeof(ProgressBadge),
			new PropertyMetadata(.6));
	}
}