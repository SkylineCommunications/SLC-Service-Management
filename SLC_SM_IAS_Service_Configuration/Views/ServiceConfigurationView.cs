﻿namespace SLC_SM_IAS_Service_Configuration.Views
{
	using Library;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ServiceConfigurationView : Dialog
	{
		public ServiceConfigurationView(IEngine engine) : base(engine)
		{
			Title = "Manage Service Configuration";
			MinWidth = Defaults.DialogMinWidth;
		}

		public Label TitleDetails { get; } = new Label("Service Configuration Details") { Style = TextStyle.Heading };

		public Button BtnUpdate { get; } = new Button("Update");

		public Button BtnCancel { get; } = new Button("Cancel");

		public Button BtnShowValueDetails { get; } = new Button("Show Value Details");

		public Section Details { get; } = new Section();
	}
}