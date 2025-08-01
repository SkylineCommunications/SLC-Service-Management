namespace SLC_SM_IAS_Configurations.Views
{
	using System;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class DiscreteValuesView : Dialog
	{
		public DiscreteValuesView(IEngine engine) : base(engine)
		{
			Title = "Manage Eligible Discrete Options";
		}

		public TextBox Value { get; } = new TextBox();

		public Label ErrorValue { get; } = new Label(String.Empty);

		public Button BtnAddOption { get; } = new Button("Add Option");

		public Section Options { get; } = new Section();

		public Button BtnApply { get; } = new Button("Apply Updates") { Style = ButtonStyle.CallToAction };

		public Button BtnReturn { get; } = new Button("Return");
	}
}