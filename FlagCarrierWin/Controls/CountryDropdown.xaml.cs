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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.IO;

namespace FlagCarrierWin.Controls
{
	public class CountryDropdownItem
	{
		public string Text { get; set; }
		public string Code { get; set; }

		public override string ToString()
		{
			return Text;
		}
	}

	/// <summary>
	/// Interaction logic for CountryDropdown.xaml
	/// </summary>
	public partial class CountryDropdown : UserControl
	{
		Dictionary<string, CountryDropdownItem> codeToItem = new Dictionary<string, CountryDropdownItem>();

		private string code;
		public string Code
		{
			get
			{
				CountryDropdownItem item = (CountryDropdownItem)countryBox.SelectedItem;
				if (item == null)
					return "";
				return item.Code;
			}
			set
			{
				string uv = value.ToUpper();
				if (codeToItem.ContainsKey(uv))
					countryBox.SelectedItem = codeToItem[uv];
				else
					throw new Exception("Requested code does not exist in our list");
			}
		}

		public CountryDropdown()
		{
			InitializeComponent();

			XmlDocument doc = new XmlDocument();

			using (Stream stream = GetType().Assembly.GetManifestResourceStream("FlagCarrierWin.Resources.countries.xml"))
				doc.Load(stream);

			foreach(XmlNode xn in doc.SelectNodes("/countries/country"))
			{
				string cca2 = xn.Attributes["cca2"].Value;
				string names = xn.Attributes["name"].Value;

				CountryDropdownItem item = new CountryDropdownItem();
				item.Text = cca2 + " [" + cca2 + "]";
				item.Code = cca2;
				countryBox.Items.Add(item);

				foreach (string name in names.Split(','))
				{
					item = new CountryDropdownItem();
					item.Text = name + " [" + cca2 + "]";
					item.Code = cca2;
					countryBox.Items.Add(item);

					if (!codeToItem.ContainsKey(cca2))
						codeToItem.Add(cca2, item);
				}
			}

			Code = "DE";
		}

		private void countryBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			string t = countryBox.Text.ToUpper();
			if (codeToItem.ContainsKey(t))
				countryBox.SelectedItem = codeToItem[t];
		}

		private void countryBox_LostFocus(object sender, RoutedEventArgs e)
		{
			CountryDropdownItem item = (CountryDropdownItem)countryBox.SelectedItem;
			if (item == null)
				Code = "DE";
		}
	}
}
