using System.Security;
using System.Windows;

namespace FlagCarrierWin.Controls
{
	/// <summary>
	/// Interaction logic for PasswordDialog.xaml
	/// </summary>
	public partial class PasswordDialog : Window
	{
		public SecureString Password
		{
			get;
			private set;
		}

		public PasswordDialog()
		{
			InitializeComponent();
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			Password = pwBox.SecurePassword;
			Close();
		}
	}
}
