namespace SLC_SM_IAS_Configurations.Views
{
	using Library;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ConfigurationView : Dialog
	{
		public ConfigurationView(IEngine engine) : base(engine)
		{
			Title = "Manage Characteristics";
			MinWidth = Defaults.DialogMinWidth;
		}

		public Button BtnUpdate { get; } = new Button("Save Changes") { Style = ButtonStyle.CallToAction };

		public Button BtnCancel { get; } = new Button("Cancel Changes");
	}
}