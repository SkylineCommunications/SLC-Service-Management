namespace SLC_SM_IAS_Add_Service_Property_1.Views
{
	using System;
	using DomHelpers.SlcServicemanagement;
	using Library;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ServicePropertyView : Dialog
	{
		public ServicePropertyView(IEngine engine) : base(engine)
		{
			Title = "Manage Service Property";

			int row = 0;
			AddWidget(LblServicePropertyName, row, 0);
			AddWidget(ServiceProperty, row, 1, 1, 2);

			AddWidget(LblValue, ++row, 0);
			AddWidget(TBoxValue, row, 1, 1, 2);
			AddWidget(ErrorValue, row, 3);
			AddWidget(DdValue, ++row, 1, 1, 2);

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(BtnAdd, ++row, 1);
			AddWidget(BtnCancel, row, 2);
		}

		public Label LblServicePropertyName { get; } = new Label("Service Property");

		public DropDown<ServicePropertiesInstance> ServiceProperty { get; } = new DropDown<ServicePropertiesInstance> { Width = Defaults.WidgetWidth };

		public Label LblValue { get; } = new Label("Value");

		public TextBox TBoxValue { get; } = new TextBox { Width = Defaults.WidgetWidth };

		public DropDown DdValue { get; } = new DropDown { Width = Defaults.WidgetWidth };

		public Label ErrorValue { get; } = new Label(String.Empty);

		public Button BtnAdd { get; } = new Button("Create") { Style = ButtonStyle.CallToAction };

		public Button BtnCancel { get; } = new Button("Cancel");
	}
}