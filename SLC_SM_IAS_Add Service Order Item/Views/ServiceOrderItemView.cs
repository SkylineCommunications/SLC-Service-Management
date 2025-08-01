﻿namespace SLC_SM_IAS_Add_Service_Order_Item_1.Views
{
	using System;

	using DomHelpers.SlcServicemanagement;

	using Library;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	using SLC_SM_Common.API.ServiceManagementApi;

	public class ServiceOrderItemView : Dialog
	{
		public ServiceOrderItemView(IEngine engine) : base(engine)
		{
			Title = "Manage Service Order Item";

			int row = 0;
			AddWidget(new Label("Service Order Item Details") { Style = TextStyle.Heading }, row, 0);
			AddWidget(LblName, ++row, 0);
			AddWidget(TboxName, row, 1, 1, 2);
			AddWidget(ErrorName, row, 3);
			AddWidget(LblAction, ++row, 0);
			AddWidget(ActionType, row, 1, 1, 2);
			AddWidget(LblStartTime, ++row, 0);
			AddWidget(Start, row, 1, 1, 2);
			AddWidget(LblEndTime, ++row, 0);
			AddWidget(End, row, 1, 1, 2);
			AddWidget(IndefiniteTime, row, 3);

			AddWidget(new Label("Service Order Configuration Details") { Style = TextStyle.Heading }, ++row, 0);
			AddWidget(LblCategory, ++row, 0);
			AddWidget(Category, row, 1, 1, 2);
			AddWidget(LblSpecification, ++row, 0);
			AddWidget(Specification, row, 1, 1, 2);
			AddWidget(ErrorSpecification, row, 3);
			AddWidget(LblService, ++row, 0);
			AddWidget(Service, row, 1, 1, 2);
			AddWidget(ErrorService, row, 3);

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(BtnAdd, ++row, 1);
			AddWidget(BtnCancel, row, 2);
		}

		public enum ActionTypeEnum
		{
			Add,
			Modify,
			Delete,
			NoChange,
			Undefined,
		}

		public Label LblName { get; } = new Label("Label");

		public TextBox TboxName { get; } = new TextBox { Width = Defaults.WidgetWidth };

		public Label ErrorName { get; } = new Label(String.Empty);

		public Label LblAction { get; } = new Label("Action");

		public DropDown<ActionTypeEnum> ActionType { get; } = new DropDown<ActionTypeEnum>
		{
			Width = Defaults.WidgetWidth,
			Options = new[]
			{
				new Option<ActionTypeEnum>("Add", ActionTypeEnum.Add),
				new Option<ActionTypeEnum>("Delete", ActionTypeEnum.Delete),
				new Option<ActionTypeEnum>("Modify", ActionTypeEnum.Modify),
				new Option<ActionTypeEnum>("No Change", ActionTypeEnum.NoChange),
				new Option<ActionTypeEnum>("Undefined", ActionTypeEnum.Undefined),
			},
		};

		public Label LblCategory { get; } = new Label("Category");

		public DropDown<Models.ServiceCategory> Category { get; } = new DropDown<Models.ServiceCategory> { Width = Defaults.WidgetWidth };

		public Label LblSpecification { get; } = new Label("Service Specification");

		public DropDown<Models.ServiceSpecification> Specification { get; } = new DropDown<Models.ServiceSpecification> { Width = Defaults.WidgetWidth };

		public Label ErrorSpecification { get; } = new Label(String.Empty);

		public Label LblService { get; } = new Label("Service Reference");

		public DropDown<Models.Service> Service { get; } = new DropDown<Models.Service> { Width = Defaults.WidgetWidth, IsDisplayFilterShown = true };

		public Label ErrorService { get; } = new Label(String.Empty);

		public Label LblStartTime { get; } = new Label("Start Time");

		public DateTimePicker Start { get; } = new DateTimePicker
		{
			Height = 25,
			Width = Defaults.WidgetWidth,
			IsTimePickerVisible = true,
			Kind = DateTimeKind.Local,
			HasSpinnerButton = true,
			AutoCloseCalendar = true,
		};

		public Label LblEndTime { get; } = new Label("End Time");

		public DateTimePicker End { get; } = new DateTimePicker
		{
			Height = 25,
			Width = Defaults.WidgetWidth,
			IsTimePickerVisible = true,
			Kind = DateTimeKind.Local,
			HasSpinnerButton = true,
			AutoCloseCalendar = true,
		};

		public CheckBox IndefiniteTime { get; } = new CheckBox("Indefinite (no end time)") { IsChecked = false };

		public Button BtnAdd { get; } = new Button("Create") { Style = ButtonStyle.CallToAction };

		public Button BtnCancel { get; } = new Button("Cancel");
	}
}