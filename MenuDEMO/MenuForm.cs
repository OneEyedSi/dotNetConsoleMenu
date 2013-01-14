using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows.Forms;

namespace MenuDEMO
{
	public partial class MenuForm : Form
	{
		#region Nested Classes, Enums, etc ********************************************************

		private struct FormDetails
		{
			public Form Form;
			public string Description;

			public FormDetails(Form form, string description)
			{
				Form = form;
				Description = description;
			}
		}

		#endregion

		#region Data Members **********************************************************************

		private List<FormDetails> _formDetailsList = new List<FormDetails>();

		#endregion

		#region Constructors, Destructors / Finalizers and Dispose Methods ************************

		public MenuForm()
		{
			InitializeComponent();

			FormDetails[] formDetailsList
				= {
					new FormDetails(new Form1(), "Form 1"), 
					new FormDetails(new Form2(), "Form 2")
					};
			_formDetailsList.AddRange(formDetailsList);

			foreach (FormDetails formDetails in _formDetailsList)
			{
				this.FormList.Items.Add(formDetails.Description);
			}
			this.FormList.SelectedIndex = 0;
		}

		#endregion

		#region Properties ************************************************************************


		#endregion

		#region Public Methods ********************************************************************


		#endregion

		#region Event Handlers ********************************************************************

		private void SelectFormHandler(object sender, EventArgs e)
		{
			int indexOfFormToRun = this.FormList.SelectedIndex;
			Form formToRun = _formDetailsList[indexOfFormToRun].Form;
			formToRun.ShowDialog();
		}

		private void CloseButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		#endregion

		#region Private and Protected Methods *****************************************************


		#endregion
	}
}