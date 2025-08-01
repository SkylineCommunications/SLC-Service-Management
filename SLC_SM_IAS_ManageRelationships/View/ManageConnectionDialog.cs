namespace SLC_SM_IAS_ManageRelationships.View
{
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcWorkflow;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public partial class ManageConnectionDialog : Dialog
	{
		private readonly List<DialogRow> _dialogRows;
		private readonly ServiceItemLinkMapContext _context;

		public ManageConnectionDialog(IEngine engine, ServiceItemLinkMapContext context) : base(engine)
		{
			_context = context;
			_dialogRows = new List<DialogRow>();

			Title = $"{_context.Pair.SourceNode.DefinitionReference} ➡ {_context.Pair.DestinationNode.DefinitionReference}";

			SetButtonValidate(context.ShowNext);
			ButtonPrevious = new Button("Previous") { Width = 100, IsVisible = context.ShowPrevious };
			ButtonCancel = new Button("Cancel") { Width = 100 };

			Label sourceInterfacesLabel = new Label("Source Interface:");
			Label destinationInterfacesLabel = new Label("Destination Interface:");

			int row = 0;
			AddWidget(sourceInterfacesLabel, row, 0);
			AddWidget(destinationInterfacesLabel, row, 1);

			RenderInterfaces(ref row);

			AddWidget(new WhiteSpace { Height = 25 }, ++row, 0);
			AddWidget(ButtonPrevious, ++row, 0, HorizontalAlignment.Left);
			AddWidget(ButtonValidate, row, 1, HorizontalAlignment.Right);
			AddWidget(ButtonCancel, row, 2, HorizontalAlignment.Right);

			SetColumnWidth(0, 100);
			SetColumnWidth(1, 100);
		}

		public Button ButtonValidate { get; private set; }

		public Button ButtonCancel { get; private set; }

		public Button ButtonPrevious{ get; private set; }

		public void UpdatePairFromDialog()
		{
			_context.Pair.ClearLinks();

			foreach (var row in _dialogRows)
				UpdateLinkFromRow(row);
		}

		private void SetButtonValidate(bool showNext)
		{
			ButtonValidate = new Button(showNext ? "Next" : "Ok")
			{
				Width = 100,
				Style = showNext ? ButtonStyle.None : ButtonStyle.CallToAction,
			};
		}

		private void RenderInterfaces(ref int row)
		{
			foreach (var source in _context.Pair.AvailableSources)
			{
				var dropdown = CreateDestinationDropdown(source);
				AddInterfaceRow(source, dropdown, ref row);
			}
		}

		private DropDown<NodesSection> CreateDestinationDropdown(NodesSection source)
		{
			var options = GetDestinationOptions();

			var dropdown = new DropDown<NodesSection>(options);
			dropdown.Changed += Dropdown_Changed;

			var link = _context.Pair.FindLinkBySource(source.NodeID);

			var existingDestination = _context.Pair.AvailableDestinations
				.FirstOrDefault(d => d.NodeID == link?.ChildServiceItemInterfaceID);

			if (existingDestination != null)
				dropdown.Selected = existingDestination;

			return dropdown;
		}

		private List<Option<NodesSection>> GetDestinationOptions()
		{
			return new[]
			{
				new Option<NodesSection>("-None-", null),
			}
			.Concat(_context.Pair.AvailableDestinations
				.Select(d => new Option<NodesSection>(d.NodeAlias, d)))
			.ToList();
		}

		private void Dropdown_Changed(object sender, DropDown<NodesSection>.DropDownChangedEventArgs e)
		{
			var changedDropdown = (DropDown<NodesSection>)sender;
			var changedRow = FindRowByDropdown(changedDropdown);

			foreach (var row in _dialogRows)
			{
				if (row.Row == changedRow.Row)
					continue;

				UpdateDropdownOptions(row.DestinationInterfaces, e);
			}
		}

		private DialogRow FindRowByDropdown(DropDown<NodesSection> dropdown)
		{
			return _dialogRows.Single(r => r.DestinationInterfaces == dropdown);
		}

		private void UpdateDropdownOptions(DropDown<NodesSection> dropdown, DropDown<NodesSection>.DropDownChangedEventArgs e)
		{
			var options = GetDestinationOptions();

			if (e.Selected == null)
			{
				options.Add(e.PreviousOption);
			}
			else
			{
				options.Remove(e.SelectedOption);
			}

			dropdown.Options = options;
		}

		private void AddInterfaceRow(NodesSection source, DropDown<NodesSection> dropdown, ref int row)
		{
			AddWidget(new Label(source.NodeAlias), ++row, 0);
			AddWidget(dropdown, row, 1);

			_dialogRows.Add(new DialogRow
			{
				Row = row,
				SourceInterface = source,
				DestinationInterfaces = dropdown,
			});
		}

		private void UpdateLinkFromRow(DialogRow row)
		{
			var sourceInterface = row.SourceInterface.NodeID;
			var selectedDestination = row.DestinationInterfaces.Selected;

			_context.Pair.AddLink(sourceInterface, selectedDestination?.NodeID ?? string.Empty);
		}

		private class DialogRow
		{
			public int Row { get; set; }

			public NodesSection SourceInterface { get; set; }

			public DropDown<NodesSection> DestinationInterfaces { get; set; }
		}
	}
}
