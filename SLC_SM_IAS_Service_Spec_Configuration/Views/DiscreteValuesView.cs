namespace SLC_SM_IAS_Service_Spec_Configuration.Views
{
	using DomHelpers.SlcConfigurations;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class DiscreteValuesView : Dialog
	{
		public DiscreteValuesView(IEngine engine) : base(engine)
		{
			Title = "Select Eligible Discrete Options";

			AddWidget(Options, 0, 0);
			AddWidget(BtnApply, 1, 0);
		}

		public CheckBoxList<DiscreteValuesInstance> Options { get; } = new CheckBoxList<DiscreteValuesInstance>();

		public Button BtnApply { get; } = new Button("Apply Selection");
	}
}