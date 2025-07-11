namespace SLC_SM_IAS_Add_Service_Item_1.Presenters
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcServicemanagement;
	using DomHelpers.SlcWorkflow;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.Utils.MediaOps.Common.IOData.Scheduling.Scripts.JobHandler;
	using Skyline.DataMiner.Utils.MediaOps.Helpers.Workflows;

	using SLC_SM_IAS_Add_Service_Item_1.Views;

	public class ServiceItemPresenter
	{
		private readonly DomInstance domInstance;
		private readonly IEngine engine;
		private readonly string[] getServiceItemLabels;
		private readonly ServiceItemView view;
		private readonly Workflow[] workflows;

		public ServiceItemPresenter(IEngine engine, ServiceItemView view, string[] getServiceItemLabels, DomInstance domInstance)
		{
			this.engine = engine;
			this.view = view;
			this.getServiceItemLabels = getServiceItemLabels;
			this.domInstance = domInstance;

			var workflowHelper = new WorkflowHelper(engine);
			workflows = workflowHelper.GetAllWorkflows().ToArray();

			view.TboxLabel.Changed += (sender, args) => ValidateLabel(args.Value);
			view.ServiceItemType.Changed += (sender, args) => OnUpdateServiceItemType(args.Selected);
			view.DefinitionReferences.Changed += (sender, args) => OnUpdateDefinitionReference(args.Selected);
		}

		public string Name => String.IsNullOrWhiteSpace(view.TboxLabel.Text) ? view.TboxLabel.PlaceHolder : view.TboxLabel.Text;

		public ServiceItemsSection Section => new ServiceItemsSection
		{
			Label = Name,
			ServiceItemType = view.ServiceItemType.Selected,
			DefinitionReference = view.DefinitionReferences.Selected ?? String.Empty,
			ServiceItemScript = view.ScriptSelection.Selected ?? String.Empty,
			ImplementationReference = String.Empty,
		};

		public string UpdateJobForWorkFlow(string label)
		{
			if (view.ServiceItemType.Selected != SlcServicemanagementIds.Enums.ServiceitemtypesEnum.Workflow)
			{
				return String.Empty;
			}

			var job = GetJobForOrder(domInstance, label);
			if (job == null)
			{
				return String.Empty;
			}

			var timings = GetServiceItemTimings(domInstance);
			var action = new EditJobAction
			{
				DomJobId = job.ID.Id,
				End = timings.Item2,
			};

			// Only add start update if the job is not already running
			if (job.JobInfo.JobStart <= DateTime.UtcNow)
			{
				action.Start = timings.Item1;
			}

			action.SendToJobHandler(engine, true);

			return job.ID.Id.ToString();
		}

		private void UpdateLabelPlaceholder(SlcServicemanagementIds.Enums.ServiceitemtypesEnum serviceItemType, string definitionReference)
		{
			string tboxLabelPlaceHolder = $"{serviceItemType}_{definitionReference}";
			while (getServiceItemLabels.Contains(tboxLabelPlaceHolder))
			{
				tboxLabelPlaceHolder += " (1)";
			}

			view.TboxLabel.PlaceHolder = tboxLabelPlaceHolder;
		}

		public void LoadFromModel()
		{
			// Load correct types
			view.ServiceItemType.SetOptions(
				new List<Option<SlcServicemanagementIds.Enums.ServiceitemtypesEnum>>
				{
					new Option<SlcServicemanagementIds.Enums.ServiceitemtypesEnum>(
						SlcServicemanagementIds.Enums.Serviceitemtypes.Workflow,
						SlcServicemanagementIds.Enums.ServiceitemtypesEnum.Workflow),
					new Option<SlcServicemanagementIds.Enums.ServiceitemtypesEnum>(
						SlcServicemanagementIds.Enums.Serviceitemtypes.SRMBooking,
						SlcServicemanagementIds.Enums.ServiceitemtypesEnum.SRMBooking),
				});
			OnUpdateServiceItemType(view.ServiceItemType.Selected);
		}

		public void LoadFromModel(ServiceItemsSection section)
		{
			// Load correct types
			LoadFromModel();

			view.BtnAdd.Text = "Edit Service Item";
			view.TboxLabel.Text = section.Label;

			if (section.ServiceItemType.HasValue)
			{
				view.ServiceItemType.Selected = section.ServiceItemType.Value;
				OnUpdateServiceItemType(section.ServiceItemType.Value);
			}

			if (!String.IsNullOrEmpty(section.DefinitionReference))
			{
				view.DefinitionReferences.Selected = section.DefinitionReference;
			}

			if (!String.IsNullOrEmpty(section.ServiceItemScript))
			{
				view.ScriptSelection.SetOptions(new[] { section.ServiceItemScript });
			}
		}

		public bool Validate()
		{
			bool ok = true;

			ok &= ValidateLabel(Name);

			return ok;
		}

		private static (DateTime?, DateTime?) GetServiceItemTimings(DomInstance domInstance)
		{
			if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.Services.Id)
			{
				var instance = new ServicesInstance(domInstance);
				return (instance.ServiceInfo.ServiceStartTime, instance.ServiceInfo.ServiceEndTime);
			}

			return (null, null);
		}

		private JobsInstance GetJobForOrder(DomInstance instance, string label)
		{
			var jobFilter = DomInstanceExposers.FieldValues.DomInstanceField(SlcWorkflowIds.Sections.JobInfo.JobDescription).Equal($"{instance.ID.Id} | {label}")
				.OR(DomInstanceExposers.FieldValues.DomInstanceField(SlcWorkflowIds.Sections.JobInfo.JobDescription).Equal($"{instance.ID.Id}|{label}"));

			var domHelper = new DomHelper(engine.SendSLNetMessages, SlcWorkflowIds.ModuleId);
			return domHelper.DomInstances.Read(jobFilter)
				.Select(x => new JobsInstance(x))
				.FirstOrDefault();
		}

		private void OnUpdateDefinitionReference(string selected)
		{
			if (String.IsNullOrEmpty(selected))
			{
				view.ScriptSelection.SetOptions(new List<string>());
				return;
			}

			UpdateLabelPlaceholder(view.ServiceItemType.Selected, selected);

			var el = engine.FindElement(selected);
			if (el == null)
			{
				view.ScriptSelection.SetOptions(new List<string>());
				return;
			}

			var scriptName = Convert.ToString(el.GetParameter(195));
			view.ScriptSelection.SetOptions(new[] { scriptName });
		}

		private void OnUpdateServiceItemType(SlcServicemanagementIds.Enums.ServiceitemtypesEnum serviceItemType)
		{
			if (serviceItemType == SlcServicemanagementIds.Enums.ServiceitemtypesEnum.Workflow)
			{
				var workflowOptions = workflows.Select(x => x.Name).OrderBy(x => x).ToList();
				view.DefinitionReferences.SetOptions(workflowOptions);
				if (workflowOptions.Exists(x => x == "Default"))
				{
					view.DefinitionReferences.Selected = "Default";
				}

				view.ScriptSelection.SetOptions(new List<string>());
				view.ScriptSelection.IsEnabled = false;
			}
			else
			{
				var bookingManagers = engine.FindElementsByProtocol("Skyline Booking Manager").Where(x => x.IsActive).Select(x => x.ElementName).ToArray();
				view.DefinitionReferences.SetOptions(bookingManagers);
				OnUpdateDefinitionReference(view.DefinitionReferences.Selected);
				view.ScriptSelection.IsEnabled = false;
			}

			UpdateLabelPlaceholder(serviceItemType, view.DefinitionReferences.Selected);
		}

		private bool ValidateLabel(string newValue)
		{
			if (String.IsNullOrWhiteSpace(newValue))
			{
				view.ErrorLabel.Text = "Placeholder will be used";
				return true;
			}

			if (getServiceItemLabels.Contains(newValue, StringComparer.InvariantCultureIgnoreCase))
			{
				view.ErrorLabel.Text = "Label already exists!";
				return false;
			}

			view.ErrorLabel.Text = String.Empty;
			return true;
		}
	}
}