﻿namespace SLC_SM_IAS_Add_Service_Item_1.Views
{
	using System;
	using DomHelpers.SlcServicemanagement;
	using Library;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ServiceItemView : Dialog
	{
		public ServiceItemView(IEngine engine) : base(engine)
		{
			Title = "Manage Service Item";
			MinWidth = Defaults.DialogMinWidth;

			int row = 0;
			AddWidget(new Label("Service Item Details") { Style = TextStyle.Heading }, row, 0);
			AddWidget(LblLabel, ++row, 0);
			AddWidget(TboxLabel, row, 1);
			AddWidget(ErrorLabel, row, 2);

			AddWidget(LblServiceItemType, ++row, 0);
			AddWidget(ServiceItemType, row, 1);

			AddWidget(LblDefinitionReference, ++row, 0);
			AddWidget(DefinitionReferences, row, 1);

			AddWidget(LblScriptSelection, ++row, 0);
			AddWidget(ScriptSelection, row, 1);

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(BtnCancel, ++row, 0);
			AddWidget(BtnAdd, row, 1);
		}

		public Label LblLabel { get; } = new Label("Label");

		public TextBox TboxLabel { get; } = new TextBox { Width = Defaults.WidgetWidth };

		public Label ErrorLabel { get; } = new Label(String.Empty);

		public Button BtnCancel { get; } = new Button("Cancel");

		public Label LblServiceItemType { get; } = new Label("Service Item Type");

		public DropDown<SlcServicemanagementIds.Enums.ServiceitemtypesEnum> ServiceItemType { get; } = new DropDown<SlcServicemanagementIds.Enums.ServiceitemtypesEnum> { Width = Defaults.WidgetWidth };

		public Label LblDefinitionReference { get; } = new Label("Definition Reference");

		public DropDown DefinitionReferences { get; } = new DropDown { Width = Defaults.WidgetWidth };

		public Label LblScriptSelection { get; } = new Label("Service Item Script");

		public DropDown ScriptSelection { get; } = new DropDown { Width = Defaults.WidgetWidth };

		public Button BtnAdd { get; } = new Button("Add");
	}
}