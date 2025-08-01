namespace Library.Views
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public sealed class ErrorView : Dialog
	{
		private const int ButtonHeight = 30;
		private const int ButtonWidth = 110;
		private const int DetailsButtonWidth = 30;
		private const int DetailsColumnWidth = 40;

		public ErrorView(IEngine engine, string title, string message, string detailsMessage) : base(engine)
		{
			MinWidth = 850;
			Title = title;

			var messageLabel = new Label { Text = message, MinWidth = 400, MaxWidth = 850 };
			DetailsBox.Text = detailsMessage;
			CloseButton.Pressed += OnCloseButtonPressed;
			DetailsButton.Pressed += OnDetailsButtonPressed;

			int row = 0;
			AddWidget(messageLabel, row, 0, 1, 2, HorizontalAlignment.Stretch, VerticalAlignment.Stretch);
			SetColumnWidth(0, DetailsColumnWidth);
			SetColumnWidthStretch(1);

			if (!String.IsNullOrWhiteSpace(detailsMessage))
			{
				AddWidget(new WhiteSpace(), ++row, 0);
				AddWidget(DetailsButton, ++row, 0, verticalAlignment: VerticalAlignment.Top);

				AddWidget(DetailsBox, ++row, 1, HorizontalAlignment.Stretch, VerticalAlignment.Stretch);

				UpdateDetailsButton();
			}

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(CloseButton, ++row, 1);
		}

		private Button CloseButton { get; } = new Button("Close") { Height = ButtonHeight, Width = ButtonWidth, Style = ButtonStyle.CallToAction };

		private TextBox DetailsBox { get; } = new TextBox { MaxWidth = 800, IsMultiline = true, IsVisible = false, MinHeight = 100, MaxHeight = 250 };

		private Button DetailsButton { get; } = new Button { Height = ButtonHeight, Width = DetailsButtonWidth };

		private static void OnCloseButtonPressed(object sender, EventArgs e)
		{
			throw new ScriptAbortException("close");
		}

		private void OnDetailsButtonPressed(object sender, EventArgs e)
		{
			DetailsBox.IsVisible = !DetailsBox.IsVisible;
			UpdateDetailsButton();
		}

		private void UpdateDetailsButton()
		{
			DetailsButton.Text = DetailsBox.IsVisible ? "-" : "+";
		}
	}
}