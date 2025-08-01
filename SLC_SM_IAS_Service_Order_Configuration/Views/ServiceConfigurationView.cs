namespace SLC_SM_IAS_Service_Order_Configuration.Views
{
	using Library;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ServiceConfigurationView : Dialog
	{
		public ServiceConfigurationView(IEngine engine) : base(engine)
		{
			Title = "Manage Service Order Item Configuration";
			MinWidth = Defaults.DialogMinWidth;
		}

		public Label TitleDetails { get; } = new Label("Service Configuration Details") { Style = TextStyle.Heading };

		public Button BtnUpdate { get; } = new Button("Update") { Style = ButtonStyle.CallToAction };

		public Button BtnCancel { get; } = new Button("Cancel");

		public Button BtnShowValueDetails { get; } = new Button("Show Value Details");

		public Button BtnShowLifeCycleDetails { get; } = new Button("Show Lifecycle Details");

		public Section Details { get; } = new Section();

		public Section LifeCycleDetails { get; } = new Section();
	}
}