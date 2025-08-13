namespace SLC_SM_IAS_Service_Spec_Configuration.Views
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations;

	public class DiscreteValuesView : Dialog
	{
		public DiscreteValuesView(IEngine engine) : base(engine)
		{
			Title = "Select Eligible Discrete Options";

			AddWidget(Options, 0, 0);
			AddWidget(BtnApply, 1, 0);
		}

		public CheckBoxList<Models.DiscreteValue> Options { get; } = new CheckBoxList<Models.DiscreteValue>();

		public Button BtnApply { get; } = new Button("Apply Selection") { Style = ButtonStyle.CallToAction };
	}
}