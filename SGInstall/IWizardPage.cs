using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SGInstall
{
	public interface IWizardPage
	{
		bool IsLoaded { get; }
		string Id { get; }
		string DisplayName { get; }
		bool CanBack { get; }
		bool ShowBack { get; }
		bool CanNext { get; }
		bool ShowNext { get; }
		bool CanCancel { get; }
		bool ShowCancel { get; }
		string BackButtonTitle { get; }
		string NextButtonTitle { get; }
		string CancelButtonTitle { get; }
		bool OnWillLoad ();
		void OnAlreadyLoad ();
		bool OnWillUnload ();
		void OnAlreadyUnload ();
		bool OnBackButtonClick (object sender, EventArgs e);
		bool OnNextButtonClick (object sender, EventArgs e);
		bool OnCancelButtonClick (object sender, EventArgs e);
		IWizardPageContainer PageContainer { set; }
		bool Send (string targetId, string name, object data, Type dataType);
		bool Receive (string sourceId, string name, object data, Type dataType);
	}
	public interface IWizardPageContainer
	{
		void Next ();
		void Back ();
		void Jump (string id);
		void Jump (int index);
		void Cancel ();
		bool Mail (string srcId, string tgId, string name, object data, Type dataType);
	}
}
