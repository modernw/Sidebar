using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SGInstall
{
	public partial class WizardPage: UserControl, IWizardPage
	{
		public WizardPage ()
		{
			InitializeComponent ();
		}
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public virtual string BackButtonTitle => Program.StringResources.SuitableResource ("INSTALLER_BTN_BACK", "< Back");
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public virtual bool CanBack => true;
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public virtual bool CanCancel => true;
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public virtual string CancelButtonTitle => Program.StringResources.SuitableResource ("INSTALLER_BTN_CANCEL", "Cancel");
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public virtual bool CanNext => true;
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public virtual string DisplayName => "Wizard Page";
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public virtual string Id => "wizard_page";
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public virtual string NextButtonTitle => Program.StringResources.SuitableResource ("INSTALLER_BTN_NEXT", "Next >");
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public virtual bool ShowBack => true;
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public virtual bool ShowCancel => true;
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public virtual bool ShowNext => true;
		[Browsable (false)]
		public IWizardPageContainer PageContainer { get; set; }
		public bool IsLoaded { get; internal set; }
		public virtual void OnAlreadyLoad () { }
		public virtual void OnAlreadyUnload () { }
		public virtual bool OnBackButtonClick (object sender, EventArgs e) => true;
		public virtual bool OnCancelButtonClick (object sender, EventArgs e) => true;
		public virtual bool OnNextButtonClick (object sender, EventArgs e) => true;
		public virtual bool OnWillLoad () => true;
		public virtual bool OnWillUnload () => true;
		public bool Send (string targetId, string name, object data, Type dataType)
		{
			return PageContainer?.Mail (Id, targetId, name, data, dataType) ?? false;
		}
		public bool Send<T> (string targetId, string name, T data) =>
			Send (targetId, name, data, typeof (T));
		public virtual bool Receive (string sourceId, string name, object data, Type dataType) { return true; }
	}
}
