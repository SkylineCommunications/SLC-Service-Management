namespace SLC_SM_IAS_Configurations.Views
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class TextOptionsView : Dialog
	{
		public TextOptionsView(IEngine engine) : base(engine)
		{
			Title = "Update Text Options";

			AddWidget(LblRegex, 0, 0);
			AddWidget(Regex, 0, 1, 1, 2);
			AddWidget(LblUserMessage, 1, 0);
			AddWidget(UserMessage, 1, 1, 1, 2);
			AddWidget(BtnApply, 2, 1);
			AddWidget(BtnReturn, 2, 2);
		}

		public Label LblRegex { get; } = new Label("Regex");

		public TextBox Regex { get; } = new TextBox();

		public Label LblUserMessage { get; } = new Label("User Message");

		public TextBox UserMessage { get; } = new TextBox();

		public Button BtnApply { get; } = new Button("Apply") { Style = ButtonStyle.CallToAction };

		public Button BtnReturn { get; } = new Button("Return");
	}
}