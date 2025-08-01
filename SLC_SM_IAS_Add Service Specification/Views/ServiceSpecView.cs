namespace SLC_SM_IAS_Add_Service_Specification.Views
{
	using System;

	using Library;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ServiceSpecView : Dialog
	{
		public ServiceSpecView(IEngine engine) : base(engine)
		{
			Title = "Manage Service Specification";

			int row = 0;
			AddWidget(LblName, row, 0);
			AddWidget(TboxName, row, 1, 1, 2);
			AddWidget(ErrorName, row, 3);

			AddWidget(LblDescription, ++row, 0);
			AddWidget(Description, row, 1, 1, 2);

			AddWidget(LblIcon, ++row, 0);
			AddWidget(Icon, row, 1, 1, 2);

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(BtnAdd, ++row, 1);
			AddWidget(BtnCancel, row, 2);
		}

		public Label LblName { get; } = new Label("Label");

		public TextBox TboxName { get; } = new TextBox { Width = Defaults.WidgetWidth };

		public Label ErrorName { get; } = new Label(String.Empty);

		public Label LblIcon { get; } = new Label("Icon");

		public TextBox Icon { get; } = new TextBox { Width = Defaults.WidgetWidth };

		public Label LblDescription { get; } = new Label("Description");

		public TextBox Description { get; } = new TextBox { Width = Defaults.WidgetWidth };

		public Button BtnAdd { get; } = new Button("Create") { Style = ButtonStyle.CallToAction };

		public Button BtnCancel { get; } = new Button("Cancel");
	}
}